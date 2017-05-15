using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public static class Extensions
    {
        public static void AddAgentVariables(this BuildEnvironment env, BuildAgentOptions agentOptions)
        {
            env.Variables.Add("AgentHost", Environment.MachineName);
            env.Variables.Add("AgentIdentifier", agentOptions.AgentIdentifier);
        }
    }
}
