// This file contains common AssemblyVersion data to be shared across all projects in this solution.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
[assembly: AssemblyCompany("")]
[assembly: AssemblyDescription("Dapper.FastCRUD is built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings. VS2015 or an equivalent build environment is recommended.")]
[assembly: AssemblyProduct("Dapper.FastCRUD")]
[assembly: AssemblyCopyright("Copyright © 2021 Dapper.FastCRUD contributors")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle("Dapper.FastCRUD")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: ComVisible(false)]


#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Revision
//      Build
//
// You can specify all the values or you can default the Revision and Build Numbers by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: InternalsVisibleTo("Dapper.FastCrud.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
