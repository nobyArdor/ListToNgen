using System.Collections.Frozen;
using System.CommandLine;
using System.Text;
using System.Threading.Channels;
using MakeListToNgen.Config;
using MakeListToNgen.Utils;

namespace MakeListToNgen
{
    internal static class RootCommandBuilder
    {
        public static RootCommand Build()
        {
            var pathArgument = new Argument<IEnumerable<FileSystemInfo>>("paths",
                "Path to file or directory");

            var configPathOption = new Option<FileInfo>("--config",
                        () => new FileInfo(Path.Combine(@".\", "config.toml")),
                        "Full file path to config file")
                    .ExistingOnly()
                ;
            configPathOption.AddAlias("-c");

            var outPathOption = new Option<string>("--output",
                () => Path.Combine(@".\", "FileList.txt"),
                "File path to output file list");
            outPathOption.AddAlias("-o");

            var rootCommand = new RootCommand("Make list of acceptable to ngen files")
            {
                pathArgument,
                configPathOption,
                outPathOption
            };

            rootCommand.SetHandler(CommandHandler, pathArgument, configPathOption, outPathOption,
                new ConfigModelBinder(configPathOption));
            return rootCommand;
        }

        private static async Task CommandHandler(IEnumerable<FileSystemInfo> items, FileInfo _, string outPath,
            ConfigModel configSource)
        {
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
            {
                AllowSynchronousContinuations = true, FullMode = BoundedChannelFullMode.Wait, SingleReader = true,
                SingleWriter = false
            });
            ConfigModelValidator.ValidateConfig(configSource);
            var allowed = configSource.NgenDotnetVersions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            var skipped = configSource.SkipDotnetVersions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            var file = File.Create(outPath);
            await file.DisposeAsync();

            var writerTask = Task.Run(async () =>
            {
                await foreach (var text in channel.Reader.ReadAllAsync())
                {
                    await File.AppendAllLinesAsync(outPath, [text], Encoding.UTF8);
                }
            });
            var parseTasks = new List<Task>();
            foreach (var fileSystemInfo in items)
            {
                switch (fileSystemInfo)
                {
                    case FileInfo fi:
                        var singleParseTask =
                            CSharpDecompilerUtils.PutAllowToNgen(fi.FullName, allowed, skipped, channel.Writer);
                        parseTasks.Add(singleParseTask);
                        break;
                    case DirectoryInfo di:
                        var dirFiles = di.GetAcceptedFiles(".dll", ".exe");
                        parseTasks.AddRange(dirFiles.Select(fileInfo => CSharpDecompilerUtils.PutAllowToNgen(fileInfo.FullName, allowed, skipped, channel.Writer)));

                        break;
                    default:
                        await Console.Out.WriteLineAsync($"skip {fileSystemInfo.FullName}");
                        break;
                }
            }

            await Task.WhenAll(parseTasks);
            
            if (!channel.Writer.TryComplete())
            {
                throw new InvalidOperationException("Can not mark as completed");
            }
            await writerTask;
        }
    }
}
