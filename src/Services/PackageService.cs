using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using pacstallion.Models;

namespace pacstallion.Services;

public class PackageService
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private const string BaseUrl = "https://pacstall.dev/api";

    public PackageService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "pacstallion");
    }

    public async Task<bool> IsPacstallInstalledAsync()
    {
        try
        {
            var result = await RunCommandAsync("pacstall", "--version");
            return result.ExitCode == 0;
        }
        catch { return false; }
    }

    public async Task<List<string>> GetInstalledPackagesAsync()
    {
        try
        {
            var (exitCode, output) = await RunCommandAsync("pacstall", "-L");
            if (exitCode != 0) return new List<string>();

            return output.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .ToList();
        }
        catch { return new List<string>(); }
    }

    public async Task<string?> GetPackageVersionAsync(string name)
    {
        try
        {
            string arg = name.EndsWith("-deb") ? "pacversion" : "version";
            var (exitCode, output) = await RunCommandAsync("pacstall", $"-Ci {name} {arg}");
            return exitCode == 0 ? output.Trim() : null;
        }
        catch { return null; }
    }

    public async Task<List<PacstallPackage>> GetRepologyPackagesAsync()
    {
        try
        {
            var data = await _httpClient.GetFromJsonAsync<List<PacstallPackage>>($"{BaseUrl}/repology");
            return data ?? new();
        }
        catch { return new(); }
    }

    private async Task<(int ExitCode, string Output)> RunCommandAsync(string fileName, string args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (process.ExitCode, output);
    }

    public async Task RunPackageActionAsync(string name, string action)
    {
        string arg = action == "install" ? "-I" : "-R";
        string command = $"pacstall {arg} '{name}'; echo; echo 'Process finished. Press Enter to close...'; read";
        
        string terminal = "";
        string terminalArgs = "";

        if (System.IO.File.Exists("/usr/bin/x-terminal-emulator"))
        {
            terminal = "/usr/bin/x-terminal-emulator";
            terminalArgs = $"-e bash -c \"{command}\"";
        }
        else if (System.IO.File.Exists("/usr/bin/konsole"))
        {
            terminal = "/usr/bin/konsole";
            terminalArgs = $"-e bash -c \"{command}\"";
        }
        else if (System.IO.File.Exists("/usr/bin/gnome-terminal"))
        {
            terminal = "/usr/bin/gnome-terminal";
            terminalArgs = $"-- bash -c \"{command}\"";
        }
        else if (System.IO.File.Exists("/usr/bin/xfce4-terminal"))
        {
            terminal = "/usr/bin/xfce4-terminal";
            terminalArgs = $"-e \"bash -c '{command}'\"";
        }
        else if (System.IO.File.Exists("/usr/bin/xterm"))
        {
            terminal = "/usr/bin/xterm";
            terminalArgs = $"-e \"bash -c '{command}'\"";
        }
        else if (System.IO.File.Exists("/usr/bin/alacritty"))
        {
            terminal = "/usr/bin/alacritty";
            terminalArgs = $"-e bash -c \"{command}\"";
        }
        else if (System.IO.File.Exists("/usr/bin/kitty"))
        {
            terminal = "/usr/bin/kitty";
            terminalArgs = $"-e bash -c \"{command}\"";
        }
        else
        // Just a fallback, will probably not work... I don't know who doesn't have a terminal installed on linux lol
        {
            terminal = "/usr/bin/bash";
            terminalArgs = $"-c \"{command}\"";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = terminal,
            Arguments = terminalArgs,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error launching terminal: {ex.Message}");
        }
    }

    public async Task<PackageDetails?> GetPackageDetailsAsync(string name, bool isInstalled = false)
    {
        try
        {
            string url = $"{BaseUrl}/packages/{name}";
            var details = await _httpClient.GetFromJsonAsync<PackageDetails>(url);
            return details;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching details for {name}: {ex.Message}");
            
            if (isInstalled)
            {
                var version = await GetPackageVersionAsync(name);
                return new PackageDetails 
                { 
                    PackageName = name, 
                    Version = version ?? "Unknown",
                    Description = "Local package info (remote metadata unavailable)"
                };
            }
            return null;
        }
    }
}