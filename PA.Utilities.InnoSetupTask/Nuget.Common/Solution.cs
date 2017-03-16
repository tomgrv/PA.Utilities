using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Construction;

namespace NuGet.Common
{
    /// <summary>
    /// Represents the solution loaded from a sln file. We use the internal class 
    /// Microsoft.Build.Construction.SolutionParser to parse sln files.
    /// </summary>
    internal class Solution
    {
        public List<ProjectInSolution> Projects { get; private set; }

		public SolutionFile SolutionFile { get; private set; }

        public string DirectoryName { get; private set; }
        public string SolutionName { get; private set; }


        public Solution(string solutionFileName)
        {

            this.DirectoryName = Path.GetDirectoryName(solutionFileName);
            this.SolutionName = Path.GetFileNameWithoutExtension(solutionFileName);

			this.SolutionFile = SolutionFile.Parse(solutionFileName);

            this.Projects = new List<ProjectInSolution>();
			foreach (var project in this.SolutionFile.ProjectsInOrder)
            {
                this.Projects.Add(project);
            }

        }
    }
}
