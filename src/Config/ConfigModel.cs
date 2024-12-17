namespace ListToNgen.Config
{
    [Serializable]
    internal sealed class ConfigModel
    {
        public string[] SkipDotnetVersions { get; set; } = [];
        public string[] NgenDotnetVersions { get; set; } = [];
    }
}
