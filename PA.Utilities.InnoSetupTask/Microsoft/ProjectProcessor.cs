using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Evaluation;
using PA.Utilities.InnoSetupTask.InnoSetup;

namespace PA.Utilities.InnoSetupTask.Microsoft
{
    public class ProjectProcessor
    {
        private const string Components = @"Components";
        private const string DestDir = @"DestDir";
        private const string Target = @"target";
        private const string Dependencies = @"dependencies";
        private const string Description = @"description";
        private const string Files = @"files";
        private const string Setup = @"setup.config";

        public Project Project { get; private set; }

        public ProjectProcessor(Project project)
        {
            this.Project = project;
        }

        public ProjectProcessor(Project project, string config, string platform)
            : this(project)
        {
            this.Project.SetProperty("Configuration", config);
            this.Project.SetProperty("Platform", platform);

            try
            {
                this.Project.ReevaluateIfNecessary();
            }
            catch (Exception e)
            {

            }
        }

        public ProjectProcessor(string path)
        {
            using (ProjectCollection pc = new ProjectCollection())
            {
                this.Project = pc.GetLoadedProjects(path).FirstOrDefault();

                if (this.Project == null)
                {
                    this.Project = pc.LoadProject(path);
                }

            }
        }

        public ProjectProcessor(string path, string config, string platform)
           : this(path)
        {
            this.Project.SetProperty("Configuration", config);
            this.Project.SetProperty("Platform", platform);
            try
            {
                this.Project.ReevaluateIfNecessary();
            }
            catch (Exception e)
            {

            }
        }

        public void Init()
        {
            ProjectItem pi = this.Project.Items.FirstOrDefault(i => i.ItemType == "None" && i.EvaluatedInclude == Setup);

            Trace.TraceInformation("Processing solution <" + pi + ">");

            if (pi == null)
            {
                using (StreamWriter sw = System.IO.File.CreateText(this.Project.DirectoryPath + Path.DirectorySeparatorChar + Setup))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    sw.WriteLine("<setup> <!-- use " + Components.ToLower() + " property to specify components --> ");
                    sw.WriteLine("<" + Target.ToLower() + " DestDir=\"{app}\\.\"  />");
                    sw.WriteLine("<" + Dependencies.ToLower() + " DestDir=\"{app}\\.\"   />");
                    sw.WriteLine("<" + Files.ToLower() + " DestDir=\"{app}\\.\"   />");
                    sw.WriteLine("</setup>");
                }

                this.Project.AddItem("None", Setup);
            }
            else
            {
                if (!pi.HasMetadata("CopyToOutputDirectory"))
                {
                    pi.SetMetadataValue("CopyToOutputDirectory", "Never");
                }
            }

            this.Project.Save();
        }

        public string GetProjectTarget()
        {
            return this.GetProjectProperty("TargetPath");
        }

        public string GetProjectProperty(string name)
        {
            name = name.ToLower();
            var prop = this.Project?.AllEvaluatedProperties?.FirstOrDefault(p => p.Name.ToLower() == name);
            return prop != null ? prop.EvaluatedValue : "";
        }

        private IEnumerable<ProjectItem> GetProjectItems(string type)
        {
            type = type.ToLower();
            return this.Project.AllEvaluatedItems.Where(i => i.ItemType.ToLower() == type);
        }

        private IEnumerable<Tuple<ProjectItem, string>> GetProjectItems(string metadata, string type)
        {
            metadata = metadata.ToLower();
            type = type.ToLower();
            return this.Project.AllEvaluatedItems
                .Where(i => i.HasMetadata(metadata) && i.ItemType.ToLower() == type)
                .Select(p => new Tuple<ProjectItem, string>(p, p.GetMetadataValue(metadata)));
        }

        internal IEnumerable<FileItem> GetFiles(string configuration, string platform, string parentComponents = null, string parentDir = null, string parentTasks = null)
        {
            var source = this.GetProjectTarget();

            var config = this.GetProjectItems("None").FirstOrDefault(p => p.EvaluatedInclude == Setup);

            if (config != null && System.IO.File.Exists(source))
            {
                var doc = new XmlDocument();

                doc.Load(this.Project.DirectoryPath + @"\" + config.EvaluatedInclude);

                XmlNode setup = doc.DocumentElement.SelectSingleNode("/setup");

                if (setup.Attributes[Description.ToLower()] == null)
                {
                    setup.Attributes.Append(doc.CreateAttribute(Description.ToLower()));

                    setup.Attributes[Description.ToLower()].Value = Assembly.ReflectionOnlyLoadFrom(source)
                        .GetCustomAttributesData()
                        .Where(ad => ad.AttributeType == typeof(AssemblyDescriptionAttribute))
                        .Select(ad => ad.ConstructorArguments[0].ToString().Trim('\"'))
                        .FirstOrDefault();
                }

                var components = FileItem.MergeConditions(GetAttribute(setup, Components), parentComponents);

                foreach (XmlNode node in setup.ChildNodes)
                {

                    switch (node.Name)
                    {
                        case Target:

                            yield return new FileItem(source,
                                GetAttribute(node, "DestDir"),
                                components,
                                GetAttribute(node, "Tasks"), true);

                            if (System.IO.File.Exists(source + ".config"))
                            {
                                yield return new FileItem(source + ".config",
                                    GetAttribute(node, "DestDir"),
                                    components,
                                    GetAttribute(node, "Tasks"));

                            }

                            break;

                        case Dependencies:

                            foreach (var files in this.GetProjectItems("Hintpath", "Reference"))
                            {
                                yield return new FileItem(Path.GetFullPath(Path.Combine(this.Project.DirectoryPath, files.Item2)),
                                    FileItem.MergeDependencies(GetAttribute(node, "DestDir"), parentDir),
                                    components,
                                    GetAttribute(node, "Tasks"));
                            }

                            foreach (var refs in this.GetProjectReferences(configuration, platform))
                            {
                                foreach (var files in refs.GetFiles(configuration, platform, parentComponents, parentDir))
                                {
                                    yield return files;
                                }
                            }

                            break;

                        case Files:

                            foreach (var files in this.GetProjectItems("CopyToOutputDirectory", "None").Where(p => p.Item2 != "Never"))
                            {
                                var destdir = GetAttribute(node, "Flatten", "false").ToLower() == "true"
                                    ? GetAttribute(node, "DestDir")
                                    : Path.Combine(GetAttribute(node, "DestDir"), Path.GetDirectoryName(files.Item1.EvaluatedInclude));

                                yield return new FileItem(Path.GetFullPath(Path.Combine(this.Project.DirectoryPath, files.Item1.EvaluatedInclude)),
                                    destdir,
                                    components,
                                    GetAttribute(node, "Tasks"));
                            }
                            break;

                        default:

                            break;

                    }
                }
            }
            else
            {
                Console.Error.WriteLine("File not found: <" + source + ">");
            }
        }

        internal IEnumerable<FileItem> GetItems(XmlNode node, string itemType, string option = null)
        {
            foreach (ProjectItem ppi in this.Project.Items.Where(i => i.ItemType == itemType
                && i.DirectMetadataCount > 0
                && i.DirectMetadata.All(m => m.Name != "Private")))
            {
                if (string.IsNullOrEmpty(option) || ppi.Metadata.Any(m => m.Name.ToLower() == option.ToLower()))
                {
                    var pm = ppi.Metadata.FirstOrDefault(m => m.Name == "HintPath");

                    if (pm != null)
                    {
                        yield return new FileItem(Path.GetFullPath(Path.Combine(this.Project.DirectoryPath, pm.EvaluatedValue)),
                            GetAttribute(node, "DestDir"),
                            GetAttribute(node.ParentNode, Components),
                            GetAttribute(node, "Tasks"));
                    }
                    else
                    {
                        yield return new FileItem(Path.GetFullPath(ppi.EvaluatedInclude),
                            GetAttribute(node, "DestDir"),
                            GetAttribute(node.ParentNode, Components),
                            GetAttribute(node, "Tasks"));
                    }
                }
            }
        }

        internal IEnumerable<ProjectProcessor> GetProjectReferences(string configuration, string platform)
        {
            using (var pc = new ProjectCollection())
            {
                foreach (ProjectItem ppi in this.Project.GetItems("ProjectReference"))
                {
                    yield return new ProjectProcessor(pc.LoadProject(Path.GetFullPath(Path.Combine(this.Project.DirectoryPath, ppi.EvaluatedInclude))), configuration, platform);
                }
            }
        }

        private static string GetAttribute(XmlNode n, string name, string defValue = "")
        {
            if (n.Attributes[name] != null)
            {
                return n.Attributes[name].Value;
            }
            else if (n.Attributes[name.ToLower()] != null)
            {
                return n.Attributes[name.ToLower()].Value;
            }
            else
            {
                return defValue;
            }
        }
    }
}

