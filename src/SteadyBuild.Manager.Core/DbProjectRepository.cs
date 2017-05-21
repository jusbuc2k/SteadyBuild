using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;
using System.Data;
using System.Data.Common;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyBuild.Manager
{
    public class DbProjectRepository : IProjectRepository, IBuildQueue, IDisposable
    {
        public DbProjectRepository(IDbConnection connection)
        {
            this.Connection = connection;
        }

        protected IDbConnection Connection { get; private set; }

        protected void EnsureOpenConnection()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }

        public async Task AddProjectAsync(BuildProjectConfiguration project)
        {
            this.EnsureOpenConnection();

            var projectData = new DbProjectRecord(project);

            using (var transaction = this.Connection.BeginTransaction())
            {
                await this.Connection.InsertAsync(projectData, transaction);

                if (project.Contacts != null && project.Contacts.Count() > 0)
                {
                    foreach (var contact in project.Contacts)
                    {
                        await this.Connection.InsertAsync(new DbProjectContactRecord(projectData.ProjectID, contact), transaction);
                    }
                }

                if (project.AssetPaths != null && project.AssetPaths.Count() > 0)
                {
                    foreach (var asset in project.AssetPaths)
                    {
                        await this.Connection.InsertAsync(new DbProjectAssetRecord(projectData.ProjectID, asset), transaction);
                    }
                }                

                if (project.RepositorySettings != null)
                {
                    foreach (var keyPair in project.RepositorySettings)
                    {
                        await  this.Connection.InsertAsync(new DbProjectSettingRecord()
                        {
                            ProjectID = projectData.ProjectID,
                            TypeName = "Repository",
                            Name = keyPair.Key,
                            Value = keyPair.Value
                        }, transaction);
                    }
                }

                if (project.Variables != null)
                {
                    foreach (var keyPair in project.Variables)
                    {
                        await this.Connection.InsertAsync(new DbProjectSettingRecord()
                        {
                            ProjectID = projectData.ProjectID,
                            TypeName = "Variable",
                            Name = keyPair.Key,
                            Value = keyPair.Value
                        }, transaction);
                    }
                }

                if (project.Tasks != null)
                {
                    for (int taskNo = 1; taskNo <= project.Tasks.Count; taskNo++)
                    {
                        await this.Connection.InsertAsync(new DbProjectTaskRecord(projectData.ProjectID, project.Tasks[taskNo-1], taskNo));
                    }
                }

                transaction.Commit();
            }
        }

        public async Task UpdateProjectAsync(BuildProjectConfiguration project)
        {
            this.EnsureOpenConnection();

            var projectData = new DbProjectRecord(project);

            using (var transaction = this.Connection.BeginTransaction())
            {
                await this.Connection.UpdateAsync(projectData, transaction);

                if (project.Contacts == null || project.Contacts.Count == 0)
                {
                    await this.Connection.ExecuteAsync("DELETE FROM ProjectContact WHERE ProjectID=@ProjectID", transaction: transaction, param: new
                    {
                        ProjectID = project.ProjectID
                    });
                }

                transaction.Commit();
            }
        }

        public async Task DeleteProjectAsync(Guid projectID)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildProjectConfiguration> GetProject(Guid projectIdentifier)
        {
            this.EnsureOpenConnection();

            var param = new
            {
                ProjectID = projectIdentifier
            };

            var commandText = new StringBuilder();

            commandText.AppendLine("SELECT * FROM Project WHERE ProjectID = ?;");
            commandText.AppendLine("SELECT * FROM ProjectContact WHERE ProjectID = ?;");
            commandText.AppendLine("SELECT * FROM ProjectTask WHERE ProjectID = ? ORDER BY TaskNumber;");
            commandText.AppendLine("SELECT * FROM ProjectAsset WHERE ProjectID = ? ORDER BY [Path];");
            commandText.AppendLine("SELECT * FROM ProjectSetting WHERE ProjectID = ? ORDER BY [TypeName]");

            var result = await this.Connection.QueryMultipleAsync(commandText.ToString(), param: param);

            var project = result.ReadSingle<DbProjectRecord>();
            var contacts = result.Read<DbProjectContactRecord>();
            var tasks = result.Read<DbProjectTaskRecord>();
            var assets = result.Read<DbProjectAssetRecord>();
            var settings = result.Read<DbProjectSettingRecord>();

            var bp = new BuildProjectConfiguration();

            bp.ProjectID = project.ProjectID;
            bp.Name = project.Name;
            bp.IsActive = project.IsActive;
            bp.EnvironmentID = project.EnvironmentID;
            bp.NotifyTheAuthor = project.NotifyTheAuthor;
            bp.PraiseTheAuthor = project.PraiseTheAuthor;
            bp.RepositoryPath = project.RepositoryPath;
            bp.RepositoryType = project.RepositoryType;
            bp.MaxFailureCount = project.MaxFailureCount;

            bp.AssetPaths = assets.Select(s => s.Path);

            bp.Contacts = new List<BuildProjectContact>(contacts.Select(s => new BuildProjectContact()
            {
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                NotifyOnFailure = s.NotifyOnFailure,
                NotifyOnSuccess = s.NotifyOnSuccess,
                RepositoryUsername = s.RepositoryUsername,
                ProjectContactID = s.ProjectContactID
            }));


            bp.RepositorySettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bp.Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var setting in settings)
            {
                if (setting.TypeName == "Repository")
                {
                    bp.RepositorySettings.Add(setting.Name, setting.Value);
                }
                else if (setting.TypeName == "Variable")
                {
                    bp.Variables.Add(setting.Name, setting.Value);
                }
                else
                {
                    throw new NotSupportedException($"An unsupported setting type {setting.TypeName} was detected.");
                }
            }

            bp.Tasks = new List<BuildTask>(tasks.Select(s => new BuildTask(s.Expression)
            {
                SuccessfulExitCodes = s.SuccessExitCodes.Split(',').Select(c => int.Parse(c)),
            }));

            return bp;
        }

        public async Task<IEnumerable<Guid>> GetProjectsByTriggerMethodAsync(BuildTriggerMethod method)
        {
            this.EnsureOpenConnection();

            return await this.Connection.QueryAsync<Guid>(@"
                SELECT ProjectID 
                FROM Projects 
                WHERE TriggerMethod = @Method AND IsActive = 1
                ORDER BY Name;", 
                param: new
                {
                    TriggerMethod = (byte)method
                }
            );
        }

        public Task<BuildProjectState> GetProjectState(Guid projectIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task SetProjectState(Guid projectIdentifier, BuildProjectState state)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BuildQueueEntry>> DequeueBuilds(string agentIdentifier)
        {
            this.EnsureOpenConnection();

            var queue = await this.Connection.QueryAsync<dynamic>("SELECT ProjectID,CreateDateTime,BuildQueueID FROM BuildQueue WHERE AssignedAgentID = @AgentID AND [Status] = 1;", param: new
            {
                AgentID = agentIdentifier
            });

            var projects = new List<BuildQueueEntry>();

            foreach (var entry in queue)
            {
                var state = await this.GetProjectState((Guid)entry.ProjectID);

                projects.Add(new BuildQueueEntry()
                {
                    BuildIdentifier = (Guid)entry.BuildQueueID,
                    EnqueueDateTime = (DateTimeOffset)entry.CreateDateTime,
                    ProjectIdentifier = (Guid)entry.ProjectID,
                    RevisionIdentifier = (string)entry.RevisionIdentifier
                });
            }

            return projects;
        }

        public async Task<Guid> EnqueBuild(BuildQueueEntry entry)
        {
            this.EnsureOpenConnection();

            var agentQuery = @"
                SELECT TOP 1 A.AgentID, P.NextBuildNumber
                FROM Project P 
                    INNER JOIN Agent A ON A.EnvironmentID = P.EnvironmentID
                WHERE P.ProjectID = @ProjectID;";

            using (var transaction = this.Connection.BeginTransaction())
            {
                // Get the first agent assigned to the environment for the given project
                var projectData = await this.Connection.QuerySingleOrDefaultAsync<dynamic>(agentQuery, param: new
                {
                    ProjectID = entry.ProjectIdentifier
                }, transaction: transaction);

                if (projectData == null)
                {
                    throw new Exception("No agent is available for the given environment.");
                }

                var queueRecord = new DbBuildQueueRecord()
                {
                    BuildQueueID = Guid.NewGuid(),
                    ProjectID = entry.ProjectIdentifier,
                    RevisionIdentifier = entry.RevisionIdentifier,
                    CreateDateTime = entry.EnqueueDateTime,
                    BuildNumber = projectData.NextBuildNumber,
                    AssignedDateTime = DateTimeOffset.UtcNow,
                    AssignedAgentID = projectData.AgentID,
                };

                await this.Connection.InsertAsync(queueRecord, transaction: transaction);

                transaction.Commit();

                return queueRecord.BuildQueueID;
            }
        }

        public Task<IBuildOutputWriter> GetMessageWriterAsync(Guid projectIdentifier, Guid buildIdentifier)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this.Connection.State != ConnectionState.Closed)
            {
                this.Connection.Close();
            }
        }
    }
}
