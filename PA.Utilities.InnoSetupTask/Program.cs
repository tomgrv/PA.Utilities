using System;
namespace PA.Utilities.InnoSetupTask
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var t = new PreprocessInnoSetupScript();
            t.Execute();
        }
    }
}
