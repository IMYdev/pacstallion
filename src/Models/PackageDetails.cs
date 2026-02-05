using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pacstallion.Models;

public class Dependency
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class PackageDetails
{
    [JsonPropertyName("packageName")] 
    public string? PackageName { get; set; }

    [JsonPropertyName("prettyName")] 
    public string? PrettyName { get; set; }

    [JsonPropertyName("description")] 
    public string? Description { get; set; }

    [JsonPropertyName("version")] 
    public string? Version { get; set; }

    [JsonPropertyName("homepage")] 
    public string? Homepage { get; set; }

    [JsonPropertyName("maintainers")] 
    public List<string>? Maintainers { get; set; }

    [JsonPropertyName("architectures")] 
    public List<string>? Architectures { get; set; }

    [JsonPropertyName("runtimeDependencies")] 
    public List<Dependency> RuntimeDependencies { get; set; } = new();

    [JsonPropertyName("pacstallDependencies")] 
    public List<Dependency> PacstallDependencies { get; set; } = new();

    [JsonPropertyName("buildDependencies")] 
    public List<Dependency> BuildDependencies { get; set; } = new();

    public bool HasDependencies => (RuntimeDependencies?.Count > 0) || (PacstallDependencies?.Count > 0) || (BuildDependencies?.Count > 0);
    public string ArchDisplay => Architectures != null ? string.Join(", ", Architectures) : "any";
}