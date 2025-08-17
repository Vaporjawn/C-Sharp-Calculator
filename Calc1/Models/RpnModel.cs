namespace ModernCalculator.Models
{
    public class RpnModel
    {
        public enum TypeofToken : int
        {
            IsOperator,
            IsValue
        }
        public string Token { get; set; } = string.Empty;
        public TypeofToken TokenType { get; set; }
    }
}
