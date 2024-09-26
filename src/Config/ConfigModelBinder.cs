using System.CommandLine;
using System.CommandLine.Binding;
using System.Text;
using Tomlyn;

namespace MakeListToNgen.Config
{
    internal class ConfigModelBinder(Option<FileInfo> configPath) : BinderBase<ConfigModel>
    {
        protected override ConfigModel GetBoundValue(BindingContext bindingContext)
        {
            var path = bindingContext.ParseResult.GetValueForOption(configPath);

            if (path?.Exists != true)
            {
                throw new InvalidOperationException($@"Config file is not exists {path}");
            }

            var text = File.ReadAllText(path.FullName, Encoding.UTF8);
            if (Toml.TryToModel<ConfigModel>(text, out var model, out var bag))
                return model;

            if (bag?.HasErrors != true)
                throw new InvalidOperationException($@"Can not parse config {path}");

            var errorText = string.Join(".", bag.Select(x => x.Message));
            throw new InvalidOperationException(errorText);
        }
    }
}
