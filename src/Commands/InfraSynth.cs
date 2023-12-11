using AspireManifestGen.Options;
using CliWrap;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AspireManifestGen;

[Command(PackageIds.InfraSynth)]
internal sealed class InfraSynth : BaseDynamicCommand<InfraSynth, Project>
{
    const string STATUS_MESSAGE = "Synthesizing Azure infrastructure";

    protected override void BeforeQueryStatus(OleMenuCommand menuItem, EventArgs e, Project item)
    {
        menuItem.Supported = item.IsCapabilityMatch("Aspire");
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e, Project project)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        OutputWindowPane pane = await VS.Windows.GetOutputWindowPaneAsync(Community.VisualStudio.Toolkit.Windows.VSOutputWindowPane.General);

        var projectPath = FindAzureYaml(project.FullPath);

        await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);
        await VS.StatusBar.ShowProgressAsync(STATUS_MESSAGE, 1, 2);

        var result = await Cli.Wrap("azd")
            .WithArguments($"infra synth --force --no-prompt")
            .WithWorkingDirectory(projectPath)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        var stdErr = stdErrBuffer.ToString();

        if (result.ExitCode != 0)
        {
            await pane.WriteLineAsync($"[AZD]: Unable to synthesize infrastructure:{stdErr}:{result.ExitCode}");
            goto Cleanup;
        }
        else
        {
            await pane.WriteLineAsync($"[AZD]: infra synth completed");
        }

        await VS.Documents.OpenAsync(Path.Combine(projectPath, "infra", "resources.bicep"));

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

    public static string FindAzureYaml(string startDirectory)
    {
        var filePath = Path.Combine(startDirectory, "azure.yaml");
        if (File.Exists(filePath))
        {
            return Path.GetDirectoryName(filePath);
        }

        var parentDirectory = Directory.GetParent(startDirectory);
        if (parentDirectory == null)
        {
            return null; // File not found in any parent directories
        }

        return FindAzureYaml(parentDirectory.FullName);
    }

}
