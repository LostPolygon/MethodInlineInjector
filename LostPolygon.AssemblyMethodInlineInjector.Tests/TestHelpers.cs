using System;
using System.IO;

namespace LostPolygon.AssemblyMethodInlineInjector.Tests {
    internal static class TestHelpers {
        /// <summary>
        ///     Call in subclass to deploy items before testing.
        /// </summary>
        /// <param name="items">Items to deploy, relative to project root.</param>
        /// <exception cref="FileNotFoundException">A source item was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">The target deployment directory was not found</exception>
        public static void DeployItems(params string[] items) {
            DeployItems(false, items);
        }

        /// <summary>
        ///     Call in subclass to deploy items before testing.
        /// </summary>
        /// <param name="items">Items to deploy, relative to project root.</param>
        /// <param name="retainDirectories">Retain directory structure of source items?</param>
        /// <exception cref="FileNotFoundException">A source item was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">The target deployment directory was not found</exception>
        public static void DeployItems(bool retainDirectories, params string[] items) {
            //DirectoryInfo environmentDir = new DirectoryInfo(Environment.CurrentDirectory);
            string dataFolderPath = TestEnvironmentConfig.Instance.ProjectDir;
            string binFolderPath = TestEnvironmentConfig.Instance.TargetDir;

            foreach (string item in items) {
                if (string.IsNullOrWhiteSpace(item)) {
                    continue;
                }

                string dirPath = retainDirectories ? Path.GetDirectoryName(item) : "";
                string filePath = item.Replace("/", @"\");
                string itemPath = new Uri(Path.Combine(dataFolderPath, filePath)).LocalPath;
                if (!File.Exists(itemPath)) {
                    throw new FileNotFoundException($"Can't find deployment source item '{itemPath}'");
                }

                if (!Directory.Exists(binFolderPath)) {
                    throw new DirectoryNotFoundException($"Deployment target directory doesn't exist: '{binFolderPath}'");
                }

                string dirPathInBin = Path.Combine(binFolderPath, dirPath);
                if (!Directory.Exists(dirPathInBin)) {
                    Directory.CreateDirectory(dirPathInBin);
                }
                string itemPathInBin = new Uri(Path.Combine(binFolderPath, dirPath, Path.GetFileName(filePath))).LocalPath;
                if (File.Exists(itemPathInBin)) {
                    File.Delete(itemPathInBin);
                }
                File.Copy(itemPath, itemPathInBin);
            }
        }
    }
}