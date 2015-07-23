using System.Linq;
using Lighthouse.Common.SilverlightUnitTestingAbstractions;

namespace LighthouseDesktop.Core.ExtensionMethods
{
    public static class ComposedUnitTestOutcomeExtensions
    {
        public static bool Succeeded(this IComposedUnitTestOutcome outcome)
        {
            return !outcome.TestResults.Any(p => p.Result != UnitTestOutcome.Passed);
        }

        public static int TotalNumberOfTestsExecuted(this IComposedUnitTestOutcome outcome)
        {
            return outcome.TestResults.Count;
        }

        public static int NumberOfErrors(this IComposedUnitTestOutcome outcome)
        {
            return outcome.NumberOf(UnitTestOutcome.Error);
        }

        private static int NumberOf(this IComposedUnitTestOutcome outcome, UnitTestOutcome toFind)
        {
            return outcome.TestResults.Where(p => p.Result == toFind).Count();
        }

        public static int NumberOfFailures(this IComposedUnitTestOutcome outcome)
        {
            return outcome.NumberOf(UnitTestOutcome.Failed);
        }

        public static int NumberOfInconclusive(this IComposedUnitTestOutcome outcome)
        {
            return outcome.NumberOf(UnitTestOutcome.Inconclusive);
        }

        public static int NumberOfNotExecuted(this IComposedUnitTestOutcome outcome)
        {
            return outcome.NumberOf(UnitTestOutcome.NotExecuted);
        }

        public static double ExecutionTimeInMiliseconds(this IComposedUnitTestOutcome outcome)
        {
            return outcome.TestResults.Sum(p => (p.Finished - p.Started).TotalMilliseconds);
        }

        

    }
}