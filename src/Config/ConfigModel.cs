namespace MakeListToNgen.Config
{
    [Serializable]
    internal class ConfigModel
    {
        public ConfigModel()
        {
            SkipDotnetVersions = [];
            NgenDotnetVersions = [];
        }
        public string[] SkipDotnetVersions { get; set; }
        public string[] NgenDotnetVersions { get; set; }
    }
}
