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
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "Projects and Solutions", ".NET Aspire", 0, 0, true)]
[ProvideProfile(typeof(OptionsProvider.GeneralOptions), "Projects and Solutions", ".NET Aspire", 0, 0, true)]

public sealed class AspireManifestGenPackage : ToolkitPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();
    }
}