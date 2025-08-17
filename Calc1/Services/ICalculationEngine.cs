using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernCalculator.Services
{
    /// <summary>
    /// Represents different calculation modes
    /// </summary>
    public enum CalculatorMode
    {
        Standard,
        Scientific,
        Programmer,
        Statistics
    }

    /// <summary>
    /// Represents different number systems for programmer mode
    /// </summary>
    public enum NumberSystem
    {
        Decimal = 10,
        Binary = 2,
        Octal = 8,
        Hexadecimal = 16
    }

    /// <summary>
    /// Represents the result of a calculation
    /// </summary>
    public record CalculationResult
    {
        public double Value { get; init; }
        public string Expression { get; init; } = string.Empty;
        public bool HasError { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.Now;
    }

    /// <summary>
    /// Main interface for calculation engine
    /// </summary>
    public interface ICalculationEngine
    {
        /// <summary>
        /// Evaluates a mathematical expression
        /// </summary>
        Task<CalculationResult> EvaluateAsync(string expression);

        /// <summary>
        /// Evaluates using RPN notation
        /// </summary>
        Task<CalculationResult> EvaluateRpnAsync(string expression);

        /// <summary>
        /// Converts number between different bases (for programmer mode)
        /// </summary>
        string ConvertNumber(string value, NumberSystem from, NumberSystem to);

        /// <summary>
        /// Performs basic arithmetic operations
        /// </summary>
        double Add(double a, double b);
        double Subtract(double a, double b);
        double Multiply(double a, double b);
        double Divide(double a, double b);

        /// <summary>
        /// Advanced mathematical operations
        /// </summary>
        double Power(double baseValue, double exponent);
        double SquareRoot(double value);
        double NthRoot(double value, double n);
        double Factorial(double value);
        double Logarithm(double value, double baseValue = Math.E);
        double Percentage(double value, double percent);

        /// <summary>
        /// Trigonometric functions
        /// </summary>
        double Sin(double angleInRadians);
        double Cos(double angleInRadians);
        double Tan(double angleInRadians);
        double Asin(double value);
        double Acos(double value);
        double Atan(double value);

        /// <summary>
        /// Statistical functions
        /// </summary>
        double Mean(IEnumerable<double> values);
        double StandardDeviation(IEnumerable<double> values);
        double Variance(IEnumerable<double> values);
    }
}
