using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild
{
    public interface IBuildOutputWriter : ILogger
    {
        System.IO.TextWriter TextWriter { get; }
    }
}
