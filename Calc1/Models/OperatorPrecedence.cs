namespace ModernCalculator.Models
{
    public class OperatorPrecedence
    {
        public enum Associative
        {
            NA,
            IsLeft,
            IsRight
        }
        public string Operator { get; set; } = string.Empty;
        public int Precedence { get; set; }
        public Associative Associativity { get; set; }
    }
}