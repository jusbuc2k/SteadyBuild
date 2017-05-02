using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public interface IWatchTask
    {
        void Run();

        void Pause();

        void Force();
    }
}
