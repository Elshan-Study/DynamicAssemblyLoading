using System.Globalization;

namespace CalculatorLib;

public class CalculatorService : ICalculator
{
    public double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is empty", nameof(expression));

        var rpn = ToRpn(expression);
        return EvalRpn(rpn);
    }
   
    private static Queue<string> ToRpn(string input)
    {
        var output = new Queue<string>();
        var ops = new Stack<char>();
        var i = 0;
        while (i < input.Length)
        {
            var c = input[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || c == '.')
            {
                var start = i;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.')) i++;
                var number = input.Substring(start, i - start);
                output.Enqueue(number);
                continue;
            }

            switch (c)
            {
                case '(':
                    ops.Push(c);
                    i++; continue;
                case ')':
                {
                    while (ops.Count > 0 && ops.Peek() != '(')
                        output.Enqueue(ops.Pop().ToString());
                    if (ops.Count == 0) throw new ArgumentException("Mismatched parentheses");
                    ops.Pop();
                    i++; continue;
                }
            }

            if (IsOperator(c))
            {
                while (ops.Count > 0 && IsOperator(ops.Peek()) &&
                      ((IsLeftAssociative(c) && Precedence(c) <= Precedence(ops.Peek())) ||
                       (!IsLeftAssociative(c) && Precedence(c) < Precedence(ops.Peek()))))
                {
                    output.Enqueue(ops.Pop().ToString());
                }
                ops.Push(c);
                i++; continue;
            }

            throw new ArgumentException($"Invalid character: {c}");
        }

        while (ops.Count > 0)
        {
            var op = ops.Pop();
            if (op is '(' or ')') throw new ArgumentException("Mismatched parentheses");
            output.Enqueue(op.ToString());
        }

        return output;
    }

    private static double EvalRpn(Queue<string> rpn)
    {
        var stack = new Stack<double>();
        while (rpn.Count > 0)
        {
            var token = rpn.Dequeue();
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            {
                stack.Push(num);
                continue;
            }

            if (token.Length != 1 || !IsOperator(token[0]))
                throw new ArgumentException($"Invalid token in RPN: {token}");
            if (stack.Count < 2) throw new ArgumentException("Invalid expression");
            var b = stack.Pop();
            var a = stack.Pop();
            stack.Push(ApplyOp(a, b, token[0]));
            continue;

        }

        if (stack.Count != 1) throw new ArgumentException("Invalid expression");
        return stack.Pop();
    }

    private static double ApplyOp(double a, double b, char op) => op switch
    {
        '+' => a + b,
        '-' => a - b,
        '*' => a * b,
        '/' => b == 0 ? throw new DivideByZeroException() : a / b,
        _ => throw new ArgumentException("Unknown operator")
    };

    private static bool IsOperator(char c) => c is '+' or '-' or '*' or '/';
    private static int Precedence(char c) => c is '+' or '-' ? 1 : 2;
    private static bool IsLeftAssociative(char c) => c is '+' or '-' or '*' or '/';
}