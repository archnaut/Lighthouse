using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Lighthouse.Common.Interoperability;
using Lighthouse.Silverlight.Core.Services;

namespace Lighthouse.Silverlight.Core.Controls
{
    public partial class WaitingPage : UserControl
    {
        public WaitingPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RemoteUnitTestingApplicationService.Current.Run(new SilverlightUnitTestRunSettings()
                                                                {
                                                                    AssembliesThatContainTests = new List<string>() { "Lighthouse.Silverlight4.SampleXapWithTests.dll" }
                                                                });

        }
    }
}
