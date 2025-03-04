using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;

namespace ManifestGenerator;

public class ManifestGeneratorTask : Task
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Author { get; set; }
    
    [Required]
    public string Version { get; set; }
    
    [Required]
    public string WebsiteURL { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public ITaskItem[] Dependencies { get; set; }
    
    [Required]
    public string OutputPath { get; set; }
    
    private readonly HashSet<string> _dependencies = new();
    
    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(Name)
            || string.IsNullOrWhiteSpace(Author)
            || string.IsNullOrWhiteSpace(Version)
            || string.IsNullOrWhiteSpace(Description))
        {
            Log.LogMessage(MessageImportance.High, "Aborting manifest generation. Important values are not set.");
            return true;
        }
        
        Log.LogMessage(MessageImportance.High, $"{nameof(ManifestGeneratorTask)}: Name:{Name}, Author:{Author}, Version:{Version}, WebsiteURL:{WebsiteURL}, Description:{Description}");

        if (Dependencies != null && Dependencies.Length > 0)
        {
            foreach (var dependency in Dependencies)
            {
                _dependencies.Add(dependency.ItemSpec);
            }
        }
        
        Log.LogMessage(MessageImportance.High, $"{nameof(ManifestGeneratorTask)}: DependencyCount: {_dependencies.Count}; {string.Join(", ", _dependencies)}");

        var manifest = new Manifest
        {
            name = Name,
            author = Author,
            description = Description,
            website_url = WebsiteURL,
            version_number = Version,
            dependencies = _dependencies.ToList()
        };

        var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
        
        var path = Path.Combine(OutputPath, "manifest.json");
        
        File.WriteAllText(path, json);
        Log.LogMessage(MessageImportance.High, $"{nameof(ManifestGeneratorTask)}: Written manifest file to output path: {path}");
        
        return true;
    }
}