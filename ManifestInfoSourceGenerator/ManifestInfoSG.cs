using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ManifestInfoSourceGenerator;

[Generator]
public class ManifestInfoSG : IIncrementalGenerator
{
    private static readonly string[] _msBuildPropertiesToGrab =
    [
        "TSName",
        "TSDescription",
        "TSVersion",
        "TSAuthor",
        "TSWebsite"
    ];
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        var tsPropertiesProvider = context.AnalyzerConfigOptionsProvider.Select((config, _) =>
        {
            var things = new Dictionary<string, string>();
            var options = config.GlobalOptions;

            foreach (var prop in _msBuildPropertiesToGrab)
            {
                if (options.TryGetValue($"build_property.{prop}", out var value))
                    things.Add(prop, value);
            }

            return things;
        });

        context.RegisterSourceOutput(tsPropertiesProvider, (ctx, dict) =>
        {
            ctx.AddSource("ManifestInfo.g.cs", SourceText.From(SourceGenerationHelper.BuildManifestInfo(dict), Encoding.UTF8));
        });
    }
}

public static class SourceGenerationHelper
{
    public static string BuildManifestInfo(Dictionary<string, string> dict)
    {
        var builder = new StringBuilder();
        builder.AppendLine("internal class ManifestInfo");
        builder.AppendLine("{");

        foreach (var kvp in dict)
        {
            var prop = kvp.Key;
            var value = kvp.Value;
            
            builder.AppendLine($"    internal const string {prop} = \"{value}\";");
        }

        builder.AppendLine("}");
        
        return builder.ToString();
    }
}