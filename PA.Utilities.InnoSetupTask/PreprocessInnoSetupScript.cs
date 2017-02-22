﻿using System;
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

namespace PA.Utilities.InnoSetupTask
{
    [LoadInSeparateAppDomain, Serializable]
    public class PreprocessInnoSetupScript : AppDomainIsolatedTask
    {
        TaskLogger logger;

        public PreprocessInnoSetupScript()
        {
            logger = new TaskLogger(this);

            this.UpdateDefineSection = true;
            this.UpdateSetupSection = true;
            this.IncludeCodeSnippets = true;
        }

        [Required]
        public string SolutionPath { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        public string Configuration { get; set; }

        public string Platform { get; set; }

        [Required]
        public string TargetDir { get; set; }

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
            }
        }

        void InnerExecute()
        {
            logger.LogInfo("Project is " + ProjectPath);

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var name = e.Name.Split(',')[0];
                var list = Directory.EnumerateFiles(Path.GetDirectoryName(SolutionPath) + Path.DirectorySeparatorChar + "packages", name + "*.dll", SearchOption.AllDirectories);

                logger.LogInfo("Load dependent assembly \"" + name + "\"");

                foreach (var asm in list)
                {
                    try
                    {
                        if (File.Exists(asm)) return Assembly.LoadFrom(asm);
                    }
                    catch
                    {
                        logger.LogError("Cannot load dependent assembly \"" + name + "\" from \"" + asm + "\"");
                    }
                }

                return null;
            };

            var s = new SolutionProcessor(SolutionPath);
            var p = (Configuration != null && Platform != null) ? new ProjectProcessor(ProjectPath, Configuration, Platform) : new ProjectProcessor(ProjectPath);

            var targets = new List<ITaskItem>();

            foreach (var script in this.Scripts.Select(e => e.ItemSpec))
            {
                var scriptsrc = Path.Combine(Path.GetDirectoryName(ProjectPath), script);
                logger.LogInfo("InnoSetup input script is " + scriptsrc);

                var scriptdst = Path.Combine(Path.GetDirectoryName(ProjectPath), TargetDir) + Path.GetFileName(script);
                logger.LogInfo("InnoSetup output script is " + scriptdst);

                File.Copy(scriptsrc, scriptdst, true);

                var t = p.GetProjectTarget();

                var a = Assembly.ReflectionOnlyLoadFrom(t);

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

            }

            this.Targets = targets.ToArray();
        }

    }
}