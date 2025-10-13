using System.IO;
using Arbor.Core.TreeBuilding;
using Spectre.Console;

namespace Arbor.Cli.Exporting;

public interface ITreeExporter
{
    void Export(DirectoryTreeResult result, TreeOptions options, IAnsiConsole console, TextWriter? writer);
}
