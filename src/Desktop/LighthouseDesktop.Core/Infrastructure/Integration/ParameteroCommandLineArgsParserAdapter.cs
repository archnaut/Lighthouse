using System;
using System.Collections.Generic;
using CommandLineParsing;

namespace LighthouseDesktop.Core.Infrastructure.Integration
{
    public class ParameteroCommandLineArgsParserAdapter : ICommandLineArgsParser
    {
        private readonly Parametero _parser;

        public void AddParameterSynonym(string name, IList<string> synonyms)
        {
            _parser.Settings.Synonyms.Add(new ParameterSynonym() {Name = name, SynonymNames = synonyms});
        }

        public ParameteroCommandLineArgsParserAdapter()
        {
            _parser = new Parametero();
        }

        public IList<string> Arguments
        {
            get { return _parser.Arguments; }
        }

        public void Parse(string[] args)
        {
            _parser.Parse(args);
        }

        public bool ParameterIsSet(string name)
        {
            return _parser.ParameterIsSet(name);
        }

        public string GetParameterValue(string name)
        {
            return _parser.GetParameterValue(name);
        }
    }
}