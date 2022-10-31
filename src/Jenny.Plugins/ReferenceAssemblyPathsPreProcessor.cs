using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DesperateDevs.Serialization;

namespace Jenny.Plugins
{
    public class ReferenceAssemblyPathsPreProcessor : IPreProcessor, IConfigurable
    {
        public string Name => "Fix Referenced Assemblies";
        public int Order => 0;
        public bool RunInDryMode => true;

        public Dictionary<string, string> DefaultProperties => _projectPathConfig.DefaultProperties;

        readonly ProjectPathConfig _projectPathConfig = new ProjectPathConfig();

        public void Configure(Preferences preferences)
        {
            _projectPathConfig.Configure(preferences);
        }

        public void PreProcess()
        {
            var doc = XDocument.Load(_projectPathConfig.ProjectPath);
            if (AddProperties(doc.Root))
                doc.Save(_projectPathConfig.ProjectPath);
        }

        static bool AddProperties(XElement root)
        {
            XNamespace xmlns = root.Name.NamespaceName;
            var changed = SetOrUpdateProperty(root, xmlns, "_TargetFrameworkDirectories", typeof(ReferenceAssemblyPathsPreProcessor).FullName);
            changed |= SetOrUpdateProperty(root, xmlns, "_FullFrameworkReferenceAssemblyPaths", typeof(ReferenceAssemblyPathsPreProcessor).FullName);
            changed |= SetOrUpdateProperty(root, xmlns, "DisableHandlePackageFileConflicts", "true");
            return changed;
        }

        static bool SetOrUpdateProperty(XElement root, XNamespace xmlns, string name, string defaultValue)
        {
            var elements = root.Elements(xmlns + "PropertyGroup").Elements(xmlns + name).ToList();
            if (elements.Any())
            {
                var updated = false;
                foreach (var element in elements)
                {
                    if (element.Value != defaultValue)
                    {
                        element.SetValue(defaultValue);
                        updated = true;
                    }
                }

                return updated;
            }

            var propertyGroup = root.Elements(xmlns + "PropertyGroup")
                .FirstOrDefault(e => !e.Attributes(xmlns + "Condition").Any());

            if (propertyGroup == null)
            {
                propertyGroup = new XElement(xmlns + "PropertyGroup");
                root.AddFirst(propertyGroup);
            }

            propertyGroup.Add(new XElement(xmlns + name, defaultValue));
            return true;
        }
    }
}
