using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModernCalculator.Models;

namespace ModernCalculator.Services
{
    /// <summary>
    /// Advanced calculation engine that builds upon the existing RPN implementation
    /// </summary>
    public class AdvancedCalculationEngine : ICalculationEngine
    {
        private readonly Dictionary<string, Func<double[], double>> _functions;

        public AdvancedCalculationEngine()
        {
            _functions = InitializeFunctions();
        }

        private Dictionary<string, Func<double[], double>> InitializeFunctions()
        {
            return new Dictionary<string, Func<double[], double>>(StringComparer.OrdinalIgnoreCase)
            {
                { "sin", args => Math.Sin(args[0]) },
                { "cos", args => Math.Cos(args[0]) },
                { "tan", args => Math.Tan(args[0]) },
                { "asin", args => Math.Asin(args[0]) },
                { "acos", args => Math.Acos(args[0]) },
                { "atan", args => Math.Atan(args[0]) },
                { "log", args => args.Length == 1 ? Math.Log10(args[0]) : Math.Log(args[0], args[1]) },
                { "ln", args => Math.Log(args[0]) },
                { "sqrt", args => Math.Sqrt(args[0]) },
                { "abs", args => Math.Abs(args[0]) },
                { "ceil", args => Math.Ceiling(args[0]) },
                { "floor", args => Math.Floor(args[0]) },
                { "round", args => args.Length == 1 ? Math.Round(args[0]) : Math.Round(args[0], (int)args[1]) },
                { "factorial", args => Factorial(args[0]) },
                { "pi", _ => Math.PI },
                { "e", _ => Math.E }
            };
        }

        public async Task<CalculationResult> EvaluateAsync(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    return new CalculationResult { HasError = true, ErrorMessage = "Empty expression" };
                }

                // Preprocess the expression to handle functions and constants
                var processedExpression = PreprocessExpression(expression);

                // Tokenize the expression
                var tokens = TokenizeExpression(processedExpression);

                // Use the existing Shunt Yard algorithm
                var rpnResult = ShuntYard.ShuntRpnModel(tokens);

                if (!string.IsNullOrEmpty(rpnResult.Item2))
                {
                    return new CalculationResult
                    {
                        HasError = true,
                        ErrorMessage = rpnResult.Item2,
                        Expression = expression
                    };
                }

                // Calculate using RPN
                var calculationResult = CalculateRpn.RpnResult(rpnResult.Item1);

                if (!string.IsNullOrEmpty(calculationResult.Item2))
                {
                    return new CalculationResult
                    {
                        HasError = true,
                        ErrorMessage = calculationResult.Item2,
                        Expression = expression
                    };
                }

                return new CalculationResult
                {
                    Value = calculationResult.Item1,
                    Expression = expression,
                    HasError = false
                };
            }
            catch (Exception ex)
            {
                return new CalculationResult
                {
                    HasError = true,
                    ErrorMessage = ex.Message,
                    Expression = expression
                };
            }
        }

        public async Task<CalculationResult> EvaluateRpnAsync(string expression)
        {
            return await EvaluateAsync(expression);
        }

        private string PreprocessExpression(string expression)
        {
            // Handle implicit multiplication (e.g., 2pi, 3(2+1))
            expression = Regex.Replace(expression, @"(\d)([a-zA-Z\(])", "$1*$2");
            expression = Regex.Replace(expression, @"([a-zA-Z\)])(\d)", "$1*$2");
            expression = Regex.Replace(expression, @"(\))(\()", "$1*$2");

            // Replace constants
            expression = Regex.Replace(expression, @"\bpi\b", Math.PI.ToString(), RegexOptions.IgnoreCase);
            expression = Regex.Replace(expression, @"\be\b", Math.E.ToString(), RegexOptions.IgnoreCase);

            return expression;
        }

        private List<string> TokenizeExpression(string expression)
        {
            var tokens = new List<string>();
            var regex = new Regex(@"(\d*\.?\d+)|([+\-*/\^()])|([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);
            var matches = regex.Matches(expression);

            foreach (Match match in matches)
            {
                var token = match.Value.Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    // Handle function calls
                    if (_functions.ContainsKey(token))
                    {
                        tokens.Add(token);
                    }
                    else
                    {
                        tokens.Add(token);
                    }
                }
            }

            return tokens;
        }

        public string ConvertNumber(string value, NumberSystem from, NumberSystem to)
        {
            try
            {
                // Parse the number based on the source base
                long decimalValue = from switch
                {
                    NumberSystem.Binary => Convert.ToInt64(value, 2),
                    NumberSystem.Octal => Convert.ToInt64(value, 8),
                    NumberSystem.Decimal => Convert.ToInt64(value, 10),
                    NumberSystem.Hexadecimal => Convert.ToInt64(value, 16),
                    _ => throw new ArgumentException("Unsupported number system")
                };

                // Convert to target base
                return to switch
                {
                    NumberSystem.Binary => Convert.ToString(decimalValue, 2),
                    NumberSystem.Octal => Convert.ToString(decimalValue, 8),
                    NumberSystem.Decimal => decimalValue.ToString(),
                    NumberSystem.Hexadecimal => Convert.ToString(decimalValue, 16).ToUpper(),
                    _ => throw new ArgumentException("Unsupported number system")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting number: {ex.Message}");
            }
        }

        // Basic arithmetic operations
        public double Add(double a, double b) => a + b;
        public double Subtract(double a, double b) => a - b;
        public double Multiply(double a, double b) => a * b;
        public double Divide(double a, double b) => b != 0 ? a / b : throw new DivideByZeroException();

        // Advanced operations
        public double Power(double baseValue, double exponent) => Math.Pow(baseValue, exponent);
        public double SquareRoot(double value) => value >= 0 ? Math.Sqrt(value) : throw new ArgumentException("Cannot calculate square root of negative number");
        public double NthRoot(double value, double n) => Math.Pow(value, 1.0 / n);
        public double Logarithm(double value, double baseValue = Math.E) => value > 0 ? Math.Log(value, baseValue) : throw new ArgumentException("Cannot calculate logarithm of non-positive number");
        public double Percentage(double value, double percent) => value * percent / 100.0;

        public double Factorial(double value)
        {
            if (value < 0) throw new ArgumentException("Cannot calculate factorial of negative number");
            if (value > 170) throw new ArgumentException("Number too large for factorial calculation");

            var n = (int)Math.Floor(value);
            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        // Trigonometric functions
        public double Sin(double angleInRadians) => Math.Sin(angleInRadians);
        public double Cos(double angleInRadians) => Math.Cos(angleInRadians);
        public double Tan(double angleInRadians) => Math.Tan(angleInRadians);
        public double Asin(double value) => Math.Asin(value);
        public double Acos(double value) => Math.Acos(value);
        public double Atan(double value) => Math.Atan(value);

        // Statistical functions
        public double Mean(IEnumerable<double> values)
        {
            var valueList = values.ToList();
            return valueList.Count > 0 ? valueList.Average() : 0;
        }

        public double StandardDeviation(IEnumerable<double> values)
        {
            var valueList = values.ToList();
            if (valueList.Count < 2) return 0;

            var mean = Mean(valueList);
            var sumOfSquares = valueList.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquares / (valueList.Count - 1));
        }

        public double Variance(IEnumerable<double> values)
        {
            var stdDev = StandardDeviation(values);
            return stdDev * stdDev;
        }
    }
}
