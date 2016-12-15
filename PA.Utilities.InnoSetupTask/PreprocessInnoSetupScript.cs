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

namespace PA.Utilities.InnoSetupTask
{
    public class PreprocessInnoSetupScript : Task
    {
        TaskLogger logger;

        public PreprocessInnoSetupScript()
        {
            logger = new TaskLogger(this);
            GitVersion.Logger.SetLoggers(this.LogInfo, this.LogWarning, s => this.LogError(s));
        }

        [Required]
        public string DestinationFolder { get; set; }

        [Required]
        public string ProjectFile { get; set; }

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Required]
        public ITaskItem[] Scripts { get; set; }

        [Required]
        public ITaskItem[] ExtraCode { get; set; }

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
                logger.LogError("Error occurred: " + exception);
                return false;
            }
            finally
            {
                GitVersion.Logger.Reset();
            }
        }


        void InnerExecute()
        {
            logger.LogInfo("Project is " + ProjectFile);

            foreach (var script in this.Scripts.Select(e => e.ItemSpec))
            {
                var scriptsrc = Path.GetDirectoryName(ProjectFile) + Path.DirectorySeparatorChar + script;
                logger.LogInfo("Script is " + scriptsrc);


                var scriptdst = Path.GetDirectoryName(ProjectFile) + Path.DirectorySeparatorChar + IntermediateOutputPath + script;
                logger.LogInfo("Temp Script is " + scriptdst);

                File.Copy(scriptsrc, scriptdst, true);

                var scriptProcessor = new Script(scriptdst);

                try
                {
                    scriptProcessor.UpdateDefine();
                    scriptProcessor.UpdateSetup();
                    scriptProcessor.UpdateCode(this.ExtraCode.Select(e => new FileInfo(e.ItemSpec)));
                }
                catch (Exception e)
                {
                    logger.LogError("Issue during processing: " + e.Message + "\n" + e.StackTrace);
                }
            }

        }

    }
}
