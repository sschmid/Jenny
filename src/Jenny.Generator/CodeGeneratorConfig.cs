using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Serialization;
using DesperateDevs.Extensions;

namespace Jenny.Generator
{
    public class CodeGeneratorConfig : AbstractConfigurableConfig
    {
        public const string SearchPathsKey = "Jenny.SearchPaths";
        public const string PluginsPathsKey = "Jenny.Plugins";

        public const string PreProcessorsKey = "Jenny.PreProcessors";
        public const string DataProvidersKey = "Jenny.DataProviders";
        public const string CodeGeneratorsKey = "Jenny.CodeGenerators";
        public const string PostProcessorsKey = "Jenny.PostProcessors";

        public const string PortKey = "Jenny.Server.Port";
        public const string HostKey = "Jenny.Client.Host";

        public override Dictionary<string, string> DefaultProperties => new Dictionary<string, string>
        {
            {SearchPathsKey, string.Empty},
            {PluginsPathsKey, string.Empty},
            {PreProcessorsKey, string.Empty},
            {DataProvidersKey, string.Empty},
            {CodeGeneratorsKey, string.Empty},
            {PostProcessorsKey, string.Empty},
            {PortKey, "3333"},
            {HostKey, "localhost"}
        };

        readonly bool _minified;
        readonly bool _removeEmptyEntries;

        public CodeGeneratorConfig() : this(false, true) { }

        public CodeGeneratorConfig(bool minified, bool removeEmptyEntries)
        {
            _minified = minified;
            _removeEmptyEntries = removeEmptyEntries;
        }

        public string[] SearchPaths
        {
            get => _preferences[SearchPathsKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[SearchPathsKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public string[] Plugins
        {
            get => _preferences[PluginsPathsKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[PluginsPathsKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public string[] PreProcessors
        {
            get => _preferences[PreProcessorsKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[PreProcessorsKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public string[] DataProviders
        {
            get => _preferences[DataProvidersKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[DataProvidersKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public string[] CodeGenerators
        {
            get => _preferences[CodeGeneratorsKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[CodeGeneratorsKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public string[] PostProcessors
        {
            get => _preferences[PostProcessorsKey].FromCSV(_removeEmptyEntries).ToArray();
            set => _preferences[PostProcessorsKey] = value.ToCSV(_minified, _removeEmptyEntries);
        }

        public int Port
        {
            get => int.Parse(_preferences[PortKey]);
            set => _preferences[PortKey] = value.ToString();
        }

        public string Host
        {
            get => _preferences[HostKey];
            set => _preferences[HostKey] = value;
        }
    }
}
