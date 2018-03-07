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

		internal SolutionProcessor(string solution, TaskLogger logger = null)
        {


            this.Solution = new Solution(solution);
            this.Logger = logger;

			logger?.LogInfo("Loading solution " + this.Solution.SolutionName);

			using (var pc = new ProjectCollection())
            {
				foreach (ProjectInSolution prj in  this.Solution.Projects.Where(p => p.ProjectType != SolutionProjectType.SolutionFolder ))
                {
                    var project = new ProjectProcessor(pc.LoadProject(Path.Combine( this.Solution.DirectoryName, prj.RelativePath)), logger);
					project.Init();
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
					var project = new ProjectProcessor(pc.LoadProject(Path.Combine(this.Solution.DirectoryName, prj.RelativePath)),configuration,platform,  logger);
					project.Init();
				}
			}
		}

        public void GetProject(string path)
        {

        }

        public void Init()
        {
            var solution = new Solution(this.Solution.DirectoryName + Path.DirectorySeparatorChar + this.Solution.SolutionName);

            using (var pc = new ProjectCollection())
            {
				foreach (ProjectInSolution prj in solution.Projects.Where(p => p.ProjectType != SolutionProjectType.SolutionFolder ))
                {
                    var project = new ProjectProcessor(pc.LoadProject(Path.Combine(solution.DirectoryName, prj.RelativePath)));
                    project.Init();
                }
            }
        }


        internal IEnumerable<FileItem> GetFiles(string configuration, string platform)
        {
            using (var pc = new ProjectCollection())
            {
                foreach (ProjectInSolution prj in this.Solution.Projects.Where(p => p.ProjectType != SolutionProjectType.SolutionFolder))
                {
                    var pp = new ProjectProcessor(pc.LoadProject(Path.Combine(this.Solution.DirectoryName, prj.RelativePath)), configuration, platform);

                    Trace.TraceInformation("Processing project <" + pp.Project.FullPath + ">");

                    foreach (FileItem file in pp.GetFiles(configuration, platform))
                    {
                        yield return file;
                    }
                }
            }
        }





    }
}
