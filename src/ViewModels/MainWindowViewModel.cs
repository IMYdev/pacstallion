using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;
using pacstallion.Models;
using pacstallion.Services;

namespace pacstallion.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly PackageService _packageService;
    private List<PacstallPackage> _allPackages = new(); 

    public ObservableCollection<PacstallPackage> Packages { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "Ready.";
    [ObservableProperty] private bool _isSearchOpen;
    [ObservableProperty] private string _searchText = string.Empty;
    
    [ObservableProperty] private bool _showInstalledOnly;

    public MainWindowViewModel()
    {
        _packageService = new PackageService();
    }

    [RelayCommand]
    private void ToggleSearch()
    {
        IsSearchOpen = !IsSearchOpen;
        if (!IsSearchOpen) SearchText = string.Empty;
    }

    [RelayCommand]
    private void ToggleInstalledFilter()
    {
        ShowInstalledOnly = !ShowInstalledOnly;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnShowInstalledOnlyChanged(bool value) => ApplyFilters();

    private void ApplyFilters()
    {
        var filtered = _allPackages.AsEnumerable();

        if (ShowInstalledOnly)
            filtered = filtered.Where(p => p.IsInstalled);

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Packages.Clear();
        foreach (var p in filtered) Packages.Add(p);
    }

    [RelayCommand]
    private async Task FetchPackages()
    {
        if (!await _packageService.IsPacstallInstalledAsync())
        {
            StatusMessage = "Error: Pacstall is not installed.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Fetching packages...";

        var localPackageNames = await _packageService.GetInstalledPackagesAsync();
        var localNamesLower = new HashSet<string>(localPackageNames.Select(n => n.ToLower()));
        
        var remotePackages = await _packageService.GetRepologyPackagesAsync();
        var remoteNames = new HashSet<string>(remotePackages.Select(p => p.Name.ToLower()));

        StatusMessage = "Checking for updates...";
        var versionTasks = localPackageNames.Select(async name => 
        {
            var version = await _packageService.GetPackageVersionAsync(name);
            return (Name: name.ToLower(), Version: version);
        });
        
        var localVersions = (await Task.WhenAll(versionTasks))
            .Where(x => x.Version != null)
            .ToDictionary(x => x.Name, x => x.Version!);

        foreach (var pkg in remotePackages)
        {
            var nameLower = pkg.Name.ToLower();
            if (localNamesLower.Contains(nameLower))
            {
                pkg.IsInstalled = true;
                if (localVersions.TryGetValue(nameLower, out var localVer))
                {
                    pkg.LocalVersion = localVer;
                    
                    var normalizedLocal = localVer.Replace("-pacstall", "-");
                    var normalizedRemote = pkg.Version.Replace("-pacstall", "-");

                    if (localVer.Contains("~git") && localVer.Contains("-pacstall"))
                    {
                            var hash = localVer.Split("~git").Last();
                            var pacstallPart = localVer.Split("~git").First().Split("-").Last();
                            if (pacstallPart.StartsWith("pacstall"))
                            {
                                var num = pacstallPart.Replace("pacstall", "");
                                normalizedLocal = $"{hash}-{num}";
                            }
                    }
                    
                    pkg.IsUpdateAvailable = normalizedLocal != normalizedRemote;
                }
            }
        }

        foreach (var localPkgName in localPackageNames)
        {
            var nameLower = localPkgName.ToLower();
            if (!remoteNames.Contains(nameLower))
            {
                remotePackages.Add(new PacstallPackage
                {
                    Name = localPkgName,
                    IsInstalled = true,
                    Description = "Installed locally (not found in remote repo)",
                    Version = "Local",
                    LocalVersion = localVersions.GetValueOrDefault(nameLower)
                });
            }
        }

        _allPackages = remotePackages;
        StatusMessage = $"Loaded {_allPackages.Count} packages.";

        ApplyFilters();
        IsLoading = false;
    }
    [ObservableProperty] 
    private PacstallPackage? _selectedPackage;
    partial void OnSelectedPackageChanged(PacstallPackage? value)
    {
        if (value == null) {
            SelectedDetails = null;
            return;
        }

        Task.Run(async () => {
            IsDetailsLoading = true;
            var details = await _packageService.GetPackageDetailsAsync(value.Name, value.IsInstalled);
            
            Dispatcher.UIThread.Invoke(() => {
                SelectedDetails = details;
                IsDetailsLoading = false;
            });
        });
    }
    [ObservableProperty] private PackageDetails? _selectedDetails;
    [ObservableProperty] private bool _isDetailsLoading;

    [RelayCommand]
    private async Task InstallPackage()
    {
        if (SelectedPackage == null) return;
        IsLoading = true;
        StatusMessage = $"Installing {SelectedPackage.Name}...";
        await _packageService.RunPackageActionAsync(SelectedPackage.Name, "install");
        await FetchPackages();
    }

    [RelayCommand]
    private async Task UninstallPackage()
    {
        if (SelectedPackage == null) return;
        IsLoading = true;
        StatusMessage = $"Uninstalling {SelectedPackage.Name}...";
        await _packageService.RunPackageActionAsync(SelectedPackage.Name, "remove");
        await FetchPackages();
    }
}