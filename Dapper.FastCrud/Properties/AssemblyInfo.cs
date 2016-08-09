using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Dapper.FastCrud")]
[assembly: AssemblyDescription("A simple and fast micro-orm for Dapper.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Dapper.FastCrud")]
[assembly: AssemblyCopyright("Copyright © Dan Cristiu 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Read from project.json
// [assembly: AssemblyVersion("1.0.0.0")]

// Read from project.json
// [assembly: AssemblyFileVersion("1.0.0.0")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#if NET_46_OR_GREATER
[assembly: Guid("4df4976a-fb18-4466-ba8f-0a48e3f11b01")]  
#else
[assembly: Guid("0044043b-c8ec-497c-8b58-abe028014d24")]
#endif

[assembly:InternalsVisibleTo("Dapper.FastCrud.Tests")]

