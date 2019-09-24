using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "084da669-6eb9-4f49-8f81-bc480089da50" )]


[assembly: InternalsVisibleTo( "MorpheusScript" )]
[assembly: InternalsVisibleTo( "EntityFrameworkRepository" )]

[assembly: InternalsVisibleTo( "UnitTests" )]
[assembly: InternalsVisibleTo( "EF_SEF_UnitTests" )]
[assembly: InternalsVisibleTo( "MSUnitTests" )]
