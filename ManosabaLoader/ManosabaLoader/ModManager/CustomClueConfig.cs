using System.Collections.Generic;

namespace ManosabaLoader.ModManager;

public class CustomClueVersion
{
    public int Version { get; set; }
    public Dictionary<string, string> LocalizationName { get; set; } = new();
    public Dictionary<string, string> LocalizationDesc { get; set; } = new();
}

public class CustomClueItem
{
    public string Id { get; set; } = "";
    public string Thumbnail { get; set; } = "";
    public List<CustomClueVersion> Versions { get; set; } = [];
}