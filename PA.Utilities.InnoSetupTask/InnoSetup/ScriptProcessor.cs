using System;
using System.Collections.Generic;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.IO;
using GitVersion;

namespace PA.Utilities.InnoSetupTask.InnoSetup
{
    public class Script
    {
        public string ScriptName { get; set; }

        public bool WithDefine { get; private set; }

        private readonly ScriptParser parser = new ScriptParser();

        public Script(string filename)
        {
            Trace.TraceInformation("Using <" + filename + ">");
            this.ScriptName = filename;
        }

        internal void UpdateDefine()
        {
            var iss = this.parser.ReadFile(this.ScriptName);

            var appname = GetAttribute<AssemblyTitleAttribute>(t => t.Title);
            var version = GetAttribute<AssemblyInformationalVersionAttribute>(t => t.InformationalVersion);

            if (appname.Length > 0 && version.Length > 0)
            {

                this.UpdateDefine(iss.Global, "AppName", appname);
                this.UpdateDefine(iss.Global, "AppNamePascalCase", Regex.Replace(appname.ToPascalCase(), @"[^0-9a-zA-Z\._]", string.Empty));
                this.UpdateDefine(iss.Global, "AppGeneration", GetAttribute<AssemblyFileVersionAttribute>(t => t.Version).Split('.').First());
                this.UpdateDefine(iss.Global, "AppVersion", GetAttribute<AssemblyFileVersionAttribute>(t => t.Version));
                this.UpdateDefine(iss.Global, "AppDescription", GetAttribute<AssemblyDescriptionAttribute>(t => t.Description));
                this.UpdateDefine(iss.Global, "AppCompany", GetAttribute<AssemblyCompanyAttribute>(t => t.Company));
                this.UpdateDefine(iss.Global, "AppCopyright", GetAttribute<AssemblyCopyrightAttribute>(t => t.Copyright));
                this.UpdateDefine(iss.Global, "AppInfoVersion", version);
                this.UpdateDefine(iss.Global, "AppInfoSemVer", version.Substring(0, version.IndexOf('+') > 0 ? version.IndexOf('+') : version.Length - 1));
                this.UpdateDefine(iss.Global, "AppProduct", GetAttribute<AssemblyProductAttribute>(t => t.Product));

                this.parser.WriteFile(this.ScriptName, iss);

                this.WithDefine = true;
            }
        }

        private void UpdateDefine(KeyDataCollection keys, string key, string value)
        {
            KeyData data = keys.FirstOrDefault(kd => Regex.Match(kd.KeyName, @"#(\w+\s){2}").Groups[1].Value.Trim() == key.Trim());

            if (data == null)
            {
                keys.AddKey("#define " + key + " \"" + value + "\"");
            }
            else
            {
                data.KeyName = Regex.Match(data.KeyName, @"#(\w+\s){2}").Value + "\"" + value + "\"";
            }
        }

        internal void UpdateSetup()
        {
            var iss = this.parser.ReadFile(this.ScriptName);

            if (!iss.Sections.ContainsSection("Setup"))
            {
                iss.Sections.AddSection("Setup");
            }

            var section = iss.Sections["Setup"];

            if (this.WithDefine)
            {
                section["AppName"] = "{#AppName}";
                section["AppVersion"] = "{#AppVersion}";
                section["AppComments"] = "{#AppDescription}";
                section["AppPublisher"] = "{#AppCompany}";
                section["AppCopyright"] = "{#AppCopyright}";
                section["VersionInfoVersion"] = "{#AppVersion}";
                section["VersionInfoCompany"] = "{#AppCompany}";
                section["VersionInfoProductName"] = "{#AppProduct}";
                section["VersionInfoDescription"] = "{#AppDescription}";
                section["VersionInfoTextVersion"] = "{#AppInfoVersion}";
                section["OutputBaseFilename"] = "Setup_{#AppNamePascalCase}.{#AppInfoSemVer}";
            }
            else
            {
                section["AppName"] = GetAttribute<AssemblyTitleAttribute>(t => t.Title);
                section["AppVersion"] = GetAttribute<AssemblyFileVersionAttribute>(t => t.Version);
                section["AppComments"] = GetAttribute<AssemblyDescriptionAttribute>(t => t.Description);
                section["AppPublisher"] = GetAttribute<AssemblyCompanyAttribute>(t => t.Company);
                section["AppCopyright"] = GetAttribute<AssemblyCopyrightAttribute>(t => t.Copyright);
                section["VersionInfoVersion"] = GetAttribute<AssemblyFileVersionAttribute>(t => t.Version);
                section["VersionInfoCompany"] = GetAttribute<AssemblyCompanyAttribute>(t => t.Company);
                section["VersionInfoProductName"] = GetAttribute<AssemblyTitleAttribute>(t => t.Title);
                section["VersionInfoDescription"] = GetAttribute<AssemblyDescriptionAttribute>(t => t.Description);
                section["VersionInfoTextVersion"] = GetAttribute<AssemblyInformationalVersionAttribute>(t => t.InformationalVersion);
                section["OutputBaseFilename"] = "Setup_" + GetAttribute<AssemblyTitleAttribute>(t => t.Title).ToPascalCase() + "." + GetAttribute<AssemblyFileVersionAttribute>(t => t.Version);
            }

            this.parser.WriteFile(this.ScriptName, iss);
        }

        private string GetAttribute<T>(Func<T, string> getField, string defValue = "")
            where T : Attribute
        {
            var attribute = Assembly.GetEntryAssembly().GetCustomAttribute<T>();
            return attribute != null ? getField(attribute) : defValue;
        }

        internal void UpdateFile(IEnumerable<FileItem> list)
        {
            var iss = this.parser.ReadFile(this.ScriptName);

            if (!iss.Sections.ContainsSection("Files"))
            {
                iss.Sections.AddSection("Files");
            }

            var section = iss.Sections["Files"];

            section.RemoveAllKeys();

            foreach (var i in FileItem.OptimizeFileItems(list))
            {
                section.AddKey(i.ToString(), "");
            }

            this.parser.WriteFile(this.ScriptName, iss);
        }

        internal void UpdateCode(IEnumerable<FileInfo> list)
        {
            if (list.Count() > 0)
            {
                var iss = this.parser.ReadFile(this.ScriptName);

                if (!iss.Sections.ContainsSection("Code"))
                {
                    iss.Sections.AddSection("Code");
                }

                var section = iss.Sections["Code"];

                foreach (var script in list)
                {
                    var code = section.FirstOrDefault(kd => kd.Value == script.Name);

                    if (code == null)
                    {
                        var key = "#include \"" + script.FullName + "\"";
                        section.AddKey(key.ToString(), "");
                    }
                }

                this.parser.WriteFile(this.ScriptName, iss);
            }
        }
    }

}

