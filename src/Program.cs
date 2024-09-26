using System.CommandLine;

namespace MakeListToNgen
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var rootCommand = RootCommandBuilder.Build();
            await rootCommand.InvokeAsync(args);
        }
    }
}
