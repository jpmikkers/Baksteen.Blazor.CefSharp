using System.Runtime.InteropServices;
using System.Windows.Markup;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("c1c90a42-4073-447d-b0a4-846fe68c07a9")]

// XAML does not like dealing with nested namespaces, so the following attributes enables
// mapping them all to a single xml namespace. See also:
// https://learn.microsoft.com/en-us/dotnet/api/system.windows.markup.xmlnsdefinitionattribute?view=windowsdesktop-8.0
[assembly: XmlnsDefinition( xmlNamespace: "http://baksteen.com/xaml/blazor", clrNamespace: "Baksteen.Blazor.CefSharpWPF.Glue")]
[assembly: XmlnsDefinition( xmlNamespace: "http://baksteen.com/xaml/blazor", clrNamespace: "Baksteen.Blazor.CefSharpWPF")]
