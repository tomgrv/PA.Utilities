using System;
using System.Reflection;

namespace Microsoft.Build
{
    /// <summary>
    /// Represents a project in a sln file.
    /// </summary>
    internal class ProjectInSolution
    {
        private static readonly Type projectInSolutionType = GetProjectInSolutionType();
        private static readonly PropertyInfo relativePathProperty = GetRelativePathProperty();
        private static readonly PropertyInfo projectTypeProperty = GetProjectTypeProperty();

        /// <summary>
        /// The path of the project relative to the solution.
        /// </summary>
        public string RelativePath { get; private set; }

        /// <summary>
        /// Indicates if the project is a solution folder.
        /// </summary>
        public bool IsSolutionFolder { get; private set; }

        public ProjectInSolution(object solutionProject)
        {
            string projectType = projectTypeProperty.GetValue(solutionProject, index: null).ToString();
            IsSolutionFolder = projectType.Equals("SolutionFolder", StringComparison.OrdinalIgnoreCase);
            RelativePath = (string)relativePathProperty.GetValue(solutionProject, index: null);
        }

        private static Type GetProjectInSolutionType()
        {
            var assembly = typeof(Microsoft.Build.Construction.ProjectElement).Assembly;
            var projectInSolutionType = assembly.GetType("Microsoft.Build.Construction.ProjectInSolution");
            if (projectInSolutionType == null)
            {
                throw new Exception("Cannot load project type");
            }

            return projectInSolutionType;
        }

        private static PropertyInfo GetRelativePathProperty()
        {
            return projectInSolutionType.GetProperty("RelativePath", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static PropertyInfo GetProjectTypeProperty()
        {
            return projectInSolutionType.GetProperty("ProjectType", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}