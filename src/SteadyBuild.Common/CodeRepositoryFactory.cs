using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;

namespace SteadyBuild
{
    public static class CodeRepositoryFactory
    {
        public static ICodeRepository Create(BuildProjectConfiguration project)
        {
            if (project.RepositoryType.Equals("svn", StringComparison.OrdinalIgnoreCase))
            {
                return new SteadyBuild.Extensions.Svn.SvnCodeRepository(project);
            }
            else
            {
                throw new NotSupportedException($"There is no provided for the repository type '{project.RepositoryType}'.");
            }
        }
    }
}
