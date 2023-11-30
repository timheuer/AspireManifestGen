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

        // get temp path to a file and rename the file extension to .json extension
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", "") + ".json");
        Process process = new Process();
        process.StartInfo.WorkingDirectory = projectPath;
        process.StartInfo.FileName = "dotnet.exe";
        process.StartInfo.Arguments = $"msbuild /t:GenerateAspireManifest /p:AspireManifestPublishOutputPath={tempPath}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Debug.WriteLine($"Error: {process.ExitCode}");
            return;
        }

        await VS.Documents.OpenAsync(tempPath);
    }
}
