namespace ListToNgen
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            await Console.Out.WriteLineAsync("ListToNgen Started");
            try
            {
                var rootCommand = RootCommandBuilder.Build();
                var parseResult = rootCommand.Parse(args);
                if (parseResult.Errors.Count == 0)
                {
                    var result = await parseResult.InvokeAsync();
                    return result;
                }

                foreach (var error in parseResult.Errors)
                {
                    await Console.Error.WriteLineAsync(error.Message);
                }

                return parseResult.Errors.Count;

            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 1;
            }
        }
    }
}
