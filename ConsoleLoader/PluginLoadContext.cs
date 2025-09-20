namespace ConsoleLoader;

using System;
using System.Reflection;
using System.Runtime.Loader;

public class PluginLoadContext() : AssemblyLoadContext(isCollectible: true)
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }
}