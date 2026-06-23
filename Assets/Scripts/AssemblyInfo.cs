using System.Runtime.CompilerServices;

// Lets editor-only tooling (custom inspectors, importers) reach internal runtime types
// without making those types public. Both live in the default Unity assemblies.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
