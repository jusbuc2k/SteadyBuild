using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildResult
    {
        public BuildResult(int statusCode)
        {
            this.StatusCode = statusCode;
        }

        public bool Success
        {
            get
            {
                return this.StatusCode == 0;
            }
        }

        public bool Timeout
        {
            get
            {
                return this.StatusCode == 418;
            }
        }

        public int StatusCode { get; private set; }

        public bool ShouldRetry { get; set; } = false;

        public static BuildResult Fail(int statusCode, bool shouldRetry = true)
        {
            return new BuildResult(statusCode)
            {
                ShouldRetry = shouldRetry
            };
        }
    }
}
