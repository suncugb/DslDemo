using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RateEngine.DataObjects;

namespace RateEngine
{
    public class RateParser
    {
        private  bool IsParser { get; set; }
        private static RateParser _parser;
        private RateParser(){}
        public static RateParser Instance
        {
            get
            {
                if (_parser == null)
                {
                    _parser = new RateParser();
                }
                return _parser;
            }
        }
        public void ToParser()
        {
            if (IsParser)
            {
                return;
            }
            string filepath = Directory.GetCurrentDirectory() + "\\" + "rate.config";
            using (FileStream fS = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                StreamReader sr = new StreamReader(fS, Encoding.UTF8);
                string input = sr.ReadToEnd();
                AntlrInputStream inputStream = new AntlrInputStream(input);
                RateGrammarLexer lexer = new RateGrammarLexer(inputStream);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                RateGrammarParser parser = new RateGrammarParser(tokens);
                IParseTree tree = parser.configfile();
                Result ret = RuleVisitor.Instance.Visit(tree);
            }
            IsParser = true;
        }
    }
}
