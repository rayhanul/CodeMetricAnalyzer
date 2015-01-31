using System;
using PBC.packageExtractor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PBC.CodeMetrics
{
    public class PublicMethodManager
    {
        PackageExtractorManager pgm;
        List<NumberOfPublicMethodInfo> numberOfPublicMethod;

        public PublicMethodManager()
        {
            pgm = new PackageExtractorManager();
            numberOfPublicMethod = new List<NumberOfPublicMethodInfo>();
        }

        public void WriteMethodNameAndClassName(string methodName, string className) {

            Console.WriteLine("MethodName:{0}, Class Name :{1}", methodName, className);
        }

        public void GetPublicInfo(string path, List<CodeMetricsClass> codeMetricsList)
        {
            string[] javaAllFileContainer = pgm.GetJavaClassList(path);
            foreach (string fileName in javaAllFileContainer)
            {

                var fileNameWithExtention = GetFileName(fileName);
                var fileNameWithOutJava = ClassNameWithOutJava(fileNameWithExtention);
                var filePath = GetFilePath(fileName, fileNameWithExtention);

                var weightedMethodPerCls = GetWeightedMethod(fileName);

                var npmCount = GetNpmCount(fileName);

                var item = new CodeMetricsClass
                {
                    FullPath = fileName,
                    ClassNameWithOutExtention = fileNameWithOutJava,
                    ClassName = fileNameWithExtention,
                    Path = filePath,
                    WmcCount = weightedMethodPerCls,
                    NpmCount = npmCount
                };
                codeMetricsList.Add(item);
                WriteMethodNameAndClassName("GetPublicInfo", item.ClassName); 
            }
        }

        public string ClassNameWithOutJava(string fileName)
        {
            return fileName.Substring(0, fileName.IndexOf(".", StringComparison.Ordinal));
        }
        public void GetParentCls(List<CodeMetricsClass> codeMetrics)
        {
            string pattern = @"(extends|implements).*";

            foreach (var item in codeMetrics)
            {
                item.Parent = new List<ParentInfo>();
                string fileText = System.IO.File.ReadAllText(item.FullPath);
                Match match = Regex.Match(fileText, pattern, RegexOptions.Multiline);

                if (match.Success)
                {
                    if (match.Value.Contains("extends") & match.Value.Contains("implements"))
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");

                        string[] listOfParent = javaclsName.Split(' ');
                        foreach (var noc in codeMetrics)
                        {
                            var name = noc.ClassName;
                            name = name.Replace(".java", "");
                            if (javaclsName.Trim().Contains(name.Trim()))
                            {
                                foreach (var s in listOfParent)
                                {
                                    if (string.Equals(s.Trim(), name.Trim()))
                                    {
                                        var parent = new ParentInfo
                                        {
                                            ParentClass = noc.ClassName,
                                            ParentClassPath = noc.FullPath
                                        };
                                        item.Parent.Add(parent);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");
                        foreach (var noc in codeMetrics)
                        {
                            var name = noc.ClassName;
                            name = name.Replace(".java", "");
                            if (string.Equals(javaclsName.Trim(), name.Trim()))
                            {
                                var parent = new ParentInfo
                                {
                                    ParentClass = noc.ClassName,
                                    ParentClassPath = noc.FullPath
                                };
                                item.Parent.Add(parent);
                            }
                        }
                    }
                }
            }
        }

        public string GetFileName(string fileName)
        {
            string[] splited = fileName.Split('\\');
            string name = string.Empty;
            foreach (var it in splited)
            {
                if (it.EndsWith(".java"))
                {
                    name = it;
                }
            }
            return name;
        }

        public string GetFilePath(string path, string fileName)
        {
            path = path.Replace(fileName, "");
            return path;
        }
        public int GetNpmCount(string filePath)
        {
            string fileText = System.IO.File.ReadAllText(filePath);
            string pattern = @"public.*\(.*\s*\)";
            // string pattern=@"(public|protected|private|static|\s)+[\w\<\>\[\]]+\s+(\w+) *\([^\)]*\)*(\{?|[^;])";
            MatchCollection matches = Regex.Matches(fileText, pattern, RegexOptions.Multiline);
            return matches.Count;
        }
        public List<NumberOfPublicMethodInfo> GetPublicMethods(string path)
        {
            string[] javaAllFileContainer = pgm.GetJavaClassList(path);

            foreach (string fileName in javaAllFileContainer)
            {
                var item = getPublicmethods(fileName);
                numberOfPublicMethod.Add(item);
            }

            return numberOfPublicMethod;
        }

        public int GetWeightedMethod(string filepath)
        {
            string fileText = System.IO.File.ReadAllText(filepath);
            string pattern = @"(public|private).*\(";
            MatchCollection matches = Regex.Matches(fileText, pattern, RegexOptions.Multiline);
            return matches.Count;
        }

        public void GetLcomInfo(List<CodeMetricsClass> codeMetrics)
        {
            List<NumberOfPublicMethodInfo> pubLicMethodInfo = new List<NumberOfPublicMethodInfo>();

            var methodsInfoList = new List<MethodsInfo>();
            var instancevariableList = new List<string>();
            int i = 1;
            foreach (var item in codeMetrics)
            {
                var methodsInfo = new List<MethodsInfo>();
                methodsInfoList = GetFileWithAllMethodsInnerText(item.FullPath, methodsInfo, item.ClassNameWithOutExtention);

                instancevariableList = GetInstanceVariableList(item.FullPath, methodsInfoList);

                foreach (var method in methodsInfoList)
                {
                    method.UsedInstanceVariableList = new List<string>();
                    foreach (var instanceVariable in instancevariableList)
                    {
                        if (method.MethodInnerBody.Contains(instanceVariable))
                        {
                            try
                            {
                                method.UsedInstanceVariableList.Add(instanceVariable);
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.Message);
                            }

                        }
                    }
                }
                int numberOfCohension = 0;
                foreach (var methodInfo in methodsInfoList)
                {

                    foreach (var innerMethodInfo in methodsInfoList)
                    {
                        if (!string.Equals(methodInfo.MethodName, innerMethodInfo.MethodName))
                        {
                            List<string> thirdList = methodInfo.UsedInstanceVariableList.Except(innerMethodInfo.UsedInstanceVariableList).ToList();
                            if (thirdList.Count != methodInfo.UsedInstanceVariableList.Count)
                            {
                                numberOfCohension++;
                            }
                        }
                    }
                }
                item.LcomCount = numberOfCohension;
                WriteMethodNameAndClassName("GetLcomInfo", item.ClassName); 
            }
        }

        public List<string> GetInstanceVariableList(string fullPath, List<MethodsInfo> methodsList)
        {
            string fileText = removeMethodsFromFile(fullPath, methodsList);
            fileText = RemoveComment(fileText);
            fileText = RemoveClassAndPackageName(fileText);

            //  string pattern = @"^\s*(var|const)\s+([a-zA-Z][a-zA-Z\d_]*(?:\s*,\s*[a-zA-Z][a-zA-Z\d_]*)*)\s*:\s*(int|real|boolean|char|string)\s*$";
            //   string str = @"[a-zA-Z][a-zA-Z0-9_]*";

            string pattern = @"(public|private)?\s+(static|final)?(int|boolean|char|string){1}.*(\,|\;)";
            MatchCollection match = Regex.Matches(fileText, pattern, RegexOptions.Multiline);


            // \s*(public|private)*\s*(int|byte|boolean|char|string|var|const)\s+([a-zA-Z][a-zA-Z\d_]*)\s*;
            string equalSignPattern = @"\=.*";
            Regex r = new Regex(equalSignPattern);
            var matchList = match.Cast<Match>().Select(mat => mat.Value).ToList();
            string[] item = { "private", "public", "static", "final", "string", "char", "int", "constant", ";", ",", "[]", "protected", "boolean" };
            List<string> matchListOnlyVariable = new List<string>();
            foreach (string mat in matchList)
            {
                string temp = mat;
                foreach (var it in item)
                {
                    if (temp.ToLower().Contains(it.ToLower()))
                    {
                        temp = temp.Replace(it.ToLower(), "");
                    }

                }
                string temp2 = r.Replace(temp, "");
                matchListOnlyVariable.Add(temp2.Trim());
            }

            var list = matchListOnlyVariable.Distinct().ToList();

            return list;
        }

        public string removeMethodsFromFile(string filePath, List<MethodsInfo> methodsList)
        {
            string fileText = System.IO.File.ReadAllText(filePath);

            foreach (var item in methodsList)
            {
                fileText = fileText.Replace(item.MethodInnerBody, "");
            }
            return fileText;
        }
        private string RemoveClassAndPackageName(string fileText)
        {
            string packagePattern = @"package .*;";
            string classPattern = @"class [a-zA-Z_$\s]*\{?";

            Regex rgx = new Regex(packagePattern);
            fileText = rgx.Replace(fileText, "");

            rgx = new Regex(classPattern);
            fileText = rgx.Replace(fileText, "");
            return fileText;
        }
        public string RemoveComment(string str)
        {

            string pattern = @"\/\/.*|\*([^*]|[\r\n]|(\*([^/]|[\r\n])))*\*";
            // Match match=Regex.Match(str, pattern,RegexOptions.Multiline);
            Regex rgx = new Regex(pattern);
            //   str = rgx.Replace(str, "");
            return str;
        }
        public List<MethodsInfo> GetFileWithAllMethodsInnerText(string fullPath, List<MethodsInfo> methodsInfo, string fileName)
        {
            int incrementor = 1;
            string fileText = System.IO.File.ReadAllText(fullPath);
            fileText = RemoveComment(fileText);
            fileText = RemoveClassAndPackageName(fileText);
            var reg = @"
                        (?<body>
                        \{(?<DEPTH>)
                        (?>
                        (?<DEPTH>)\{
                            |
                        \}(?<-DEPTH>)  
                            |
                        (?(DEPTH)[^\{\}]* | )
                        )*
                        \}(?<-DEPTH>)
                        (?(DEPTH)(?!))
                        )";
            foreach (Match m in Regex.Matches(fileText, reg, RegexOptions.IgnorePatternWhitespace))
            {
                incrementor++;
                var methods = new MethodsInfo
                  {
                      ClassName = fullPath,
                      MethodInnerBody = m.Groups["body"].Value,
                      MethodName = string.Format("{0}{1}", fileName, incrementor),
                      ClassNameWithoutExcention = fileName
                  };

                methodsInfo.Add(methods);
            }


            return methodsInfo;
        }

        public void GetCoupling(List<CodeMetricsClass> codeMetrics)
        {
            List<NumberOfPublicMethodInfo> listOfPublicMethodInfo = new List<NumberOfPublicMethodInfo>();
            foreach (var item in codeMetrics)
            {
                getPublicMethodsWithMethodsNameList(listOfPublicMethodInfo, item.FullPath, item.ClassNameWithOutExtention);
            }

            foreach (var item in codeMetrics)
            {
                List<string> coupledObjects = new List<string>();
                string fileText = System.IO.File.ReadAllText(item.FullPath);
                foreach (var publicMethodsList in listOfPublicMethodInfo)
                {
                    if (fileText.Contains(publicMethodsList.MethodNameWithOutParam) & fileText.Contains(publicMethodsList.ClassName))
                    {
                        if (!coupledObjects.Contains(publicMethodsList.ClassName))
                        {
                            coupledObjects.Add(publicMethodsList.ClassName);

                        }
                    }
                }
                item.CobCount = coupledObjects.Count;
                WriteMethodNameAndClassName("GetCoupling", item.ClassName); 
            }

            //foreach (var item in codeMetrics)
            //{
            //    List<string> coupledObjects = new List<string>();
            //    string fileText = System.IO.File.ReadAllText(item.FullPath);
            //    foreach (var innerItem in codeMetrics)
            //    {
            //        if (fileText.Contains(innerItem.ClassNameWithOutExtention))
            //        {
            //            if (!coupledObjects.Contains(innerItem.ClassNameWithOutExtention))
            //            {
            //                coupledObjects.Add(innerItem.ClassNameWithOutExtention);

            //            }
            //        }
            //    }
            //    item.CobCount = coupledObjects.Count;
            //}
        }

        public void GetNocCount(List<CodeMetricsClass> codeMetrics)
        {
            string pattern = @"(extends|implements).*";

            foreach (var item in codeMetrics)
            {
                string fileText = System.IO.File.ReadAllText(item.FullPath);
                Match match = Regex.Match(fileText, pattern, RegexOptions.Multiline);

                if (match.Success)
                {
                    if (match.Value.Contains("extends") & match.Value.Contains("implements"))
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");

                        string[] listOfParent = javaclsName.Split(' ');
                        foreach (var noc in codeMetrics)
                        {
                            if (javaclsName.Trim().Contains(noc.ClassNameWithOutExtention))
                            {
                                foreach (var s in listOfParent)
                                {
                                    if (string.Equals(s.Trim(), noc.ClassNameWithOutExtention.Trim()))
                                    {
                                        noc.NocCount++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");
                        foreach (var noc in codeMetrics)
                        {
                            if (string.Equals(javaclsName.Trim(), noc.ClassNameWithOutExtention.Trim()))
                            {
                                noc.NocCount++;
                            }
                        }
                    }
                }
                WriteMethodNameAndClassName("GetNocCount", item.ClassName); 
            }
        }

        public void GetDitCount(List<CodeMetricsClass> codeMetrics)
        {
            string pattern = @"(extends|implements).*";

            foreach (var item in codeMetrics)
            {
                string fileText = System.IO.File.ReadAllText(item.FullPath);
                Match match = Regex.Match(fileText, pattern, RegexOptions.Multiline);

                if (match.Success)
                {
                    if (match.Value.Contains("extends") & match.Value.Contains("implements"))
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");

                        string[] listOfParent = javaclsName.Split(' ');
                        foreach (var dit in codeMetrics)
                        {
                            if (javaclsName.Trim().Contains(dit.ClassNameWithOutExtention))
                            {
                                foreach (var s in listOfParent)
                                {
                                    if (string.Equals(s.Trim(), dit.ClassNameWithOutExtention.Trim()))
                                    {
                                        item.DitCount++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string javaclsName = match.Value;
                        javaclsName = javaclsName.Replace("extends", "");
                        javaclsName = javaclsName.Replace("implements", "");
                        foreach (var dit in codeMetrics)
                        {
                            if (string.Equals(javaclsName.Trim(), dit.ClassNameWithOutExtention.Trim()))
                            {
                                item.DitCount++;
                            }
                        }
                    }
                }
                WriteMethodNameAndClassName("GetDitCount", item.ClassName); 
            }
        }

        public void GetRfcCount(List<CodeMetricsClass> codeMetricsList)
        {
            foreach (var item in codeMetricsList)
            {
                var npm = GetNpmCount(item.FullPath);
                if (item.Parent.Any())
                {
                    npm += item.Parent.Sum(parent => GetNpmCount(parent.ParentClassPath));
                }
                item.RfcCount = npm;         
                WriteMethodNameAndClassName("GetRfcCount", item.ClassName); 
            }
        }

        private static void GetObjectList(List<CouplingBetweenObject> coupling, string[] javaAllFileContainer)
        {
            foreach (string fileName in javaAllFileContainer)
            {
                var item = new CouplingBetweenObject();
                string[] splited = fileName.Split('\\');
                foreach (var it in splited)
                {
                    if (it.EndsWith(".java"))
                    {
                        string[] fileNamewithoutextention = it.Split('.');
                        item.ClassName = fileNamewithoutextention[0];
                        item.Count = 0;

                    }
                }
                coupling.Add(item);
            }
        }

        private static void GetNocObjectList(List<Noc> coupling, string[] javaAllFileContainer)
        {
            foreach (string fileName in javaAllFileContainer)
            {
                var item = new Noc();
                string[] splited = fileName.Split('\\');
                foreach (var it in splited)
                {
                    if (it.EndsWith(".java"))
                    {
                        string[] fileNamewithoutextention = it.Split('.');
                        item.ClassName = fileNamewithoutextention[0];
                        item.Count = 0;

                    }
                }
                coupling.Add(item);
            }
        }

        private NumberOfPublicMethodInfo getPublicmethods(string fileName)
        {

            string fileText = System.IO.File.ReadAllText(fileName);

            string pattern = @"public.*\(.*\s*\)";
            MatchCollection matches = Regex.Matches(fileText, pattern, RegexOptions.Multiline);
            var item = new NumberOfPublicMethodInfo();
            if (matches.Count > 0)
            {
                item.ClassName = fileName;
                item.Count = matches.Count;
            }
            else
            {
                item.ClassName = fileName;
                item.Count = 0;
            }
            return item;
        }

        private void getPublicMethodsWithMethodsNameList(List<NumberOfPublicMethodInfo> item, string fullPath, string fileName)
        {
            string fileText = System.IO.File.ReadAllText(fullPath);

            string pattern = @"public.*\(.*\s*\)";
            // (private|public).*\(.*\s*\)
            MatchCollection matches = Regex.Matches(fileText, pattern, RegexOptions.Multiline);
            string methodPattern = @"[_a-zA-Z0-9]*\(.*\)";

            if (matches.Count > 0)
            {
                foreach (Match ItemMatch in matches)
                {
                    Match match = Regex.Match(ItemMatch.Value, methodPattern);

                    var methodNameWithOutParam = match.Value.Substring(0, match.Value.IndexOf("(", StringComparison.Ordinal));

                    NumberOfPublicMethodInfo info = new NumberOfPublicMethodInfo
                    {
                        MethodsName = match.Value,
                        ClassName = fileName,
                        Count = matches.Count,
                        MethodNameWithOutParam = methodNameWithOutParam
                    };
                    item.Add(info);
                }
            }
        }
    }

    public class NumberOfPublicMethodInfo
    {
        public string ClassName { get; set; }
        public int Count { get; set; }
        public string MethodsName { get; set; }
        public string MethodNameWithOutParam { get; set; }
    }

    public class CouplingBetweenObject
    {
        public string ClassName { get; set; }
        public int Count { get; set; }

    }

    public class Noc
    {
        public string ClassName { get; set; }
        public int Count { get; set; }
    }

    public class Dit
    {
        public string ClassName { get; set; }
        public int Count { get; set; }
        public string ParentClass { get; set; }
    }

    public class Rfc
    {
        public string ClassName { get; set; }
        public int Count { get; set; }

    }

    public class ParentInfo
    {
        public string ParentClass { get; set; }
        public string ParentClassPath { get; set; }
    }

    public class CodeMetricsClass
    {
        public string ClassName { get; set; }
        public string ClassNameWithOutExtention { get; set; }
        public string FullPath { get; set; }
        public string Path { get; set; }
        public List<ParentInfo> Parent { get; set; }
        public int NocCount { get; set; }
        public int DitCount { get; set; }
        public int CobCount { get; set; }
        public int LocCount { get; set; }
        public int RfcCount { get; set; }
        public int NpmCount { get; set; }
        public int WmcCount { get; set; }
        public int LcomCount { get; set; }

    }

    public class MethodsInfo
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string MethodInnerBody { get; set; }
        public string ClassNameWithoutExcention { get; set; }
        public List<string> UsedInstanceVariableList { get; set; }

    }
}