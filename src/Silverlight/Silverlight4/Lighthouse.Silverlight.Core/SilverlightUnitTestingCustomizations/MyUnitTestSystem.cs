using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using Lighthouse.Common.Interoperability;
using Lighthouse.Silverlight.Core.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Client;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.Service;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace Lighthouse.Silverlight.Core.SilverlightUnitTestingCustomizations
{
    public class MyUnitTestSystem
    {
        /// <summary>
        /// Friendly unit test system name.
        /// </summary>
        private const string UnitTestSystemName = "Silverlight Unit Test Framework";

        /// <summary>
        /// Gets the test system name built into the assembly.
        /// </summary>
        public static string SystemName { get { return UnitTestSystemName; } }

        /// <summary>
        /// Gets a string representing the file version attribute of the main
        /// unit test framework assembly, if present.
        /// </summary>
        public static string FrameworkFileVersion
        {
            get
            {
                Assembly utf = typeof(UnitTestSystem).Assembly;
                return utf.ImageRuntimeVersion;
            }
        }

        /// <summary>
        /// Register another available unit test provider for the unit test system.
        /// </summary>
        /// <param name="provider">A unit test provider.</param>
        public static void RegisterUnitTestProvider(IUnitTestProvider provider)
        {
            if (!UnitTestProviders.Providers.Contains(provider))
            {
                UnitTestProviders.Providers.Add(provider);
            }
        }

        /// <summary>
        /// Test harness instance.
        /// </summary>
        private UnitTestHarness _harness;

        /// <summary>
        /// Start a new unit test run.
        /// </summary>
        /// <param name="settings">Unit test settings object.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This makes the purpose clear to test developers")]
        public void Run(UnitTestSettings settings)
        {
            // Avoid having the Run method called twice
            if (_harness != null)
            {
                return;
            }

            _harness = settings.TestHarness;
            if (_harness == null)
            {
                throw new InvalidOperationException("NoTestHarnessInSettings");
            }

            // Conside re-setting the test service only in our default case
            if (settings.TestService is SilverlightTestService)
            {
                SetTestService(settings);
            }

            _harness.Settings = settings;
            _harness.TestHarnessCompleted += (sender, args) => OnTestHarnessCompleted(args);

            if (settings.StartRunImmediately)
            {
                _harness.Run();
            }
        }

        /// <summary>
        /// Prepares the default log manager.
        /// </summary>
        /// <param name="settings">The test harness settings.</param>
        public static void SetStandardLogProviders(UnitTestSettings settings)
        {
            // Debug provider
            DebugOutputProvider debugger = new DebugOutputProvider();
            debugger.ShowAllFailures = true;
            settings.LogProviders.Add(debugger);

            // Visual Studio log provider
            try
            {
                TryAddVisualStudioLogProvider(settings);
            }
            catch
            {
            }

            PrepareCustomLogProviders(settings);
        }

        /// <summary>
        /// Tries to instantiate and initialize a VSTT provider. Requires that 
        /// XLinq is available and included in the application package.
        /// </summary>
        /// <param name="settings">The test harness settings object.</param>
        private static void TryAddVisualStudioLogProvider(UnitTestSettings settings)
        {
            VisualStudioLogProvider trx = new VisualStudioLogProvider();
            settings.LogProviders.Add(trx);
        }

        /// <summary>
        /// Creates the default settings that would be used by the UnitTestHarness
        /// if none were specified.
        /// </summary>
        /// <returns>A new RootVisual.</returns>
        /// <remarks>Assumes the calling assembly is a test assembly.</remarks>
        public static UnitTestSettings CreateDefaultSettings()
        {
/*            foreach (var assemblyPart in Deployment.Current.Parts)
            {
                var sri = Application.GetResourceStream(new Uri(assemblyPart.Source, UriKind.Relative));

                try
                {
                    var assembly = new AssemblyPart().Load(sri.Stream);
                    assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format("Skipping Assembly: {0}. Error: {1}.", assemblyPart.Source, e.Message));         
                }
            }*/

            return CreateDefaultSettings(Assembly.GetCallingAssembly());
        }

        public static UnitTestSettings CreateDefaultSettings(SilverlightUnitTestRunSettings silverlightUnitTestRunSettings)
        {
            var settings = new UnitTestSettings();
            settings.ShowTagExpressionEditor = false;
            settings.SampleTags = new List<string>();
            settings.TagExpression = null;
            
            if (silverlightUnitTestRunSettings != null)
            {
                if (silverlightUnitTestRunSettings.AssembliesThatContainTests.Any())
                {
                    foreach (var assemblyPartDllName in silverlightUnitTestRunSettings.AssembliesThatContainTests)
                    {
                        var partDllNameCopy = assemblyPartDllName;
                        var foundAssemblyPart = Deployment.Current.Parts.FirstOrDefault(p => p.Source == partDllNameCopy);
                        if (foundAssemblyPart != null)
                        {
                            var sri = Application.GetResourceStream(new Uri(foundAssemblyPart.Source, UriKind.Relative));
                            var assembly = new AssemblyPart().Load(sri.Stream);
                            settings.TestAssemblies.Add(assembly);                            
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(silverlightUnitTestRunSettings.TagFilter))
                {
                    settings.TagExpression = silverlightUnitTestRunSettings.TagFilter;
                }
            }

            SetStandardLogProviders(settings);
            settings.TestHarness = new LighthouseUnitTestHarness();
            settings.TestHarness.Settings = settings;
            // Sets initial but user can override
            SetTestService(settings);
            return settings;
        }

        /// <summary>
        /// A completed test harness handler.
        /// </summary>
        public event EventHandler<TestHarnessCompletedEventArgs> TestHarnessCompleted;

        /// <summary>
        /// Call the TestHarnessCompleted event.
        /// </summary>
        /// <param name="args">The test harness completed event arguments.</param>
        private void OnTestHarnessCompleted(TestHarnessCompletedEventArgs args)
        {
            var handler = TestHarnessCompleted;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Create a default settings object for unit testing.
        /// </summary>
        /// <param name="callingAssembly">The assembly reflection object.</param>
        /// <returns>A unit test settings instance.</returns>
        private static UnitTestSettings CreateDefaultSettings(Assembly callingAssembly)
        {
            return CreateDefaultSettings(new List<Assembly> { callingAssembly });            
        }

        private static UnitTestSettings CreateDefaultSettings(IEnumerable<Assembly> callingAssemblies)
        {
            var settings = new UnitTestSettings();
            settings.ShowTagExpressionEditor = false;
            settings.SampleTags = new List<string>();
            settings.TagExpression = null;

            if (callingAssemblies != null)
            {
                foreach (var callingAssembly in callingAssemblies)
                {
                    if (!settings.TestAssemblies.Contains(callingAssembly))
                    {
                        settings.TestAssemblies.Add(callingAssembly);
                    }
                }                
            }
            SetStandardLogProviders(settings);
            settings.TestHarness = new LighthouseUnitTestHarness();
            settings.TestHarness.Settings = settings;
            // Sets initial but user can override
            SetTestService(settings);
            return settings;
        }

        public static void PrepareCustomLogProviders(UnitTestSettings settings)
        {
            // TODO: Consider what to do on this one...
            // Should probably update to use the newer log system with events,
            // and then after that figure out when it applies... perhaps only
            // when someone first requests to use it.
            ////if (HtmlPage.IsEnabled)
            ////{
            ////settings.LogProviders.Add(new TextFailuresLogProvider());
            ////}
        }

        public static void SetTestService(UnitTestSettings settings)
        {
            settings.TestService = new SilverlightTestService(settings);
        }

        /// <summary>
        /// Creates a new TestPage visual that in turn will setup and begin a 
        /// unit test run.
        /// </summary>
        /// <returns>A new RootVisual.</returns>
        /// <remarks>Assumes the calling assembly is a test assembly.</remarks>
        public static UIElement CreateTestPage()
        {
            UnitTestSettings settings = CreateDefaultSettings(Assembly.GetCallingAssembly());
            return CreateTestPage(settings);
        }

        /// <summary>
        /// Creates a new TestPage visual that in turn will setup and begin a 
        /// unit test run.
        /// </summary>
        /// <param name="settings">Test harness settings to be applied.</param>
        /// <returns>A new RootVisual.</returns>
        /// <remarks>Assumes the calling assembly is a test assembly.</remarks>
        public static UIElement CreateTestPage(UnitTestSettings settings)
        {
            var system = new MyUnitTestSystem();

            Type testPageType = Environment.OSVersion.Platform == PlatformID.WinCE ? typeof(MobileTestPage) : typeof(LighthouseUnitTestRunnerPage);

            Type testPageInterface = typeof(ITestPage);
            if (settings.TestPanelType != null && testPageInterface.IsAssignableFrom(settings.TestPanelType))
            {
                testPageType = settings.TestPanelType;
            }

            object testPage;
            try
            {
                // Try creating with an instance of the test harness
                testPage = Activator.CreateInstance(testPageType, settings.TestHarness);
            }
            catch (Exception e)
            {
                // Fall back to a standard instance only
                testPage = Activator.CreateInstance(testPageType);
            }

            PrepareTestService(settings, () => system.Run(settings));

            // NOTE: A silent failure would be if the testPanel is not a
            // UIElement, and it returns anyway.
            return testPage as UIElement;
        }

        private static void MergeSettingsAndParameters(TestServiceProvider testService, UnitTestSettings inputSettings)
        {
            if (testService != null && testService.HasService(TestServiceFeature.RunSettings))
            {
                SettingsProvider settings = testService.GetService<SettingsProvider>(TestServiceFeature.RunSettings);
                foreach (string key in settings.Settings.Keys)
                {
                    if (inputSettings.Parameters.ContainsKey(key))
                    {
                        Debug.WriteLine("MergeSettingsAndParameters: Overwriting " + key + " key during merge.");
                    }
                    inputSettings.Parameters[key] = settings.Settings[key];
                }
            }
        }

        private static void PrepareTestService(UnitTestSettings inputSettings, Action complete)
        {
            TestServiceProvider testService = inputSettings.TestService;
            if (testService != null && testService.Initialized == false)
            {
                Action after = delegate
                {
                    MergeSettingsAndParameters(testService, inputSettings);
                    complete();
                };
                testService.InitializeCompleted += delegate(object sender, EventArgs e) { after(); };
                testService.Initialize();
            }
            else
            {
                complete();
            }
        }

    }
}