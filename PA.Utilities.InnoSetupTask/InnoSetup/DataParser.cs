using System;
using System.Text.RegularExpressions;
using IniParser.Model;
using IniParser.Parser;

namespace PA.Utilities.InnoSetupTask.InnoSetup
{

    class DataParser : IniDataParser
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
}
