using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AspireManifestGen.Options;

internal partial class OptionsProvider
{
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<General> { }
}

public class General : BaseOptionModel<General>
{
    [Category("Manifest file")]
    [DisplayName("Default file name")]
    [Description("The default name of the manifest file to be generated")]
    [DefaultValue("aspire-manifest.json")]
    public string DefaultName { get; set; } = "aspire-manifest.json";

    [Category("Manifest file")]
    [DisplayName("Use temporary file")]
    [Description("Use a temporary file on disk instead of project location")]
    [DefaultValue(true)]
    public bool UseTempFile { get; set; } = true;

    [Category("Manifest file")]
    [DisplayName("Enable azd debug output")]
    [Description("Enables azd debug output (--debug) to the infra synth")]
    [DefaultValue(false)]
    public bool AzdDebug { get; set; } = false;
}