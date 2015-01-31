using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PBC.packageExtractor
{
    public class PackageExtractorManager
    {
        List<PackageInfomation> allPackageInfomations;
        public PackageExtractorManager()
        {
            allPackageInfomations = new List<PackageInfomation>();
        }

        public void GetPackageInfo(string path)
        {

            string[] JavaAllFileContainer = GetJavaClassList(path);

            foreach (string fileName in JavaAllFileContainer)
            {
                string FileText = System.IO.File.ReadAllText(fileName);

                string pattern = @"package .*\b";

                Match match = Regex.Match(FileText, pattern);

                if (match.Success)
                {
                    var package = new PackageInfomation
                    {
                        className = fileName,
                        packageName = match.Value
                    };
                    allPackageInfomations.Add(package);
                }
            }
        }

        public string[] GetJavaClassList(string path)
        {
            return Directory.GetFiles(path, "*.java",
                                             SearchOption.AllDirectories);
        }

        public List<ClusterInfo> GetPackageList()
        {

            var numberOfPackages = allPackageInfomations.Select(a => a.packageName).Distinct().ToList();
            var numberOfClassesInPackage = new List<ClusterInfo>();
            foreach (var numberOfPackage in numberOfPackages)
            {
                int count = allPackageInfomations.Count(ap => ap.packageName.Equals(numberOfPackage));
                var item = new ClusterInfo()
                {
                    ClusterName = numberOfPackage,
                    NumberOfClass = count
                };
                numberOfClassesInPackage.Add(item);
            }
            return numberOfClassesInPackage;
        }
    }

    public class PackageInfomation
    {
        public string packageName { get; set; }
        public string className { get; set; }
    }

    public class ClusterInfo
    {
        public string ClusterName { get; set; }
        public int NumberOfClass { get; set; }
    }
}
