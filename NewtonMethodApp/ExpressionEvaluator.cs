using System;
using System.Globalization;

namespace NewtonMethodApp
{
    public static class ExpressionEvaluator
    {
        public static double Eval(string s, double x)
        {
            int pos = 0;
            double v = ParseExpr(s, ref pos, x);
            SkipWs(s, ref pos);
            if (pos != s.Length)
                throw new FormatException($"Лишний символ '{s[pos]}' в формуле (позиция {pos}).");
            return v;
        }

        private static void SkipWs(string s, ref int p)
        {
            while (p < s.Length && char.IsWhiteSpace(s[p])) p++;
        }

        private static double ParseExpr(string s, ref int p, double x)
        {
            double v = ParseTerm(s, ref p, x);
            while (true)
            {
                SkipWs(s, ref p);
                if (p < s.Length && (s[p] == '+' || s[p] == '-'))
                {
                    char op = s[p++];
                    double r = ParseTerm(s, ref p, x);
                    v = op == '+' ? v + r : v - r;
                }
                else break;
            }
            return v;
        }

        private static double ParseTerm(string s, ref int p, double x)
        {
            double v = ParseFactor(s, ref p, x);
            while (true)
            {
                SkipWs(s, ref p);
                if (p < s.Length && (s[p] == '*' || s[p] == '/'))
                {
                    char op = s[p++];
                    double r = ParseFactor(s, ref p, x);
                    if (op == '/' && r == 0) throw new DivideByZeroException("Деление на ноль в формуле.");
                    v = op == '*' ? v * r : v / r;
                }
                else break;
            }
            return v;
        }

        private static double ParseFactor(string s, ref int p, double x)
        {
            SkipWs(s, ref p);
            if (p < s.Length && (s[p] == '+' || s[p] == '-'))
            {
                char sign = s[p++];
                double v = ParseFactor(s, ref p, x);
                return sign == '-' ? -v : v;
            }

            return ParsePower(s, ref p, x);
        }

        private static double ParsePower(string s, ref int p, double x)
        {
            double b = ParseAtom(s, ref p, x);
            SkipWs(s, ref p);
            if (p < s.Length && s[p] == '^')
            {
                p++;
                double exp = ParsePower(s, ref p, x);
                return Math.Pow(b, exp);
            }
            return b;
        }

        private static double ParseAtom(string s, ref int p, double x)
        {
            SkipWs(s, ref p);
            if (p >= s.Length) throw new FormatException("Неожиданный конец формулы.");

            if (s[p] == '(')
            {
                p++;
                double v = ParseExpr(s, ref p, x);
                SkipWs(s, ref p);
                if (p >= s.Length || s[p] != ')') throw new FormatException("Ожидалась ')'.");
                p++;
                return v;
            }

            if (char.IsDigit(s[p]) || s[p] == '.')
            {
                int start = p;
                while (p < s.Length && (char.IsDigit(s[p]) || s[p] == '.')) p++;
                // Экспонента 1e-3
                if (p < s.Length && (s[p] == 'e' || s[p] == 'E'))
                {
                    p++;
                    if (p < s.Length && (s[p] == '+' || s[p] == '-')) p++;
                    while (p < s.Length && char.IsDigit(s[p])) p++;
                }
                string num = s.Substring(start, p - start);
                return double.Parse(num, CultureInfo.InvariantCulture);
            }

            // Идентификатор
            if (char.IsLetter(s[p]) || s[p] == '_')
            {
                int start = p;
                while (p < s.Length && (char.IsLetterOrDigit(s[p]) || s[p] == '_')) p++;
                string name = s.Substring(start, p - start).ToLowerInvariant();
                SkipWs(s, ref p);

                switch (name)
                {
                    case "x": return x;
                    case "pi": return Math.PI;
                    case "e": return Math.E;
                }

                if (p < s.Length && s[p] == '(')
                {
                    p++;
                    double arg = ParseExpr(s, ref p, x);
                    SkipWs(s, ref p);
                    if (p >= s.Length || s[p] != ')') throw new FormatException($"Ожидалась ')' после {name}(...).");
                    p++;
                    return ApplyFunc(name, arg);
                }

                throw new FormatException($"Неизвестное имя '{name}'.");
            }

            throw new FormatException($"Неожиданный символ '{s[p]}'.");
        }

        private static double ApplyFunc(string name, double a)
        {
            switch (name)
            {
                case "sin": return Math.Sin(a);
                case "cos": return Math.Cos(a);
                case "tan": return Math.Tan(a);
                case "exp": return Math.Exp(a);
                case "ln": return Math.Log(a);
                case "log": return Math.Log10(a);
                case "sqrt": return Math.Sqrt(a);
                case "abs": return Math.Abs(a);
                default: throw new FormatException($"Неизвестная функция '{name}'.");
            }
        }
    }
}
