using System;
using System.Windows.Browser;

namespace Lighthouse.Client.Logging
{
    public class ClientLogger : IClientLogger
    {
        private const string ClientLoggingMethodName = "ClientLogMessage";

        public bool SendClientLogMessage(string message)
        {
            return InvokeExternalMethod(ClientLoggingMethodName, new object[] {message});
        }

        private static bool InvokeExternalMethod(string name, object[] parameters)
        {
            var testResultsInformer = HtmlPage.Window.Eval("window.external") as ScriptObject;
            if (testResultsInformer != null)
            {
                try
                {
                    testResultsInformer.Invoke(name, parameters);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

    }
}