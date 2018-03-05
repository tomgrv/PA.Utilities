using System;
using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PA.Utilities.InnoSetupTask;

namespace PA.Utilities.InnoSetupTask
{
    public class PreprocessFileList : Task
    {
        TaskLogger logger;

        public PreprocessFileList()
        {
            logger = new TaskLogger(this);
            GitVersion.Logger.SetLoggers(this.LogInfo, this.LogWarning, s => this.LogError(s));
        }

        [Required]
        public string IntermediateOutputPath { get; set; }

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
                logger.LogError("Error occurred: " + exception +" at line " + exception.Source );
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

            }
        }
