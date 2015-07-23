using System;
using Lighthouse.Common.Interoperability;

namespace LighthouseDesktop.Core.Infrastructure.TestExecution
{
    public class RemoteTestExecutionSettings
    {
        public Uri XapUri { get; set; }

        private SilverlightUnitTestRunSettings _unitTestRunSettings = new SilverlightUnitTestRunSettings();
        public SilverlightUnitTestRunSettings SilverlightUnitTestRunSettings
        {
            get { return _unitTestRunSettings; }
            set { _unitTestRunSettings = value; }
        }

        private int _timeoutInSeconds = 60;
        public int TimeoutInSeconds
        {
            get { return _timeoutInSeconds; }
            set { _timeoutInSeconds = value; }
        }
    }
}