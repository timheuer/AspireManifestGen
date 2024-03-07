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
    [DisplayName("Default path name")]
    [Description("The default path of the folder where the manifest file will be generated. This will be at the solution root.")]
    [DefaultValue(".aspire")]
    public string DefaultPath { get; set; } = ".aspire";

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