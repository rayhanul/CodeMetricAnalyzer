using PBC.packageExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PBC.CodeMetrics
{
    public class LinesOfCode
    {
        PackageExtractorManager pgm;
        List<NumberOfPublicMethodInfo> numberOfPublicMethod;
        public LinesOfCode()
        {
            pgm = new PackageExtractorManager();
            numberOfPublicMethod = new List<NumberOfPublicMethodInfo>();
        }

        public void WriteMethodNameAndClassName(string methodName, string className)
        {

            Console.WriteLine("MethodName:{0}, Class Name :{1}", methodName, className);
        }
        public void GetLinesOfCode(List<CodeMetricsClass> codeMetrics )
        {
           // string[] JavaAllFileContainer = pgm.GetJavaClassList(path);

            foreach (var item in codeMetrics)
            {
                string fileText = System.IO.File.ReadAllText(item.FullPath);
                string pattern = @".*\;";
                MatchCollection matches = Regex.Matches(fileText, pattern, RegexOptions.Multiline);
              //  var item = new NumberOfPublicMethodInfo();
               /* if (matches.Count > 0)
                {
                    item.ClassName = fileName;
                    item.Count = matches.Count;
                }
                else
                {
                    item.ClassName = fileName;
                    item.Count = 0;
                }*/
                item.LocCount = matches.Count;

                WriteMethodNameAndClassName("GetLinesOfCode", item.ClassName);
            }
        }
    }
}
