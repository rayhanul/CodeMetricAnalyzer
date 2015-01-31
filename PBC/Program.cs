using System.Reflection.Emit;
using PBC.CodeMetrics;
using PBC.packageExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PBC
{
    public class RecursiveFileProcessor
    {
        public static void Main(string[] args)
        {


            //for home
          //  string path = @"E:\EducatioN\MSSE\thesis\project\poi-src-3.10-FINAL-20140208";
          //  string excelFilePath = @"C:\Users\x-man\Desktop\poi.xlsx";


            //for lab
            string path = @"D:\Code\TestCasePrioritization";

            string excelFilePath = @"C:\Users\xman\Documents\mynewfile2.xlsx";
            PackageExtractorManager packageManager = new PackageExtractorManager();

            packageManager.GetPackageInfo(path);
            var item = packageManager.GetPackageList();

            List<CodeMetricsClass> codeMetrics = new List<CodeMetricsClass>();

            PublicMethodManager pmm = new PublicMethodManager();
            pmm.GetPublicInfo(path, codeMetrics);
            pmm.GetParentCls(codeMetrics);
            pmm.GetLcomInfo(codeMetrics);
            pmm.GetCoupling(codeMetrics);
            pmm.GetNocCount(codeMetrics);
            pmm.GetDitCount(codeMetrics);
            pmm.GetRfcCount(codeMetrics);

            LinesOfCode loc = new LinesOfCode();
            loc.GetLinesOfCode(codeMetrics);

            FileManager fileManager = new FileManager();
            fileManager.GenerateExcelFile(codeMetrics, excelFilePath);
            Console.WriteLine("End of Operation...");
        }

    }

}
