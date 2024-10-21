namespace Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputPrompt = "Equation to be evaluated: ";
            Console.Write(inputPrompt);
            var input = Console.ReadLine()!.Trim();
            var lexer = new Lexer(input);
            foreach (var token in lexer.ParseAll())
            {
                Console.WriteLine(token);
            }

            Parser parser;
            IExpression result;
            try
            {
                parser = new Parser(lexer);
                result = parser.Parse();
                result = parser.Parse();
            }
            catch (Exception ex) when (ex is IErrorMessage)
            {
                var e = (IErrorMessage)ex;
                int seperatorLength;
                if ((inputPrompt.Length + input.Length) > e.Message.Length)
                {
                    seperatorLength = inputPrompt.Length + input.Length;
                }
                else
                {
                    seperatorLength = e.Message.Length;
                }

                Console.WriteLine(new string('-', seperatorLength));
                Console.WriteLine(e.Message);
                Console.WriteLine(input);
                Console.WriteLine(new string(' ', e.Start) + new string('^', e.End - e.Start + 1));
                Environment.Exit(1);
                throw;
            }

            var variables = new Dictionary<char, double>();
            foreach (var i in result.ListVariables(new List<Variable>()))
            {
                if (!variables.ContainsKey(i.Name))
                {
                    Console.Write($"{i} = ");
                    var v = Console.ReadLine()!;
                    if (!double.TryParse(v, out var vint)!)
                    {
                        throw new Exception("Invalid input");
                    }
                    else
                    {
                        variables.Add(i.Name, vint);
                    }
                }
            }

            Console.WriteLine(new string('-', inputPrompt.Length + input.Length));
            Console.WriteLine(result.Eval(
                variables
            ));
        }
    }
}