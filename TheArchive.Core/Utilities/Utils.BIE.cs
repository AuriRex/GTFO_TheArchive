// This file is licensed under the LGPL 2.1 LICENSE
// See LICENSE_BepInEx in the projects root folder
// Original code from https://github.com/BepInEx/BepInEx

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;

namespace TheArchive.Utilities;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Utils
{
    
    /// <summary>
    ///     Convert the given array to a hex string.
    /// </summary>
    /// <param name="data">Bytes to convert.</param>
    /// <returns>Bytes reinterpreted as a hex number.</returns>
    public static string ByteArrayToString(byte[] data)
    {
        var builder = new StringBuilder(data.Length * 2);

        foreach (var b in data)
            builder.AppendFormat("{0:x2}", b);

        return builder.ToString();
    }
    
    /// <summary>
    ///     Compute a SHA256 hash of the given stream.
    /// </summary>
    /// <param name="stream">Stream to hash</param>
    /// <returns>SHA256 hash as a hex string</returns>
    public static string HashStream(Stream stream)
    {
        using var sha256 = SHA256.Create();
        
        var buf = new byte[4096];
        int read;
        while ((read = stream.Read(buf, 0, buf.Length)) > 0)
        {
            sha256.TransformBlock(buf, 0, read, buf, 0);
        }
        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return ByteArrayToString(sha256.Hash);
    }

    /// <summary>
    ///     Try to resolve and load the given assembly DLL.
    /// </summary>
    /// <param name="assemblyName">Name of the assembly, of the type <see cref="AssemblyName" />.</param>
    /// <param name="directory">Directory to search the assembly from.</param>
    /// <param name="assembly">The loaded assembly.</param>
    /// <returns>True, if the assembly was found and loaded. Otherwise, false.</returns>
    public static bool TryResolveDllAssembly<T>(AssemblyName assemblyName, string directory, Func<string, T> loader, out T assembly) where T : class
    {
        assembly = null;

        var potentialDirectories = new List<string> { directory };

        if (!Directory.Exists(directory))
            return false;

        potentialDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));

        foreach (var subDirectory in potentialDirectories)
        {
            var potentialPaths = new[]
            {
                $"{assemblyName.Name}.dll",
                $"{assemblyName.Name}.exe"
            };

            foreach (var potentialPath in potentialPaths)
            {
                var path = Path.Combine(subDirectory, potentialPath);

                if (!File.Exists(path))
                    continue;

                try
                {
                    assembly = loader(path);
                }
                catch (Exception)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Try to resolve and load the given assembly DLL.
    /// </summary>
    /// <param name="assemblyName">Name of the assembly, of the type <see cref="AssemblyName" />.</param>
    /// <param name="directory">Directory to search the assembly from.</param>
    /// <param name="readerParameters">Reader parameters that contain possible custom assembly resolver.</param>
    /// <param name="assembly">The loaded assembly.</param>
    /// <returns>True, if the assembly was found and loaded. Otherwise, false.</returns>
    public static bool TryResolveDllAssembly(AssemblyName assemblyName, string directory, ReaderParameters readerParameters, out AssemblyDefinition assembly)
    {
        return TryResolveDllAssembly(assemblyName, directory, s => AssemblyDefinition.ReadAssembly(s, readerParameters), out assembly);
    }

    /// <summary>
    ///     Try to parse given string as an assembly name
    /// </summary>
    /// <param name="fullName">Fully qualified assembly name</param>
    /// <param name="assemblyName">Resulting <see cref="AssemblyName" /> instance</param>
    /// <returns><c>true</c>, if parsing was successful, otherwise <c>false</c></returns>
    /// <remarks>
    ///     On some versions of mono, using <see cref="Assembly.GetName()" /> fails because it runs on unmanaged side
    ///     which has problems with encoding.
    ///     Using <see cref="AssemblyName" /> solves this by doing parsing on managed side instead.
    /// </remarks>
    public static bool TryParseAssemblyName(string fullName, out AssemblyName assemblyName)
    {
        try
        {
            assemblyName = new AssemblyName(fullName);
            return true;
        }
        catch (Exception)
        {
            assemblyName = null;
            return false;
        }
    }

    /// <summary>
    ///     Sorts a given dependency graph using a direct toposort, reporting possible cyclic dependencies.
    /// </summary>
    /// <param name="nodes">Nodes to sort</param>
    /// <param name="dependencySelector">Function that maps a node to a collection of its dependencies.</param>
    /// <typeparam name="TNode">Type of the node in a dependency graph.</typeparam>
    /// <returns>Collection of nodes sorted in the order of least dependencies to the most.</returns>
    /// <exception cref="Exception">Thrown when a cyclic dependency occurs.</exception>
    public static IEnumerable<TNode> TopologicalSort<TNode>(IEnumerable<TNode> nodes,
        Func<TNode, IEnumerable<TNode>> dependencySelector)
    {
        var sorted_list = new List<TNode>();

        var visited = new HashSet<TNode>();
        var sorted = new HashSet<TNode>();

        foreach (var input in nodes)
        {
            var currentStack = new Stack<TNode>();
            if (!Visit(input, currentStack))
                throw new Exception("Cyclic Dependency:\r\n" + currentStack.Select(x => $" - {x}") //append dashes
                    .Aggregate((a, b) =>
                        $"{a}\r\n{b}")); //add new lines inbetween
        }


        return sorted_list;

        bool Visit(TNode node, Stack<TNode> stack)
        {
            if (visited.Contains(node))
            {
                if (!sorted.Contains(node)) return false;
            }
            else
            {
                visited.Add(node);
                stack.Push(node);
                if (dependencySelector(node).Any(dep => !Visit(dep, stack)))
                    return false;

                sorted.Add(node);
                sorted_list.Add(node);

                stack.Pop();
            }

            return true;
        }
    }

}