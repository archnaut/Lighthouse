using System;
using System.Threading;
using Lighthouse.Common.Ioc;
using LighthouseDesktop.Core;
using LighthouseDesktop.Core.Infrastructure.TestExecution;

namespace Lighthouse
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Initialize();
            var thread = new Thread(DoYourThing);
            thread.SetApartmentState(ApartmentState.STA); //we must set the thread to STA for this to work!
            thread.Start();

            thread.Join();
        }

        private static void DoYourThing()
        {
            var orchestrator = SimpleServiceLocator.Instance.Get<ITestExecutionOrchectrator>();

            AppDomain.CurrentDomain.ProcessExit += (s, a) => orchestrator.Cleanup();
            Console.CancelKeyPress += (s, a) => orchestrator.Abort();

            orchestrator.Execute();           
        }
    }
}
