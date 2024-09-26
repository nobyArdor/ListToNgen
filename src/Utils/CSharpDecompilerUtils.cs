using System.Collections.Frozen;
using System.Threading.Channels;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

namespace MakeListToNgen.Utils
{
    internal static class CSharpDecompilerUtils
    {
        public static async Task PutAllowToNgen(string filePath, FrozenSet<string> allowed, FrozenSet<string> skipped, ChannelWriter<string> writer)
        {
            try
            {
                var decompiler = new CSharpDecompiler(filePath,
                    new DecompilerSettings() { ThrowOnAssemblyResolveErrors = false });
                var assemblyAttributes = decompiler.TypeSystem.MainModule.GetAssemblyAttributes();
                var target = assemblyAttributes.Where(x => string.Equals(x.AttributeType.Name,
                    nameof(System.Runtime.Versioning.TargetFrameworkAttribute), StringComparison.OrdinalIgnoreCase));
                foreach (var attribute in target)
                {
                    foreach (var valueStr in attribute.FixedArguments
                                 .Select(customAttributeTypedArgument => customAttributeTypedArgument.Value)
                                 .OfType<object>().Select(value => value.ToString()))
                    {
                        if (valueStr is null)
                            continue;

                        if (allowed.Contains(valueStr))
                        {
                            await writer.WriteAsync(filePath);
                            return;
                        }

                        if (!skipped.Contains(valueStr))
                            throw new InvalidOperationException(
                                $"For target framework {valueStr} missing solution please add it to config file");
                    }
                }

            }
            catch (ICSharpCode.Decompiler.Metadata.PEFileNotSupportedException ex)
            {
                //simple skip
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"{filePath} - {ex.Message}");
            }
        }
    }
}
