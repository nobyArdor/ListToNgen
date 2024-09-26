namespace MakeListToNgen.Utils
{
    internal static class FileSystemInfoUtils
    {
        public static IEnumerable<FileSystemInfo> GetAcceptedFiles(this DirectoryInfo directoryInfo, params string[] acceptedExtensions)
        {
            var dirFiles = directoryInfo.EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories)
                    .AsParallel()
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                    .Where(fileSystemInfo => acceptedExtensions.AtLeastOne(x => x.EqualAsId(fileSystemInfo.Extension)))
                ;
            return dirFiles;
        }
    }
}
