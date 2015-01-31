using OfficeOpenXml;
using System;
using System.IO;
namespace PBC.CodeMetrics
{
    public class FileManager
    {

        public void GenerateExcelFile(System.Collections.Generic.List<CodeMetricsClass> codeMetrics, string filePath)
        {

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            FileInfo newFile = new FileInfo(filePath);
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.Add("JEdit-3.2");
                // write some titles into column 1
                worksheet.Cell(1, 1).Value = "FilName";
                worksheet.Cell(1, 2).Value = "Path";
                worksheet.Cell(1, 3).Value = "DIT";
                worksheet.Cell(1, 4).Value = "NOC";
                worksheet.Cell(1, 5).Value = "CBO";
                worksheet.Cell(1, 6).Value = "LOC";
                worksheet.Cell(1, 7).Value = "WMC";
                worksheet.Cell(1, 8).Value = "LCOM";
                worksheet.Cell(1, 9).Value = "RFC";
                worksheet.Cell(1, 10).Value = "NPM";

                for (int j = 0; j < codeMetrics.Count; j++)
                {
                    worksheet.Cell(j + 2, 1).Value = codeMetrics[j].ClassName;
                    worksheet.Cell(j + 2, 2).Value = codeMetrics[j].FullPath;
                    worksheet.Cell(j + 2, 3).Value = string.Format("{0}", codeMetrics[j].DitCount);
                    worksheet.Cell(j + 2, 4).Value = string.Format("{0}", codeMetrics[j].NocCount);
                    worksheet.Cell(j + 2, 5).Value = string.Format("{0}", codeMetrics[j].CobCount);
                    worksheet.Cell(j + 2, 6).Value = string.Format("{0}", codeMetrics[j].LocCount);
                    worksheet.Cell(j + 2, 7).Value = string.Format("{0}", codeMetrics[j].WmcCount);
                    worksheet.Cell(j + 2, 8).Value = string.Format("{0}", codeMetrics[j].LcomCount);
                    worksheet.Cell(j + 2, 9).Value = string.Format("{0}", codeMetrics[j].RfcCount);
                    worksheet.Cell(j + 2, 10).Value = string.Format("{0}", codeMetrics[j].NpmCount);
                }
                xlPackage.Save();
            }
        }

        public void WritingToFile() {
            Console.WriteLine("Generating excel file...");
        }
    }
}
