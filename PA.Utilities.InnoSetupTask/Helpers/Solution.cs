using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Build
{
    /// <summary>
    /// Represents the solution loaded from a sln file. We use the internal class 
    /// Microsoft.Build.Construction.SolutionParser to parse sln files.
    /// </summary>
    internal class Solution
    {
        private static readonly Type solutionParserType = GetSolutionParserType();
        private static readonly PropertyInfo solutionReaderProperty = GetSolutionReaderProperty();
        private static readonly MethodInfo parseSolutionMethod = GetParseSolutionMethod();
        private static readonly PropertyInfo projectsProperty = GetProjectsProperty();

        public List<ProjectInSolution> Projects { get; private set; }

        public string DirectoryName { get; private set; }
        public string SolutionName { get; private set; }

        public Solution(string solutionFileName)
        {

            this.DirectoryName = Path.GetDirectoryName(solutionFileName);
            this.SolutionName = Path.GetFileNameWithoutExtension(solutionFileName);

            var solutionParser = solutionParserType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null, types: Type.EmptyTypes, modifiers: null).Invoke(null);
            using (var streamReader = new StreamReader(File.OpenRead(solutionFileName)))
            {
                solutionReaderProperty.SetValue(solutionParser, streamReader, index: null);
                parseSolutionMethod.Invoke(solutionParser, parameters: null);
            }

            this.Projects = new List<ProjectInSolution>();
            foreach (var proj in (object[])projectsProperty.GetValue(solutionParser, index: null))
            {
                this.Projects.Add(new ProjectInSolution(proj));
            }

        }

        private static Type GetSolutionParserType()
        {
            var assembly = typeof(Microsoft.Build.Construction.ProjectElement).Assembly;
            var solutionParser = assembly.GetType("Microsoft.Build.Construction.SolutionParser");

            if (solutionParser == null)
            {
                throw new Exception("Cannot Find Parser");
            }

            return solutionParser;
        }

        private static PropertyInfo GetSolutionReaderProperty()
        {
            if (solutionParserType != null)
            {
                return solutionParserType.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return null;
        }

        private static MethodInfo GetParseSolutionMethod()
        {
            if (solutionParserType != null)
            {
                return solutionParserType.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return null;
        }

        private static PropertyInfo GetProjectsProperty()
        {
            if (solutionParserType != null)
            {
                return solutionParserType.GetProperty("Projects", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return null;
        }
    }
}
