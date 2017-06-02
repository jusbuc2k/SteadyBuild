using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Extensions.Svn
{
    public class SvnCodeRepository : ICodeRepository
    {
        private string _exePath;
        private readonly BuildProjectConfiguration _project;

        public SvnCodeRepository(BuildProjectConfiguration project) : this(null, project)
        {
        }

        public SvnCodeRepository(string exePath, BuildProjectConfiguration project)
        {
            _project = project;

            if (exePath == null)
            {
                _exePath = "svn.exe";
            }
            else
            {
                _exePath = System.IO.Path.Combine(exePath, "svn.exe");
            }            
        }

        protected async Task<System.Xml.Linq.XDocument> ExecuteXml(string command, string options, string arguments)
        {
            var commandText = new StringBuilder();

            commandText.Append(command).Append(" --xml");

            options = string.Concat("--xml ", options);

            if (!string.IsNullOrEmpty(arguments))
            {
                commandText.Append(" ").Append(arguments);
            }

            var result = await ExecuteCommand(command, options, arguments);

            if (result.Item1 != 0)
            {
                throw new Exception($"The SVN command {command} {arguments} exited with code {result.Item1}.");
            }

            return System.Xml.Linq.XDocument.Load(new System.IO.StringReader(result.Item2));
        }

        protected async Task<Tuple<int, string>> ExecuteCommand(string command, string options, string arguments)
        {
            var commandText = new StringBuilder();

            commandText.Append(command);

            if (!string.IsNullOrEmpty(options))
            {
                commandText.Append(" ").Append(options);
            }

            if (_project.RepositorySettings.ContainsKey("username"))
            {
                commandText.Append(" --username ").Append(_project.RepositorySettings["username"]);
            }

            if (_project.RepositorySettings.ContainsKey("password"))
            {
                commandText.Append(" --password ").Append(_project.RepositorySettings["password"]);
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                commandText.Append(" ").Append(arguments);
            }

            var outText = new System.Text.StringBuilder();
            var errText = new System.Text.StringBuilder();

            int exitCode = await ProcessUtils.StartProcessAsync(_exePath, commandText.ToString(), output: (line) =>
            {
                outText.AppendLine(line);
            }, errorOutput: (line) =>
            {
                errText.AppendLine(line);
            });

            return new Tuple<int, string>(exitCode, outText.ToString());
        }

        public async Task<int> Export(string path, string revisionIdentifier, string targetPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            await this.ExecuteCommand("export", "--force", $"\"{path}@{revisionIdentifier}\" \"{targetPath}\"");

            return 0;
        }

        public async Task<IEnumerable<string>> GetChangedFiles(string path, string revisionIdentifierA, string revisionIdentifierB)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var diffResult = await this.ExecuteXml("diff", "--summarize", $"{_project.RepositoryPath}@{revisionIdentifierA} {_project.RepositoryPath}@{revisionIdentifierB}");
            var fileList = diffResult.Descendants("path").Select(s => s.Value.Substring(path.Length).TrimStart('/'));

            if (diffResult != null)
            {
                return fileList;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public Task<CodeRepositoryInfo> GetInfo(string path)
        {
            return this.GetInfo(path, "HEAD");
        }

        public async Task<CodeRepositoryInfo> GetInfo(string path, string revisionIdentifier)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (revisionIdentifier == null)
            {
                throw new ArgumentNullException(nameof(revisionIdentifier));
            }

            var info = await this.ExecuteXml("info", "", $"{path}@{revisionIdentifier}");

            if (info == null)
            {
                return null;
            }

            var result = new CodeRepositoryInfo();
            var revisionAttribute = info.Descendants("entry")
                .Select(s => s.Attribute("revision"))
                .SingleOrDefault();
            
            if (revisionAttribute != null)
            {
                result.RevisionIdentifier = revisionAttribute.Value;
            }

            var authorValue = info.Descendants("author").Select(s => s.Value).FirstOrDefault();

            if (authorValue != null)
            {
                result.Author = authorValue;
            }

            if (_project.RepositorySettings.ContainsKey("compat") && _project.RepositorySettings["compat"] == "github")
            {
                // svn propget git-commit --revprop -r $newRev $url
                var gitInfo = await this.ExecuteXml("propget git-commit", $"--revprop -r {result.RevisionIdentifier}", path);
                var commitHash = gitInfo.Descendants("property")
                    .Where(x => x.Attribute("name").Value == "git-commit")
                    .Select(s => s.Value)
                    .FirstOrDefault();

                //TODO: Get this into CodeRepositoryInfo somehow
            }

            return result;
        }
    }
}
