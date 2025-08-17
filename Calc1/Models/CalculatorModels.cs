using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernCalculator.Models
{
    /// <summary>
    /// Enhanced calculator operation model with additional functionality
    /// </summary>
    public partial class CalculatorOperation : ObservableObject
    {
        [ObservableProperty]
        private string _displayValue = "0";

        [ObservableProperty]
        private string _expression = "";

        [ObservableProperty]
        private double _memoryValue;

        [ObservableProperty]
        private bool _isError;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isInRadianMode = true;

        [ObservableProperty]
        private int _decimalPlaces = 10;

        [ObservableProperty]
        private bool _isScientificNotation;

        /// <summary>
        /// Gets or sets the current calculator mode
        /// </summary>
        [ObservableProperty]
        private CalculatorMode _calculatorMode = CalculatorMode.Standard;

        /// <summary>
        /// Gets or sets the number system for programmer mode
        /// </summary>
        [ObservableProperty]
        private NumberSystem _numberSystem = NumberSystem.Decimal;

        /// <summary>
        /// Resets the calculator to initial state
        /// </summary>
        public void Reset()
        {
            DisplayValue = "0";
            Expression = "";
            IsError = false;
            ErrorMessage = "";
            // Keep memory, mode, and other settings
        }

        /// <summary>
        /// Clears everything including memory
        /// </summary>
        public void ClearAll()
        {
            Reset();
            MemoryValue = 0;
        }

        /// <summary>
        /// Formats a number for display based on current settings
        /// </summary>
        public string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "Error";
            }

            try
            {
                if (IsScientificNotation || Math.Abs(value) >= Math.Pow(10, DecimalPlaces) || (Math.Abs(value) < 0.000001 && value != 0))
                {
                    return value.ToString($"E{DecimalPlaces}");
                }

                return Math.Round(value, DecimalPlaces).ToString();
            }
            catch
            {
                return "Error";
            }
        }

        /// <summary>
        /// Converts angle from degrees to radians if needed
        /// </summary>
        public double ConvertAngle(double angle)
        {
            return IsInRadianMode ? angle : angle * Math.PI / 180.0;
        }

        /// <summary>
        /// Converts angle from radians to degrees if needed
        /// </summary>
        public double ConvertAngleResult(double angle)
        {
            return IsInRadianMode ? angle : angle * 180.0 / Math.PI;
        }
    }

    /// <summary>
    /// Model for representing calculator buttons
    /// </summary>
    public partial class CalculatorButton : ObservableObject
    {
        [ObservableProperty]
        private string _content = "";

        [ObservableProperty]
        private string _command = "";

        [ObservableProperty]
        private string _tooltip = "";

        [ObservableProperty]
        private ButtonType _buttonType = ButtonType.Number;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private string _shortcutKey = "";

        public CalculatorButton(string content, string command, ButtonType buttonType, string tooltip = "", string shortcutKey = "")
        {
            Content = content;
            Command = command;
            ButtonType = buttonType;
            Tooltip = string.IsNullOrEmpty(tooltip) ? content : tooltip;
            ShortcutKey = shortcutKey;
        }
    }

    /// <summary>
    /// Types of calculator buttons
    /// </summary>
    public enum ButtonType
    {
        Number,
        Operator,
        Function,
        Memory,
        Control,
        Scientific,
        Programmer
    }

    /// <summary>
    /// Calculator modes
    /// </summary>
    public enum CalculatorMode
    {
        Standard,
        Scientific,
        Programmer,
        Statistics
    }

    /// <summary>
    /// Number systems for programmer mode
    /// </summary>
    public enum NumberSystem
    {
        Binary = 2,
        Octal = 8,
        Decimal = 10,
        Hexadecimal = 16
    }

    /// <summary>
    /// Memory operation types
    /// </summary>
    public enum MemoryOperation
    {
        Store,
        Recall,
        Clear,
        Add,
        Subtract
    }
}