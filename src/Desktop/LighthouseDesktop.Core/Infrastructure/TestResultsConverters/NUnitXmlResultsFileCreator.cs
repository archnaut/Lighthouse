using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;
using LighthouseDesktop.Core.ExtensionMethods;
using LighthouseDesktop.Core.Infrastructure.TestExecution;
using LighthouseDesktop.Core.Infrastructure.XapManagement;

namespace LighthouseDesktop.Core.Infrastructure.TestResultsConverters
{
    public interface INUnitXmlResultsFileCreator
    {
        string CreateFile(RemoteTestExecutionResults outcome);
    }

    public class NUnitXmlResultsFileCreator : INUnitXmlResultsFileCreator
    {
        public string OutputFilePath { get; set; }
        private XmlTextWriter xmlWriter;
        private MemoryStream memoryStream;

        public NUnitXmlResultsFileCreator()
        {


        }

        public string CreateFile(RemoteTestExecutionResults results)
        {
            this.memoryStream = new MemoryStream();
            this.xmlWriter = new XmlTextWriter(new StreamWriter(memoryStream, System.Text.Encoding.UTF8));
            InitializeXmlFile(results);

            var outcome = results.UnitTestOutcome;

            var groupedByAssembly = outcome.TestResults.GroupBy(r => r.TestClass.Assembly.Name);

            foreach (var assembly in groupedByAssembly)
            {
                WriteAssemblyTestSuiteElement(new ComposedUnitTestOutcome() { TestResults = assembly.ToList() });
            }

            return TerminateXmlFile();
        }

        private void StartTestSuiteElement(string testSuiteName, string testSuiteTypeText, bool executed, bool succeeded, double time, int assertsCount)
        {
            xmlWriter.WriteStartElement("test-suite");

            if (!string.IsNullOrEmpty(testSuiteTypeText))
            {
                xmlWriter.WriteAttributeString("type", testSuiteTypeText);
            }

            xmlWriter.WriteAttributeString("name", testSuiteName);

            xmlWriter.WriteAttributeString("executed", executed.ToString());
            xmlWriter.WriteAttributeString("result", succeeded ? "Success" : "Failure");

            if (executed)
            {
                xmlWriter.WriteAttributeString("success", succeeded ? "True" : "False");
                xmlWriter.WriteAttributeString("time", time.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
                xmlWriter.WriteAttributeString("asserts", assertsCount.ToString());
            }
        }

        private void WriteAssemblyTestSuiteElement(IComposedUnitTestOutcome results)
        {
            var assemblyName = results.TestResults.FirstOrDefault().TestClass.Assembly.Name;

            StartTestSuiteElement(assemblyName, "Assembly", results.TotalNumberOfTestsExecuted() > 0, results.Succeeded(),
                                  results.ExecutionTimeInMiliseconds(), 0);

            xmlWriter.WriteStartElement("results");

            var groupedByNamespace = results.TestResults.GroupBy(r => r.TestClass.Namespace);

            foreach (var namespaceData in groupedByNamespace)
            {
                WriteNamespaceTestSuiteElement(new ComposedUnitTestOutcome() { TestResults = namespaceData.ToList() });
            }

            xmlWriter.WriteEndElement(); // results

            xmlWriter.WriteEndElement(); // test suite element
        }

        private void WriteNamespaceTestSuiteElement(IComposedUnitTestOutcome results)
        {
            var namespaceName = results.TestResults.FirstOrDefault().TestClass.Namespace;

            StartTestSuiteElement(namespaceName, "Namespace", results.TotalNumberOfTestsExecuted() > 0, results.Succeeded(),
                                  results.ExecutionTimeInMiliseconds(), 0);

            xmlWriter.WriteStartElement("results");

            WriteTestFixtureTestSuiteElement(results);

            xmlWriter.WriteEndElement(); // results

            xmlWriter.WriteEndElement(); // test suite element
        }

        private void WriteTestFixtureTestSuiteElement(IComposedUnitTestOutcome results)
        {
            var testSuiteRealName = results.TestResults.FirstOrDefault().TestClass.Name;

            StartTestSuiteElement(testSuiteRealName, "TestFixture", results.TotalNumberOfTestsExecuted() > 0, results.Succeeded(),
                                  results.ExecutionTimeInMiliseconds(), 0);

            xmlWriter.WriteStartElement("results");

            if (results.TestResults.Any())
                WriteChildResults(results.TestResults);

            xmlWriter.WriteEndElement(); // results

            xmlWriter.WriteEndElement(); // test suite element
        }

        private void WriteChildResults(IList<IUnitTestScenarioResult> results)
        {
            foreach (var childResult in results)
                    WriteResultElement(childResult);
        }

        private void WriteResultElement(IUnitTestScenarioResult result)
        {
            xmlWriter.WriteStartElement("test-case");
            xmlWriter.WriteAttributeString("name", result.TestMethod.Name);

            xmlWriter.WriteAttributeString("executed", (result.Result != UnitTestOutcome.NotExecuted).ToString());
            xmlWriter.WriteAttributeString("result", result.Result == UnitTestOutcome.Passed ? "Success" : "Error");

/*
            if (result.Result == UnitTestOutcome.Passed)
            {
*/
                xmlWriter.WriteAttributeString("success", (result.Result == UnitTestOutcome.Passed).ToString());
                xmlWriter.WriteAttributeString("time", (result.Finished-result.Started).TotalSeconds.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
                xmlWriter.WriteAttributeString("asserts", "0");
/*
            }
*/

            switch (result.Result)
            {
                case UnitTestOutcome.Failed:
                case UnitTestOutcome.Error:
                case UnitTestOutcome.Timeout:
                    WriteFailureElement(result);
                    break;
            }

            xmlWriter.WriteEndElement(); // test element
        }

        private void WriteFailureElement(IUnitTestScenarioResult result)
        {
            xmlWriter.WriteStartElement("failure");

            xmlWriter.WriteStartElement("message");

            string errorMsg = "Unknown";
            string stackTrace = "Unknown";
            if (result.Exception != null)
            {
                if (result.Exception.Message != null)
                {
                    errorMsg = result.Exception.Message;
                }

                if (result.Exception.StackTrace != null)
                {
                    stackTrace = result.Exception.StackTrace;
                }
            }

            WriteCData(errorMsg);

            xmlWriter.WriteEndElement();
           
            xmlWriter.WriteStartElement("stack-trace");


            WriteCData(StackTraceFilter.Filter(stackTrace));
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
        }

        private string TerminateXmlFile()
        {
            var result = string.Empty;

            try
            {
                xmlWriter.WriteEndElement(); // test-results
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();

                if (memoryStream != null)
                {
                    memoryStream.Position = 0;
                    using (var rdr = new StreamReader(memoryStream))
                    {
                        result = rdr.ReadToEnd();
                    }
                }

                xmlWriter.Close();
            }
            finally
            {
                //writer.Close();
            }

            return result;
        }

        private void WriteEnvironment()
        {
            xmlWriter.WriteStartElement("environment");
            xmlWriter.WriteAttributeString("nunit-version", "2.5.9");
            xmlWriter.WriteAttributeString("clr-version",
                                           Environment.Version.ToString());
            xmlWriter.WriteAttributeString("os-version",
                                           Environment.OSVersion.ToString());
            xmlWriter.WriteAttributeString("platform",
                Environment.OSVersion.Platform.ToString());
            xmlWriter.WriteAttributeString("cwd",
                                           Environment.CurrentDirectory);
            xmlWriter.WriteAttributeString("machine-name",
                                           Environment.MachineName);
            xmlWriter.WriteAttributeString("user",
                                           Environment.UserName);
            xmlWriter.WriteAttributeString("user-domain",
                                           Environment.UserDomainName);
            xmlWriter.WriteEndElement();
        }

        private void WriteCultureInfo()
        {
            xmlWriter.WriteStartElement("culture-info");
            xmlWriter.WriteAttributeString("current-culture",
                                           CultureInfo.CurrentCulture.ToString());
            xmlWriter.WriteAttributeString("current-uiculture",
                                           CultureInfo.CurrentUICulture.ToString());
            xmlWriter.WriteEndElement();
        }

        private void InitializeXmlFile(RemoteTestExecutionResults executionResults)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteStartDocument(false);
            xmlWriter.WriteComment("This file represents the results of running a test suite");

            xmlWriter.WriteStartElement("test-results");
            var outcome = executionResults.UnitTestOutcome;

            var errors = outcome.TestResults.Where(p => p.Result == UnitTestOutcome.Error);

            string name;
            if (executionResults.XapBuildResult is XapSourcedXapBuildResult)
            {
                name = (executionResults.XapBuildResult as XapSourcedXapBuildResult).SourceXapFullPath;
            }
            else
            {
                name = executionResults.XapBuildResult.ResultingXapFullPath;
            }

            xmlWriter.WriteAttributeString("name", name);


            xmlWriter.WriteAttributeString("total", outcome.TotalNumberOfTestsExecuted().ToString());
            xmlWriter.WriteAttributeString("errors", outcome.NumberOfErrors().ToString());
            xmlWriter.WriteAttributeString("failures", outcome.NumberOfFailures().ToString());
            xmlWriter.WriteAttributeString("not-run", outcome.NumberOfNotExecuted().ToString());
            xmlWriter.WriteAttributeString("inconclusive", outcome.NumberOfInconclusive().ToString());
            xmlWriter.WriteAttributeString("ignored", "0");
            xmlWriter.WriteAttributeString("skipped", "0");
            xmlWriter.WriteAttributeString("invalid", "0");

            DateTime now = DateTime.Now;
            xmlWriter.WriteAttributeString("date", XmlConvert.ToString(now, "yyyy-MM-dd"));
            xmlWriter.WriteAttributeString("time", XmlConvert.ToString(now, "HH:mm:ss"));
            WriteEnvironment();
            WriteCultureInfo();
        }


        #region Output Helpers
        /// <summary>
        /// Makes string safe for xml parsing, replacing control chars with '?'
        /// </summary>
        /// <param name="encodedString">string to make safe</param>
        /// <returns>xml safe string</returns>
        private static string CharacterSafeString(string encodedString)
        {
            /*The default code page for the system will be used.
            Since all code pages use the same lower 128 bytes, this should be sufficient
            for finding uprintable control characters that make the xslt processor error.
            We use characters encoded by the default code page to avoid mistaking bytes as
            individual characters on non-latin code pages.*/
            char[] encodedChars = System.Text.Encoding.Default.GetChars(System.Text.Encoding.Default.GetBytes(encodedString));

            System.Collections.ArrayList pos = new System.Collections.ArrayList();
            for (int x = 0; x < encodedChars.Length; x++)
            {
                char currentChar = encodedChars[x];
                //unprintable characters are below 0x20 in Unicode tables
                //some control characters are acceptable. (carriage return 0x0D, line feed 0x0A, horizontal tab 0x09)
                if (currentChar < 32 && (currentChar != 9 && currentChar != 10 && currentChar != 13))
                {
                    //save the array index for later replacement.
                    pos.Add(x);
                }
            }
            foreach (int index in pos)
            {
                encodedChars[index] = '?';//replace unprintable control characters with ?(3F)
            }
            return System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(encodedChars));
        }

        private void WriteCData(string text)
        {
            int start = 0;
            while (true)
            {
                int illegal = text.IndexOf("]]>", start);
                if (illegal < 0)
                    break;
                xmlWriter.WriteCData(text.Substring(start, illegal - start + 2));
                start = illegal + 2;
                if (start >= text.Length)
                    return;
            }

            if (start > 0)
                xmlWriter.WriteCData(text.Substring(start));
            else
                xmlWriter.WriteCData(text);
        }

        #endregion

    }

    public class StackTraceFilter
    {
        public static string Filter(string stack)
        {
            if (stack == null) return null;
            StringWriter sw = new StringWriter();
            StringReader sr = new StringReader(stack);

            try
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!FilterLine(line))
                        sw.WriteLine(line.Trim());
                }
            }
            catch (Exception)
            {
                return stack;
            }
            return sw.ToString();
        }

        static bool FilterLine(string line)
        {
            string[] patterns = new string[]
			{
				"NUnit.Core.TestCase",
				"NUnit.Core.ExpectedExceptionTestCase",
				"NUnit.Core.TemplateTestCase",
				"NUnit.Core.TestResult",
				"NUnit.Core.TestSuite",
				"NUnit.Framework.Assertion", 
				"NUnit.Framework.Assert",
                "System.Reflection.MonoMethod"
			};

            for (int i = 0; i < patterns.Length; i++)
            {
                if (line.IndexOf(patterns[i]) > 0)
                    return true;
            }

            return false;
        }

    }

}