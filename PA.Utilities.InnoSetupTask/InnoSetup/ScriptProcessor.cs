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
//using GitVersion;
using Microsoft.Build.Framework.XamlTypes;

namespace PA.Utilities.InnoSetupTask.InnoSetup
{
	public class ScriptProcessor
	{
		public string ScriptName { get; set; }

		public string Output { get; set; }

		public string AppName { get; private set; }
		public string AppNamePascalCase { get; private set; }
		public string AppVersion { get; private set; }
		public string AppGeneration { get; private set; }
		public string AppDescription { get; private set; }
		public string AppCompany { get; private set; }
		public string AppCopyright { get; private set; }
		public string AppInfoVersion { get; private set; }
		public string AppInfoSemVer { get; private set; }
		public string AppProduct { get; private set; }
		public string OutputBaseFilename { get; private set; }

		public struct ScriptData
		{
			public bool isSetup;
			public string data;
		}

		public IDictionary<string, ScriptData> Defined;

		public Assembly Assembly { get; private set; }

		private readonly ScriptParser parser = new ScriptParser();

		public ScriptProcessor(string filename, Assembly a)
		{
			Trace.TraceInformation("Using <" + filename + ">");
			this.ScriptName = filename;
			this.Assembly = a;
			this.Defined = new Dictionary<string, ScriptData>();

			this.AppName = this.InitKeys("AppName", GetAttribute<AssemblyTitleAttribute>(t => t.Title));
			this.AppNamePascalCase = this.InitKeys("AppNamePascalCase", Regex.Replace(this.AppName.ToPascalCase(), @"[^0-9a-zA-Z\._]", string.Empty), false);
			this.AppVersion = this.InitKeys("AppVersion", GetAttribute<AssemblyFileVersionAttribute>(t => t.Version));
			this.AppInfoVersion = this.InitKeys("VersionInfoVersion", GetAttribute<AssemblyInformationalVersionAttribute>(t => t.InformationalVersion), false);
			this.AppGeneration = this.InitKeys("AppGeneration", this.AppVersion.Split('.').First(), false);
			this.AppDescription = this.InitKeys("VersionInfoDescription", GetAttribute<AssemblyDescriptionAttribute>(t => t.Description));
			this.AppCompany = this.InitKeys("VersionInfoCompany", GetAttribute<AssemblyCompanyAttribute>(t => t.Company));
			this.AppCopyright = this.InitKeys("VersionInfoCopyright", GetAttribute<AssemblyCopyrightAttribute>(t => t.Copyright));
			this.AppInfoSemVer = this.InitKeys("VersionInfoTextVersion", GetFullSemVer(this.AppInfoVersion));
			this.AppProduct = this.InitKeys("VersionInfoProductName", GetAttribute<AssemblyProductAttribute>(t => t.Product));
			this.OutputBaseFilename = this.InitKeys("OutputBaseFilename", "Setup_" + this.AppNamePascalCase + "." + this.AppInfoSemVer);
		}

		private string GetFullSemVer(string s)
		{             return s.Substring(0, s.IndexOf('+') > 0 ? s.IndexOf('+') : s.Length - 1);
		}


		private string GetAttribute<T>(Func<T, string> getField, string defValue = "")
			where T : Attribute
		{

			var attributesdata = (Assembly == null ? Assembly.GetEntryAssembly().GetCustomAttributesData() : Assembly.GetCustomAttributesData()).FirstOrDefault(a => a.AttributeType == typeof(T));

			if (attributesdata != null)
			{
				var attribute = (T)Activator.CreateInstance(typeof(T), attributesdata.ConstructorArguments.Select(a => a.Value).ToArray());

				if (attribute != null)
				{
					return getField(attribute);
				}
			}

			return typeof(T).Name;
		}

		private string InitKeys(string[] key, string value, bool isSetup = true)
		{
			foreach (var k in key)
			{
				InitKeys(k, value, isSetup);
			}

			return value;
		}

		private string InitKeys(string key, string value, bool isSetup = true)
		{
			if (this.Defined.ContainsKey(key))
			{
				this.Defined[key] = new ScriptData() { data = value, isSetup = isSetup };
			}
			else
			{
				this.Defined.Add(key, new ScriptData() { data = value, isSetup = isSetup });
			}

			return value;
		}

		internal void UpdateDefine()
		{
			if (File.Exists(this.ScriptName))
			{
				var iss = this.parser.ReadFile(this.ScriptName);

				foreach (var key in this.Defined.Keys)
				{
					KeyData data = iss.Global.FirstOrDefault(kd => Regex.Match(kd.KeyName, @"#(\w+\s){2}").Groups[1].Value.Trim() == key.Trim());

					if (data == null)
					{
						iss.Global.AddKey("#define " + key + " \"" + this.Defined[key].data + "\"");
					}
					else
					{
						data.KeyName = Regex.Match(data.KeyName, @"#(\w+\s){2}").Value + "\"" + this.Defined[key].data + "\"";
					}
				}

				this.parser.WriteFile(this.ScriptName, iss);
			}
		}

		internal void UpdateSetup(bool withDefine = true)
		{
			if (File.Exists(this.ScriptName))
			{
				var iss = this.parser.ReadFile(this.ScriptName);

				if (!iss.Sections.ContainsSection("Setup"))
				{
					iss.Sections.AddSection("Setup");
				}

				var section = iss.Sections["Setup"];

				foreach (var k in this.Defined.Where(e => e.Value.isSetup))
				{
					section[k.Key] = withDefine ? ("{#" + k.Key + "}") : k.Value.data;
				}

				this.parser.WriteFile(this.ScriptName, iss);
			}
		}

		internal void UpdateFile(FileItem[] list)
		{
			if (File.Exists(this.ScriptName) && list.Length > 0)
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
		}

		internal void UpdateCode(FileInfo[] list)
		{
			if (File.Exists(this.ScriptName) && list.Length > 0)
			{
				var iss = this.parser.ReadFile(this.ScriptName);

				if (!iss.Sections.ContainsSection("Code"))
				{
					iss.Sections.AddSection("Code");
				}

				var section = iss.Sections["Code"];

				foreach (var script in list)
				{
					var code = section.FirstOrDefault(kd => Path.GetFileName(kd.Value) == Path.GetFileName(script.FullName));

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

