using AspireManifestGen.Options;
using CliWrap;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AspireManifestGen;

[Command(PackageIds.MyCommand)]
internal sealed class ManifestGen : BaseDynamicCommand<ManifestGen, Project>
{
    const string STATUS_MESSAGE = "Generating Aspire Manifest";

    protected override void BeforeQueryStatus(OleMenuCommand menuItem, EventArgs e, Project item)
    {
        menuItem.Supported = item.IsCapabilityMatch("Aspire");
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e, Project project)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        OutputWindowPane pane = OutputWindowManager.AspireOutputPane;

        var projectPath = Path.GetDirectoryName(project.FullPath);

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        var solutionPath = Path.GetDirectoryName(solution.FullPath);

        var options = await General.GetLiveInstanceAsync();

        string manifestPath;
        string manifestFileName = "aspire-manifest.json";

        if (options.UseTempFile)
        {
            manifestPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
        }
        else
        {
            manifestPath = Path.Combine(solutionPath, options.DefaultPath);
        }

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.Build);
        await VS.StatusBar.ShowProgressAsync(STATUS_MESSAGE, 1, 2);
        await pane.WriteLineAsync(OutputWindowManager.GenerateOutputMessage(STATUS_MESSAGE, "ManifestGen", LogLevel.Information));
        await pane.WriteLineAsync(OutputWindowManager.GenerateOutputMessage($"Generating using: msbuild /t:GenerateAspireManifest /p:AspireManifestPublishOutputPath={manifestPath}", "ManifestGen", LogLevel.Information));
        var result = await Cli.Wrap("dotnet")
            .WithArguments($"msbuild /t:GenerateAspireManifest /p:AspireManifestPublishOutputPath={manifestPath}")
            .WithWorkingDirectory(projectPath)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        // TODO: Need better error handling, issue #3
        if (result.ExitCode != 0)
        {
            await pane.WriteLineAsync(OutputWindowManager.GenerateOutputMessage($"Unable to create manifest:{stdErr}:{result.ExitCode}", "ManifestGen", LogLevel.Error));
            goto Cleanup;
        }
        else
        {
            await pane.WriteLineAsync(OutputWindowManager.GenerateOutputMessage($"Manifest created at {manifestPath}\\{manifestFileName}", "ManifestGen", LogLevel.Information)); 
        }

        try
        {
            await VS.Documents.OpenAsync(Path.Combine(manifestPath,manifestFileName));
        }
        catch (Exception ex)
        {
            await pane.WriteLineAsync(OutputWindowManager.GenerateOutputMessage($"Unable to open manifest:{ex.Message}", "ManifestGen", LogLevel.Error));
        }

    Cleanup:
        await VS.StatusBar.EndAnimationAsync(StatusAnimation.Build);
        await VS.StatusBar.ShowProgressAsync(STATUS_MESSAGE, 2, 2);
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
