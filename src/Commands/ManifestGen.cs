using AspireManifestGen.Options;
using CliWrap;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        OutputWindowPane pane = await VS.Windows.GetOutputWindowPaneAsync(Community.VisualStudio.Toolkit.Windows.VSOutputWindowPane.General);

        var projectPath = Path.GetDirectoryName(project.FullPath);

        var options = await General.GetLiveInstanceAsync();

        string manifestPath;
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
            await pane.WriteLineAsync($"[.NET Aspire]: Unable to create manifest:{stdErr}:{result.ExitCode}");
            goto Cleanup;
        }
        else
        {
            await pane.WriteLineAsync($"[.NET Aspire]: Manifest created at {manifestPath}");
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
