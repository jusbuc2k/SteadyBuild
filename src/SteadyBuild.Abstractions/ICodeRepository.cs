using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface ICodeRepository
    {
        Task<CodeRepositoryInfo> GetInfo(string path);

        Task<CodeRepositoryInfo> GetInfo(string path, string revisionIdentifier);

        Task<IEnumerable<string>> GetChangedFiles(string path, string revisionIdentifierA, string revisionIdentifierB);

        Task<int> Export(string path, string revisionIdentifier, string targetPath);
    }
}
