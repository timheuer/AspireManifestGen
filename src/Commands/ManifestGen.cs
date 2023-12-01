using AspireManifestGen.Options;
using System.Diagnostics;
using System.IO;

namespace AspireManifestGen;

[Command(PackageIds.MyCommand)]
internal sealed class ManifestGen : BaseCommand<ManifestGen>
{
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var project = await VS.Solutions.GetActiveProjectAsync();
        var projectPath = Path.GetDirectoryName(project.FullPath);

        var options = await General.GetLiveInstanceAsync();
        // get temp path to a file and rename the file extension to .json extension
        //var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", "") + ".json");

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
            Debug.WriteLine($"Error: {process.ExitCode}");
            return;
        }

        await VS.Documents.OpenAsync(manifestPath);
    }
}
