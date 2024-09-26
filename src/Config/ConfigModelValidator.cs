using System.Collections.Frozen;

namespace MakeListToNgen.Config
{
    internal static class ConfigModelValidator
    {
        public static void ValidateConfig(ConfigModel model)
        {

            var ngenDotnetVersions = model.NgenDotnetVersions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            var skipDotnetVersions = model.SkipDotnetVersions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            if (!ngenDotnetVersions.Overlaps(skipDotnetVersions)) return;
            var commonElements = string.Join(", ", ngenDotnetVersions.Intersect(skipDotnetVersions));
            throw new InvalidOperationException(
                $"Config error. {nameof(model.NgenDotnetVersions)} and {nameof(model.SkipDotnetVersions)} has common elements {commonElements}");
        }
    }
}
