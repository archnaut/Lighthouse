using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LighthouseDesktop.Core.Infrastructure.FileSystem
{
    public interface IWildcardPathsParser
    {
        IList<string> ConvertPathsWithWildcardsToIndividualPaths(IList<string> pathsWithWildcards);
    }

    public class WildcardPathsParser : IWildcardPathsParser
    {
        public IList<string> ConvertPathsWithWildcardsToIndividualPaths(IList<string> pathsWithWildcards)
        {
            var results = new List<string>();

            foreach (var referencedDll in pathsWithWildcards)
            {
                if (!FileNameContainsWildcards(referencedDll))
                {
                    results.Add(referencedDll);
                    continue;
                }

                var directoryToSearchIn = Path.GetDirectoryName(referencedDll);
                if (directoryToSearchIn == null || !Directory.Exists(directoryToSearchIn))
                {
                    throw new FileNotFoundException(string.Format("Could not find directory {0} for wildcarded file path {1}", directoryToSearchIn, referencedDll), referencedDll);
                }

                var dirInstance = new DirectoryInfo(directoryToSearchIn);
                var filePartWithWildcard = Path.GetFileName(referencedDll);
                if (filePartWithWildcard == null)
                {
                    throw new FileNotFoundException(string.Format("Could not find directory {0} for wildcarded file path {1}", directoryToSearchIn, referencedDll), referencedDll);
                }

                var foundFiles = dirInstance.GetFiles(filePartWithWildcard);
                results.AddRange(foundFiles.Select(foundFile => foundFile.FullName));
            }

            return results;
        }


        private static bool FileNameContainsWildcards(string fileName)
        {
            return fileName.Contains("?") || fileName.Contains("*");
        }
    }
}