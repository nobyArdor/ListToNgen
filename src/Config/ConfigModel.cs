using System.Diagnostics.CodeAnalysis;

namespace ListToNgen.Config
{
    [Serializable]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    internal sealed record ConfigModel
    {
        public string[]? SkipDotnetVersions { get; init; }
        public string[]? NgenDotnetVersions { get; init; }
    }

}
