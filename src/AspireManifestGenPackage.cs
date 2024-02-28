global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using AspireManifestGen.Options;

namespace AspireManifestGen;
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.AspireManifestGenString)]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), ".NET Aspire", "Manifest", 0, 0, true)]
[ProvideProfile(typeof(OptionsProvider.GeneralOptions), ".NET Aspire", "Manifest", 0, 0, true)]
[ProvideAutoLoad(UIContextGuid, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideUIContextRule(UIContextGuid,
    name: "Show on Aspire Project",
    expression: "Aspire",
    termNames: ["Aspire"],
    termValues: ["ActiveProjectCapability:Aspire"])]
public sealed class AspireManifestGenPackage : ToolkitPackage
{
    public const string UIContextGuid = "F686D1D0-9DDF-47DB-A0DC-59032E168F69";

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        OutputWindowManager.AspireOutputPane = await VS.Windows.CreateOutputWindowPaneAsync(".NET Aspire", true);
        await this.RegisterCommandsAsync();
    }
}