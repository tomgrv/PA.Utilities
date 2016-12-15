using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PA.Utilities.InnoSetupTask.InnoSetup
{
    internal class FileItem
    {
        public string Source { get; set; }
        public string Tasks { get; set; }
        public string Components { get; set; }
        public string DestDir { get; set; }
        public string Flags { get; set; }
        public string Attribs { get; set; }
        public string Permissions { get; set; }

        internal static KeyValuePair<Environment.SpecialFolder, string>[] FolderMap = {
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Windows, "win"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.SystemX86, "sys"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.System,"syswow64"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.ProgramFiles,"pf"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.ProgramFilesX86,"pf32"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonProgramFiles,"cf"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonProgramFilesX86,"cf32"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Fonts,"fonts"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.StartMenu,"group"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.LocalApplicationData,"localappdata"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonApplicationData,"commonappdata"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.SendTo,"sendto"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonDesktopDirectory,"commondesktop"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.DesktopDirectory,"userdesktop"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonDocuments,"commondocs"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.MyDocuments,"userdocs"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Favorites,"commonfavorites"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Programs,"userprograms"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonPrograms,"commonprograms"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.StartMenu,"userstartmenu"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonStartMenu,"commonstartmenu"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Startup,"userstartup"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonStartup,"commonstartup"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.Templates,"usertemplates"),
                new KeyValuePair<Environment.SpecialFolder, string>(Environment.SpecialFolder.CommonTemplates,"commontemplates")
        };

        private void PreparePath(ref string path, string variable, string name)
        {
            if (path.StartsWith(variable))
            {
                path = "{" + name + "}" + path.Remove(0, variable.Length);
            }
        }

        private string FormatPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);

                foreach (var kvp in FolderMap)
                {
                    this.PreparePath(ref path, Environment.GetFolderPath(kvp.Key), kvp.Value);
                }

                this.PreparePath(ref path, Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0, 2), "sd");
            }

            return path;
        }

        private string GetString(string title, bool doFormatPath)
        {
            string data = GetProperty(title);
            return data.Length > 0 ? title + ": \"" + (doFormatPath ? this.FormatPath(data) : data) + "\";" : string.Empty;

        }

        private string GetFlag(string title)
        {
            string data = GetProperty(title);
            return data.Length > 0 ? title + ": " + data + ";" : string.Empty;
        }

        private string GetProperty(string title)
        {
            var p = this.GetType().GetProperty(title, typeof(String));

            if (p != null)
            {
                return (p.GetValue(this) ?? string.Empty).ToString();
            }

            return string.Empty;
        }

        private void SetProperty(string title, string value)
        {
            var p = this.GetType().GetProperty(title);

            if (p != null)
            {
                p.SetValue(this, value);

            }
        }

        public FileItem(string source, string dest, string component, string task)
        {
            this.SetProperty("Source", source);
            this.SetProperty("DestDir", dest);
            this.SetProperty("Components", component);
            this.SetProperty("Tasks", task);
        }

        public override string ToString()
        {
            return this.GetString("Source", false) +
                this.GetString("DestDir", true) +
                this.GetFlag("Components").Replace('.', '-') +
                this.GetFlag("Tasks") +
                this.GetFlag("Flags") +
                this.GetFlag("Attribs") +
                this.GetFlag("Permissions");
        }

        internal static IEnumerable<FileItem> OptimizeFileItems(IEnumerable<FileItem> items)
        {
            FileItem[] files = items.Distinct().ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i] != null)
                {
                    for (int j = i + 1; j < files.Length; j++)
                    {
                        if (files[j] != null)
                        {
                            if (Path.GetFullPath(files[i].Source) == Path.GetFullPath(files[j].Source) && Path.GetFullPath(files[i].DestDir) == Path.GetFullPath(files[j].DestDir))
                            {
                                files[i].Tasks = MergeConditions(files[i].Tasks, files[j].Tasks);
                                files[i].Components = MergeConditions(files[i].Components, files[j].Components);
                                files[j] = null;
                            }

                            //                            if (System.IO.File.OpenRead(files[i].Source).GetSignature() == System.IO.File.OpenRead(files[j].Source).GetSignature())
                            //                            {
                            //                                files[i].Tasks = MergeConditions(files[i].Tasks, files[j].Tasks);
                            //                                files[i].Components = MergeConditions(files[i].Components, files[j].Components);
                            //                                files[j] = null;
                            //                            }
                        }
                    }
                }
            }

            return files.Where(s => s != null).OrderBy(s => s.Source);
        }

        internal static string MergeConditions(string cA, string cB)
        {
            if (!string.IsNullOrEmpty(cA) && !string.IsNullOrEmpty(cB))
            {
                var a = cA.Split(new string[] { " or " }, StringSplitOptions.RemoveEmptyEntries);
                var b = cB.Split(new string[] { " or " }, StringSplitOptions.RemoveEmptyEntries);
                return a.Union(b).OrderBy(t => t).Aggregate((c1, c2) => c1 + " or " + c2);
            }
            else if (string.IsNullOrEmpty(cA) && !string.IsNullOrEmpty(cB))
            {
                return cB;
            }

            return cA ?? string.Empty;
        }

        internal static string MergeDependencies(string dA, string dB)
        {
            if (!string.IsNullOrEmpty(dA) && !string.IsNullOrEmpty(dB))
            {
                var a = dA.TrimEnd('/', '\\', '.');
                var b = dB.TrimEnd('/', '\\', '.');

                if (b.StartsWith(a))
                {
                    return a;
                }

                if (a.StartsWith(b))
                {
                    return b;
                }
            }
            else if (string.IsNullOrEmpty(dA) && !string.IsNullOrEmpty(dB))
            {
                return dB;
            }

            return dA ?? string.Empty;
        }
    }
}
