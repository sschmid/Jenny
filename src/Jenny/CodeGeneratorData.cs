using System.Collections.Generic;
using System.Text.RegularExpressions;
using DesperateDevs.Extensions;

namespace Jenny
{
    public class CodeGeneratorData : Dictionary<string, object>
    {
        const string VariablePattern = @"\${(.+?)}";

        public CodeGeneratorData() { }

        public CodeGeneratorData(CodeGeneratorData data) : base(data) { }

        public string ReplacePlaceholders(string template) => Regex.Replace(template, VariablePattern,
            match =>
            {
                var split = match.Groups[1].Value.Split(':');
                return TryGetValue(split[0], out var value)
                    ? split.Length == 1
                        ? value.ToString()
                        : split[1] switch
                        {
                            "lower" => value.ToString().ToLower(),
                            "upper" => value.ToString().ToUpper(),
                            "lowerFirst" => value.ToString().ToLowerFirst(),
                            "upperFirst" => value.ToString().ToUpperFirst(),
                            "foreach" => ForEach((IEnumerable<object>)value, split[2]),
                            _ => value.ToString()
                        }
                    : match.Value;
            });

        static string ForEach(IEnumerable<object> values, string template)
        {
            var result = string.Empty;
            foreach (var value in values)
                result += template
                    .Replace("$item", value.ToString())
                    .Replace("\\n", "\n");

            return result;
        }
    }
}
