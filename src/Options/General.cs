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
    [Description("The default path of the folder where the manifest file will be generated.")]
    [DefaultValue(".aspire")]
    public string DefaultPath { get; set; } = ".aspire";

    [Category("Manifest file")]
    [DisplayName("Relative to Solution or Project")]
    [Description("Choose whether relative to the Solution or Project")]
    [DefaultValue(RelativeTo.Project)]
    public RelativeTo RelativeTo { get; set; } = RelativeTo.Project;

    [Category("Manifest file")]
    [DisplayName("Use temporary file")]
    [Description("Use a temporary file on disk instead of project location. Note if this is true, Default path name and Relative path are ignored.")]
    [DefaultValue(true)]
    public bool UseTempFile { get; set; } = true;

    [Category("Manifest file")]
    [DisplayName("Enable azd debug output")]
    [Description("Enables azd debug output (--debug) to the infra synth")]
    [DefaultValue(false)]
    public bool AzdDebug { get; set; } = false;
}

public enum RelativeTo
{
    [Description("Project")]
    Project,
    [Description("Solution")]
    Solution
}