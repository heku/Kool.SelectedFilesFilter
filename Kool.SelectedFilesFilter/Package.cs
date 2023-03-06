using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace Kool.SelectedFilesFilter;

[Guid(Ids.PACKAGE)]
[InstalledProductRegistration("#110", "#112", VERSION, IconResourceID = 400)]
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideMenuResource("Menus.ctmenu", 1)]
public sealed class Package : ToolkitPackage
{
    internal const string VERSION = "0.0.0";
    internal const string NAME = "Selected Files Filter";
    internal const string URL = "https://github.com/heku/kool.selectedfilesfilter";
}