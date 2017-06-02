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

            commandText.AppendLine("SELECT * FROM Project WHERE ProjectID = @ProjectID;");
            commandText.AppendLine("SELECT * FROM ProjectContact WHERE ProjectID = @ProjectID;");
            commandText.AppendLine("SELECT * FROM ProjectTask WHERE ProjectID = @ProjectID ORDER BY TaskNumber;");
            commandText.AppendLine("SELECT * FROM ProjectAsset WHERE ProjectID = @ProjectID ORDER BY [Path];");
            commandText.AppendLine("SELECT * FROM ProjectSetting WHERE ProjectID = @ProjectID ORDER BY [TypeName]");

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

        public async Task<IEnumerable<Guid>> GetProjectsToPollForChanges()
        {
            this.EnsureOpenConnection();

            // Get a list of polled projects not already in the queue
            return await this.Connection.QueryAsync<Guid>(@"
                SELECT ProjectID 
                FROM Project
                WHERE TriggerMethod = @TriggerMethod AND IsActive = 1
                    AND NOT EXISTS (SELECT 1 FROM BuildQueue 
                        WHERE ProjectID = Project.ProjectID AND [Status] IN (1,2)
                        )
                ORDER BY Name;", 
                param: new
                {
                    TriggerMethod = (byte)BuildTriggerMethod.Polling
                }
            );
        }

        public async Task<BuildQueueEntry> GetMostRecentBuildAsync(Guid projectIdentifier)
        {
            this.EnsureOpenConnection();

            return await this.Connection.QuerySingleOrDefaultAsync<BuildQueueEntry>(@"
                SELECT TOP 1 *
                FROM [BuildQueue]
                WHERE [ProjectID] = @ProjectID
                ORDER BY [CreateDateTime] DESC
            ", param: new
            {
                ProjectID = projectIdentifier
            });
        }

        public async Task SetBuildResultAsync(Guid buildQueueID, BuildResult result)
        {
            this.EnsureOpenConnection();

            try
            {
                if (result.Success)
                {
                    await this.Connection.ExecuteAsync(@"
                    UPDATE BuildQueue
                    SET CompleteDateTime = @Now,
                    Status = 4,
                    LastResultCode = 0,
                    LastResultDateTime = @Now
                    WHERE BuildQueueID = @BuildQueueID;
                ", param: new
                    {
                        BuildQueueID = buildQueueID,
                        Now = DateTimeOffset.Now
                    });

                    //TODO: UPdate NextBuildNumber?
                }
                else if (result.ShouldRetry)
                {
                    await this.Connection.ExecuteAsync(@"
                    UPDATE BuildQueue
                    SET Status = 1
                    LastResultCode = @StatusCode,
                    FailCount = FailCount + 1,
                    RetryAfterDateTime = @RetryDateTime,
                    LastResultDateTime = @Now
                    WHERE BuildQueueID = @BuildQueueID;
                ", param: new
                    {
                        BuildQueueID = buildQueueID,
                        result.StatusCode,
                        Now = DateTimeOffset.Now,
                        RetryDateTime = DateTimeOffset.Now.AddMinutes(1)
                    });
                }
                else
                {
                    await this.Connection.ExecuteAsync(@"
                    UPDATE BuildQueue
                    SET Status = 4
                    CompleteDateTime = @Now,
                    LastResultCode = @StatusCode,
                    FailCount = FailCount + 1,
                    LastResultDateTime = @Now
                    WHERE BuildQueueID = @BuildQueueID;
                ", param: new
                    {
                        BuildQueueID = buildQueueID,
                        result.StatusCode,
                        Now = DateTimeOffset.Now
                    });
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public async Task<IEnumerable<BuildQueueEntry>> DequeueBuilds(string agentIdentifier)
        {
            this.EnsureOpenConnection();

            return await this.Connection.QueryAsync<BuildQueueEntry>(@"
                UPDATE BuildQueue
                SET [Status] = 2, 
                    [LockExpiresDateTime] = @LockExpiresDateTime
                OUTPUT inserted.*
                FROM BuildQueue 
                WHERE AssignedAgentID = @AgentID 
                    AND [Status] = 1
                    AND ([FailCount] = 0 OR [RetryAfterDateTime] < SYSDATETIMEOFFSET())
                ;",
                param: new
            {
                AgentID = agentIdentifier,
                LockExpiresDateTime = DateTimeOffset.Now.AddMinutes(1)
            });
        }

        public async Task<Guid> EnqueueBuild(BuildQueueEntry entry)
        {
            this.EnsureOpenConnection();

            if (entry.BuildQueueID == Guid.Empty)
            {
                entry.BuildQueueID = Guid.NewGuid();
            }

            // if a build already exists in the queue for the same project with status 1 (queued)
            //      update the revision ident to the newer value
            // else if the lock is expired on a status 2 record
            //      reset the lock to null
            //      reset the status to 1
            //      update the revision ident to the newer value
            // else if the lock is not expired
            //      insert a new build queue record

            var findAgentCmd = @"
                SELECT TOP 1 A.AgentID
                FROM Project P 
                    INNER JOIN Agent A ON A.EnvironmentID = P.EnvironmentID
                WHERE P.ProjectID = @ProjectID;   
            ";

            var batchCmd = @"
                IF (EXISTS (SELECT 1 FROM BuildQueue WHERE [Status]=1 AND ProjectID=@ProjectID)) BEGIN
                    UPDATE BuildQueue
                    SET RevisionIdentifier = @RevisionIdentifier
                    WHERE ProjectID = @ProjectID
                        AND [Status] = 1;
                END ELSE IF (EXISTS (SELECT 1 FROM BuildQueue WHERE [Status]=2 AND ProjectID = @ProjectID AND [LockExpiresDateTime] < SYSDATETIMEOFFSET())) BEGIN
                    UPDATE BuildQueue
                    SET [LockExpiresDateTime] = NULL,
                        [Status] = 1,
                        [LastResultCode] = 3,
                        [FailCount] = [FailCount] + 1
                    WHERE [ProjectID] = @ProjectID
                        AND [Status] = 2;
                END ELSE BEGIN
                    INSERT INTO BuildQueue ([BuildQueueID],[ProjectID],[RevisionIdentifier],[BuildNumber],[CreateDateTime],[AssignedDateTime],[AssignedAgentID],[Status])
                    VALUES (@BuildQueueID, @ProjectID, @RevisionIdentifier, @BuildNumber, @CreateDateTime, @AssignedDateTime, @AssignedAgentID, @Status);
                END
            ";

            using (var transaction = this.Connection.BeginTransaction(IsolationLevel.Serializable))
            {
                var agentID = await this.Connection.QuerySingleOrDefaultAsync<Guid?>(findAgentCmd, param: new
                {
                    ProjectID = entry.ProjectID
                }, transaction: transaction);
                
                if (!agentID.HasValue)
                {
                    throw new Exception($"No agent could be found to assign for project {entry.ProjectID}.");
                }

                // Get the first agent assigned to the environment for the given project
                var batchResult = await this.Connection.ExecuteAsync(batchCmd, param: new
                {
                    entry.BuildQueueID,
                    entry.ProjectID,
                    entry.RevisionIdentifier,
                    BuildNumber = entry.BuildNumber,
                    CreateDateTime = DateTimeOffset.Now,
                    AssignedDateTime = DateTimeOffset.Now,
                    AssignedAgentID = agentID,
                    Status = 1
                }, transaction: transaction);

                transaction.Commit();

                return entry.BuildQueueID;
            }
        }

        public async Task WriteLogMessageAsync(Guid buildIdentifier, MessageSeverity severity, int messageNumber, string message)
        {
            this.EnsureOpenConnection();

            await this.Connection.ExecuteAsync(@"
                INSERT INTO BuildLog(BuildQueueID, MessageNumber, Severity, Message, [DateTime])
                VALUES(@BuildQueueID, @MessageNumber, @Severity, @Message, @DateTime);
            ", param: new
            {
                BuildQueueID = buildIdentifier,
                Severity = severity,
                MessageNumber = messageNumber,
                Message = message,
                DateTime = DateTimeOffset.UtcNow
            });
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
