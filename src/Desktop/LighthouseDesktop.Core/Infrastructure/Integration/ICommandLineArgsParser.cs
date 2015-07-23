using System.Collections.Generic;

namespace LighthouseDesktop.Core.Infrastructure.Integration
{
    public interface ICommandLineArgsParser
    {
        void Parse(string[] args);
        bool ParameterIsSet(string name);
        string GetParameterValue(string name);
        IList<string> Arguments { get; }
        void AddParameterSynonym(string name, IList<string> synonyms);
    }
}