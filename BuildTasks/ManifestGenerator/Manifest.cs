using System.Collections.Generic;

namespace ManifestGenerator;

public class Manifest
{
    // ReSharper disable InconsistentNaming
    public string name { get; set; }
    public string author { get; set; }
    public string version_number { get; set; }
    public string website_url { get; set; }
    public string description { get; set; }
    public List<string> dependencies { get; set; } = new();
    // ReSharper restore InconsistentNaming
}