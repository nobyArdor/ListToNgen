using ListToNgen.Config;
using ListToNgen.Utils;
using System.Collections.Frozen;
using System.CommandLine;
using System.Text;
using System.Threading.Channels;
using Tomlyn;

namespace ListToNgen
{
    internal static class RootCommandBuilder
    {
        public static RootCommand Build()
        {
            var pathArgument = new Argument<IEnumerable<FileSystemInfo>>("paths")
            {
                Description = "Path to file or directory",
                Arity = ArgumentArity.OneOrMore
            };
            pathArgument = pathArgument.AcceptLegalFilePathsOnly();

          var configPathOption = new Option<FileInfo>("--config", "-c")
                {
                    Description = "Full file path to config file",
                    Arity = ArgumentArity.ZeroOrOne,
                    DefaultValueFactory = ar =>
                    {
                        if (ar.Tokens.Count == 0)
                            return new FileInfo(Path.Combine(@".\", "config.toml"));

                        var filePath = ar.Tokens.Single().Value;
                        if (!Path.IsPathFullyQualified(filePath))
                            filePath = Path.GetFullPath(filePath, AppContext.BaseDirectory);

                        return new FileInfo(filePath);
                    }
            };
            configPathOption = configPathOption.AcceptLegalFilePathsOnly().AcceptExistingOnly()
                ;

            var outPathOption = new Option<string>("--output", "-o")
            {
                Description = "File path to output file list",
                Arity = ArgumentArity.ZeroOrOne,
                DefaultValueFactory = ar =>
                {
                    if (ar.Tokens.Count == 0)
                        return Path.Combine(@".\", "FileList.txt");

                    var filePath = ar.Tokens.Single().Value;
                    if (!Path.IsPathFullyQualified(filePath))
                        filePath = Path.GetFullPath(filePath, AppContext.BaseDirectory);

                    return filePath;
                }
            };


            var rootCommand = new RootCommand("Make list of acceptable to ngen files");
            rootCommand.Options.Add(configPathOption);
            rootCommand.Options.Add(outPathOption);
            rootCommand.Arguments.Add(pathArgument);

            rootCommand.TreatUnmatchedTokensAsErrors = true;
            rootCommand.SetAction(async (r, _) =>
            {
                var configPath = r.GetRequiredValue(configPathOption);
                var config = ParseConfigModel(configPath);
                var items = r.GetRequiredValue(pathArgument);
                var outPath = r.GetRequiredValue(outPathOption);
                await Console.Out.WriteLineAsync("ListToNgen Running");
                await CommandHandler(items, outPath, config);
            });
            return rootCommand;
        }

        private static ConfigModel ParseConfigModel(FileInfo configPath)
        {
            if (!configPath.Exists)
            {
                throw new InvalidOperationException($"Config file is not exists {configPath}");
            }

            var text = File.ReadAllText(configPath.FullName, Encoding.UTF8);
            if (Toml.TryToModel<ConfigModel>(text, out var model, out var bag))
                return model;

            if (bag.HasErrors)
                throw new InvalidOperationException($"""
                                                     Can not parse config {configPath}
                                                     {bag}
                                                     """);

            var errorText = string.Join(".", bag.Select(x => x.Message));
            throw new InvalidOperationException(errorText);
        }

        private static async Task CommandHandler(IEnumerable<FileSystemInfo> items, string outPath,
            ConfigModel configSource)
        {
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
            {
                AllowSynchronousContinuations = true, FullMode = BoundedChannelFullMode.Wait, SingleReader = true,
                SingleWriter = false
            });
            ConfigModelValidator.ValidateConfig(configSource);
            var allowed = (configSource.NgenDotnetVersions ?? []).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            var skipped = (configSource.SkipDotnetVersions ?? []).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
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
