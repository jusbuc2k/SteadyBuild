using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Agent
{
    public static class PathUtils
    {
        public static void CleanFolder(string path)
        {
            var filesInFolder = System.IO.Directory.EnumerateFiles(path);
            var dirsInFolder = System.IO.Directory.EnumerateDirectories(path);

            foreach(var file in filesInFolder)
            {
                System.IO.File.Delete(file);
            }

            foreach (var dir in dirsInFolder)
            {
                System.IO.Directory.Delete(dir, true);
            }
        }
    }
}
