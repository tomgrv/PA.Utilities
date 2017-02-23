using System;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;
using IniParser.Exceptions;
using System.Text;
using System.Text.RegularExpressions;
using IniParser.Model.Formatting;

namespace PA.Utilities.InnoSetupTask.InnoSetup
{
    public class ScriptParser : StreamIniDataParser
    {
        class InnoScriptDataFormatter : DefaultIniDataFormatter
        {
            public override string IniDataToString(IniData iniData)
            {
                return base.IniDataToString(iniData)
                    .Replace(this.Configuration.AssigmentSpacer + this.Configuration.KeyValueAssigmentChar + this.Configuration.AssigmentSpacer + Environment.NewLine, Environment.NewLine);
            }
        }

        class InnoScriptDataParser : IniDataParser
        {
            private string currentSection = string.Empty;

            protected override string ExtractKey(string s)
            {
                if (this.currentSection != "setup")
                {
                    return s;
                }

                return base.ExtractKey(s);
            }

            protected override string ExtractValue(string s)
            {
                if (this.currentSection != "setup")
                {
                    return string.Empty;
                }

                return base.ExtractValue(s);
            }

            protected override bool LineMatchesAKeyValuePair(string line)
            {
                if (this.currentSection != "setup" && Regex.IsMatch(line, "^([a-zA-Z]+\\s*:|#[a-z]+\\s+)"))
                {
                    return true;
                }

                return base.LineMatchesAKeyValuePair(line);
            }





            protected override void ProcessSection(string line, IniData currentIniData)
            {
                base.ProcessSection(line, currentIniData);
                this.currentSection = line.Trim().Trim('[', ']').ToLower();
            }
        }

        public ScriptParser()
            : base(new InnoScriptDataParser())
        {
            this.Parser.Configuration.KeyValueAssigmentChar = '=';
            this.Parser.Configuration.AssigmentSpacer = "";
            this.Parser.Configuration.AllowDuplicateKeys = true;
            this.Parser.Configuration.SkipInvalidLines = true;
            this.Parser.Configuration.AllowKeysWithoutSection = true;
        }

        public IniData ReadFile(string filePath, Encoding fileEncoding = null)
        {
            if (fileEncoding == null)
            {
                fileEncoding = Encoding.ASCII;
            }
            if (filePath == string.Empty)
            {
                throw new ArgumentException("Bad filename.");
            }
            IniData result;
            try
            {
                using (FileStream fileStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream, fileEncoding))
                    {
                        result = base.ReadData(streamReader);
                    }
                }
            }
            catch (IOException innerException)
            {
                throw new ParsingException(string.Format("Could not parse file {0}", filePath), innerException);
            }
            return result;
        }

        public void WriteFile(string filePath, IniData parsedData, Encoding fileEncoding = null)
        {
            if (fileEncoding == null)
            {
                fileEncoding = Encoding.ASCII;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Bad filename.");
            }
            if (parsedData == null)
            {
                throw new ArgumentNullException("parsedData");
            }
            using (FileStream fileStream = System.IO.File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, fileEncoding))
                {
                    base.WriteData(streamWriter, parsedData, new InnoScriptDataFormatter());
                }
            }
        }
    }
}

