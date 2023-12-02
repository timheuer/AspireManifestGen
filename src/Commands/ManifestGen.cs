using AspireManifestGen.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AspireManifestGen;

[Command(PackageIds.MyCommand)]
internal sealed class ManifestGen : BaseDynamicCommand<ManifestGen, Project>
{
    protected override void BeforeQueryStatus(OleMenuCommand menuItem, EventArgs e, Project item)
    {
        menuItem.Supported = item.IsCapabilityMatch("Aspire");
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e, Project project)
    {
        //var project = await VS.Solutions.GetActiveProjectAsync();
        var projectPath = Path.GetDirectoryName(project.FullPath);

        var options = await General.GetLiveInstanceAsync();

        var manifestPath = string.Empty;

        if (options.UseTempFile)
        {
            // get temp path to a file and rename the file extension to .json extension
            manifestPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", "") + ".json");
        }
        else
        {
            manifestPath = Path.Combine(projectPath, options.DefaultName);
        }

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.Build);
        await VS.StatusBar.ShowProgressAsync("Generating Aspire Manifest", 1, 2);
        Process process = new Process();
        process.StartInfo.WorkingDirectory = projectPath;
        process.StartInfo.FileName = "dotnet.exe";
        process.StartInfo.Arguments = $"msbuild /t:GenerateAspireManifest /p:AspireManifestPublishOutputPath={manifestPath}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);
        process.Start();
        process.WaitForExit();

        // TODO: Need better error handling, issue #3
        if (process.ExitCode != 0)
        {
            var errorString = await process.StandardError.ReadToEndAsync();
            Debug.WriteLine($"Error: {errorString}");
            goto Cleanup;
        }

        await VS.Documents.OpenAsync(manifestPath);

    Cleanup:
        await VS.StatusBar.EndAnimationAsync(StatusAnimation.Build);
        await VS.StatusBar.ShowProgressAsync("Generating Aspire Manifest", 2, 2);
        return;
    }

    protected override IReadOnlyList<Project> GetItems()
    {
        return ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            List<Project> items = [await VS.Solutions.GetActiveProjectAsync()];
            return items;
        });
    }
}
