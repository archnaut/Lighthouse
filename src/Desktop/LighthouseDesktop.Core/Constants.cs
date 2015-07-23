namespace LighthouseDesktop.Core
{
    public class Constants
    {
        public class Versions
        {
            public static double ConsoleRunnerVersion = 1.01;
            public static string VersionDisplayFormat = "{0:0.00}";
            public static string ConsoleRunnerVersionText = string.Format(VersionDisplayFormat, ConsoleRunnerVersion);
        }

        public class ResourceNames
        {
            public class DllNames
            {
                public static string LighthouseSilverlightTestRunnerAppDllName = "Lighthouse.Silverlight.TestRunnerApp.dll";
                public static string LighthouseSilverlightCoreDllName = "Lighthouse.Silverlight.Core.dll";
                public static string LighthouseSilverlightCommonDllName = "Lighthouse.Common.dll";

                public static string MicrosoftSilverlightTestingDllName = "Microsoft.Silverlight.Testing.dll";
                public static string MicrosoftSilverlightTestingQualityToolsDllName = "Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll";
            }

            public class PdbNames
            {
                public static string LighthouseSilverlightTestRunnerAppDllName = "Lighthouse.Silverlight.TestRunnerApp.pdb";
                public static string LighthouseSilverlightCoreDllName = "Lighthouse.Silverlight.Core.pdb";
                public static string LighthouseSilverlightCommonDllName = "Lighthouse.Common.pdb";

                public static string MicrosoftSilverlightTestingDllName = "Microsoft.Silverlight.Testing.pdb";
                public static string MicrosoftSilverlightTestingQualityToolsDllName = "Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.pdb";
            }

        }
    }

}