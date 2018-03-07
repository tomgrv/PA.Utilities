using Microsoft.Build.Evaluation;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using PA.Utilities.InnoSetupTask.InnoSetup;
using Microsoft.Build.Construction;

namespace PA.Utilities.InnoSetupTask.Microsoft
{
    internal class SolutionProcessor
    {
        internal Solution Solution { get; private set; }
		internal TaskLogger Logger { get; private set; }

		internal Dictionary<string, ProjectProcessor> Projects { get; private set; } = new Dictionary<string, ProjectProcessor>();

		internal SolutionProcessor(string solution, TaskLogger logger = null)
        {


            this.Solution = new Solution(solution);
            this.Logger = logger;

			logger?.LogInfo("Loading solution " + this.Solution.SolutionName);

			using (var pc = new ProjectCollection())
            {
				foreach (ProjectInSolution prj in  this.Solution.Projects.Where(p => p.ProjectType != SolutionProjectType.SolutionFolder ))
                {
					var path = Path.Combine(this.Solution.DirectoryName, prj.RelativePath);

					var project = new ProjectProcessor(pc.LoadProject(path), logger);
					project.Init();

					Projects.Add(path, project);
                }
            }
        }

		internal SolutionProcessor(string solution, string configuration, string platform, TaskLogger logger = null)
		{
			this.Solution = new Solution(solution);
			this.Logger = logger;

			logger?.LogInfo("Loading solution " + this.Solution.SolutionName + " - "+configuration+"/"+platform);

			using (var pc = new ProjectCollection())
			{
				foreach (ProjectInSolution prj in this.Solution.Projects.Where(p => p.ProjectType != SolutionProjectType.SolutionFolder))
				{
					var path = Path.Combine(this.Solution.DirectoryName, prj.RelativePath);

					var project = new ProjectProcessor(pc.LoadProject(path),configuration,platform,  logger);
					project.Init();

					Projects.Add(path, project);
				}
			}
		}

        public ProjectProcessor GetProject(string path)
        {
			path = Path.Combine(this.Solution.DirectoryName, path);

			Logger?.LogInfo("Getting project at " + path);

			return this.Projects[path];
        }


        internal IEnumerable<FileItem> GetFiles(string configuration, string platform)
        {
			foreach (var kvp in this.Projects)
			{
				foreach (FileItem file in kvp.Value.GetFiles(configuration, platform))
				{
					yield return file;
				}
            }
        }

    }
}
