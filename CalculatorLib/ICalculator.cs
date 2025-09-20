namespace CalculatorLib;

public interface ICalculator
{
    /// <summary>
    /// Evaluates a simple arithmetic expression containing + - * / and parentheses.
    /// Throws ArgumentException for invalid input.
    /// </summary>
    double Evaluate(string expression);
}