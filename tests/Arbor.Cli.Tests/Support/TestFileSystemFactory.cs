using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Arbor.Cli.Tests.Support;

internal static class TestFileSystemFactory
{
    public static (MockFileSystem FileSystem, string RootPath) Create()
    {
        var rootPath = Path.DirectorySeparatorChar == '\\'
            ? @"C:\workspace"
            : "/workspace";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), rootPath);

        fileSystem.AddDirectory(rootPath);

        var srcPath = fileSystem.Path.Combine(rootPath, "src");
        var testsPath = fileSystem.Path.Combine(rootPath, "tests");
        var nestedPath = fileSystem.Path.Combine(testsPath, "Nested");

        fileSystem.AddDirectory(srcPath);
        fileSystem.AddDirectory(testsPath);
        fileSystem.AddDirectory(nestedPath);

        fileSystem.AddFile(fileSystem.Path.Combine(rootPath, "README.md"), new MockFileData(string.Empty));
        fileSystem.AddFile(fileSystem.Path.Combine(srcPath, "Program.cs"), new MockFileData(string.Empty));
        fileSystem.AddFile(fileSystem.Path.Combine(testsPath, "UnitTest.cs"), new MockFileData(string.Empty));

        return (fileSystem, rootPath);
    }
}
