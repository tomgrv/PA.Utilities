using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PA.Utilities.InnoSetupTask.Microsoft;
using PA.Utilities.InnoSetupTask.InnoSetup;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using PA.Utilities.InnoSetupTask.Utils;

namespace PA.Utilities.InnoSetupTask
{
    [LoadInSeparateAppDomain, Serializable]
    public class PreprocessInnoSetupScript : AppDomainIsolatedTask
    {
        TaskLogger logger;

        public PreprocessInnoSetupScript()
        {
            logger = new TaskLogger(this);
            GitVersion.Logger.SetLoggers(logger.LogInfo, logger.LogWarning, s => logger.LogError(s));

            this.UpdateDefineSection = true;
            this.UpdateSetupSection = true;
            this.IncludeCodeSnippets = true;
        }

        [Required]
        public string DestinationFolder { get; set; }

        [Required]
        public string SolutionPath { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        public string Configuration { get; set; }

        public string Platform { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public ITaskItem[] Scripts { get; set; }

        public ITaskItem[] ExtraCode { get; set; }

        [Output]
        public ITaskItem[] Targets { get; set; }

        public bool UpdateDefineSection { get; set; }

        public bool UpdateSetupSection { get; set; }

        public bool IncludeCodeSnippets { get; set; }

        public override bool Execute()
        {
            try
            {
                InnerExecute();
                return true;
            }
            catch (WarningException errorException)
            {
                logger.LogWarning(errorException.Message);
                return true;
            }
            catch (Exception exception)
            {
                logger.LogError("Error occurred: " + exception + "\n" + exception.StackTrace);
                return false;
            }
            finally
            {
                GitVersion.Logger.Reset();
            }
        }

        void InnerExecute()
        {
            logger.LogInfo("Project is " + ProjectPath);

            var s = new SolutionProcessor(SolutionPath);
            var p = new ProjectProcessor(ProjectPath, Configuration, Platform);

            var targets = new List<ITaskItem>();

            foreach (var script in this.Scripts.Select(e => e.ItemSpec))
            {
                var scriptsrc = Path.GetDirectoryName(ProjectPath) + Path.DirectorySeparatorChar + script;
                logger.LogInfo("InnoSetup input script is " + scriptsrc);

                var scriptdst = Path.GetDirectoryName(ProjectPath) + Path.DirectorySeparatorChar + DestinationFolder + script;
                logger.LogInfo("InnoSetup output script is " + scriptdst);

                File.Copy(scriptsrc, scriptdst, true);

                var t = p.GetProjectTarget();

                logger.LogInfo("InnoSetup target is " + scriptdst);


                var a = Assembly.ReflectionOnlyLoadFrom(t);

                // IsolatedDomain.ReflectionOnlyLoadFrom(t, a =>
                // {
                var scriptProcessor = new ScriptProcessor(scriptdst, a);

                try
                {
                    if (this.UpdateDefineSection)
                        scriptProcessor.UpdateDefine();

                    if (this.UpdateSetupSection)
                        scriptProcessor.UpdateSetup();

                    if (this.IncludeCodeSnippets)
                        scriptProcessor.UpdateCode(this.ExtraCode.Select(e => new FileInfo(e.ItemSpec)).OrderBy(e => e.ToString()).ToArray());

                    var files = s.GetFiles(Configuration, Platform);

                    foreach (var f in files)
                    {
                        logger.LogInfo("InnoSetup script will reference <" + f.Source + ">");
                    }

                    scriptProcessor.UpdateFile(files.ToArray());

                    targets.Add(new TaskItem(scriptProcessor.OutputBaseFilename));

                    logger.LogInfo("InnoSetup script successfully updated!");
                }
                catch (Exception e)
                {
                    logger.LogError("InnoSetup script cannot be updated: " + e.Message + "\n" + e.StackTrace);
                }
                // });

            }

            this.Targets = targets.ToArray();
        }

    }
}
