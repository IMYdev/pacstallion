using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace pacstallion.Models;

public partial class PacstallPackage : ObservableObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "N/A";

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isInstalled;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string? _localVersion;
}