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

        protected async Task<System.Xml.Linq.XDocument> ExecuteSvnCommand(string command, string options, string arguments)
        {
            var commandText = new StringBuilder();

            commandText.Append(command).Append(" --xml");

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

            var sb = new System.Text.StringBuilder();

            int exitCode = await ProcessUtils.StartProcessAsync(_exePath, commandText.ToString(), output: (line) =>
            {
                sb.AppendLine(line);
            });

            return System.Xml.Linq.XDocument.Load(new System.IO.StringReader(sb.ToString()));
        }

        public async Task<int> Export(string path, string revisionIdentifier, string targetPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            await this.ExecuteSvnCommand("export", "", $"\"{path}@{revisionIdentifier}\" \"{targetPath}\"");

            return 0;
        }

        public async Task<IEnumerable<string>> GetChangedFiles(string path, string revisionIdentifierA, string revisionIdentifierB)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var diffResult = await this.ExecuteSvnCommand("diff", "--summarize", $"{_project.RepositoryPath}@{revisionIdentifierA} {_project.RepositoryPath}@{revisionIdentifierB}");
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

            var info = await this.ExecuteSvnCommand("info", "", $"{path}@{revisionIdentifier}");

            if (info == null)
            {
                return null;
            }

            var result = new CodeRepositoryInfo();
            var revisionAttribute = info.Descendants("entry").Select(s => s.Attribute("revision")).SingleOrDefault();
            
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
                var gitInfo = await this.ExecuteSvnCommand("propget git-commit", $"--revprop -r {result.RevisionIdentifier}", path);
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
