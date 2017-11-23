using GeneticTree.BooleanLogicParser;
using GeneticTree.Signal;
using QuantConnect.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect;

namespace GeneticTree
{

    public class Rule
    {

        public IEnumerable<ISignal> List { get; }
        public Symbol Symbol { get; }
        public string Expression { get; }
        public Rule(Symbol symbol, IEnumerable<ISignal> signal)
        {
            List = signal;
            Symbol = symbol;
        }

        public Rule(Symbol symbol, IEnumerable<ISignal> signal, string expression)
        {
            List = signal;
            Expression = expression;
            Symbol = symbol;
        }

        public bool IsReady()
        {
            return List.All(s => s.IsReady);
        }

        public bool IsTrue()
        {
            if(Expression.Length>0)
            {
                return FixedExpressionIsTrue();
            }
            else
            {
                return GeneticIsTrue();
            }
        }

        private bool FixedExpressionIsTrue()
        {
            object[] signals = new object[List.Count()];
            for (int i = 0; i < List.Count(); i++)
            {
                signals[i] = List.ElementAt(i).IsTrue();
            }

            var tokens = new Tokenizer(string.Format(Expression, signals)).Tokenize();
            var parser = new Parser(tokens);
            var result = parser.Parse();
            return result;
        }

        private bool GeneticIsTrue()
        {
            string expression = "";

            foreach (var item in List)
            {
                string isTrue = item.IsTrue().ToString().ToLower();

                if (new[] { Operator.NorInclusive, Operator.OrInclusive }.Contains(item.Operator))
                {
                    isTrue = "(" + isTrue;
                }

                if (item.Parent != null && new[] { Operator.NorInclusive, Operator.OrInclusive }.Contains(item.Parent.Operator))
                {
                    isTrue += ")";
                }

                expression = expression + (isTrue);

                if (item.Child != null)
                {
                    if (item.Operator == Operator.And)
                    {
                        expression = expression + (" and ");
                    }
                    else if (new[] { Operator.Or, Operator.OrInclusive }.Contains(item.Operator))
                    {
                        expression = expression + (" or ");
                    }
                    else if (item.Operator == Operator.Not)
                    {
                        expression = expression + (" and !");
                    }
                    else if (new[] { Operator.Nor, Operator.NorInclusive }.Contains(item.Operator))
                    {
                        expression = expression + (" or !");
                    }
                }
            }

            var tokens = new Tokenizer(expression.ToString()).Tokenize();
            var parser = new Parser(tokens);
            var result = parser.Parse();
            return result;
        }


        public void Update(BaseData data)
        {
            List.First().Update(data);
        }

    }
}