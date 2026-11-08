using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace NewtonMethodApp
{
    public partial class Form1 : Form
    {
        // Точка на траектории метода Ньютона
        private class Step
        {
            public double X;
            public double F;
            public double D1;
            public double D2;
        }

        private double _a = -5, _b = 5, _e = 0.001;
        private Func<double, double> _f = null;
        private bool _plotReady = false;

        // Результат расчёта
        private bool _hasExtremum = false;
        private double _extremumX = double.NaN;
        private string _extremumType = "";

        // Пошаговый просмотр
        private List<Step> _steps = new List<Step>();
        private int _stepIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            char c = e.KeyChar;

            if (char.IsControl(c)) return; // Backspace, Delete и т.п.

            char sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

            // Минус — только первым символом и только если его ещё нет
            if (c == '-' && tb.SelectionStart == 0 && !tb.Text.Contains("-"))
                return;

            // Разделитель дробной части — только один
            if ((c == '.' || c == ',') && !tb.Text.Contains(".") && !tb.Text.Contains(","))
                return;

            if (char.IsDigit(c)) return;

            e.Handled = true;
        }

        private void txtPositiveNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            char c = e.KeyChar;
            if (char.IsControl(c)) return;
            if ((c == '.' || c == ',') && !tb.Text.Contains(".") && !tb.Text.Contains(","))
                return;
            if (char.IsDigit(c)) return;
            e.Handled = true;
        }

        private void txtFormula_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (char.IsControl(c)) return;
            const string allowed = "0123456789+-*/^()., xXabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            if (allowed.IndexOf(c) >= 0) return;
            e.Handled = true;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string s = tb.Text;

            string cleaned = FilterNumber(s);
            if (cleaned != s)
            {
                int pos = tb.SelectionStart;
                tb.Text = cleaned;
                tb.SelectionStart = Math.Min(pos, cleaned.Length);
            }
        }

        private void txtPositiveNumber_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string s = tb.Text;
            string cleaned = FilterNumber(s).Replace("-", "");
            if (cleaned != s)
            {
                int pos = tb.SelectionStart;
                tb.Text = cleaned;
                tb.SelectionStart = Math.Min(pos, cleaned.Length);
            }
        }

        private string FilterNumber(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new System.Text.StringBuilder();
            bool hasMinus = false, hasDot = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '-')
                {
                    if (i == 0 && !hasMinus) { sb.Append(c); hasMinus = true; }
                }
                else if (c == '.' || c == ',')
                {
                    if (!hasDot) { sb.Append(c); hasDot = true; }
                }
                else if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private bool TryParseCommon(out double a, out double b, out double e, out double x0, out Func<double, double> f)
        {
            a = b = e = x0 = 0;
            f = null;

            if (!TryParseDouble(txtA.Text, out a))
            { ShowError("Параметр 'a' — некорректное вещественное число."); return false; }

            if (!TryParseDouble(txtB.Text, out b))
            { ShowError("Параметр 'b' — некорректное вещественное число."); return false; }

            if (a >= b) { ShowError("Должно выполняться a < b."); return false; }

            if (!TryParseDouble(txtE.Text, out e))
            { ShowError("Точность 'e' — некорректное вещественное число."); return false; }

            if (e <= 0) { ShowError("Точность 'e' должна быть > 0."); return false; }

            if (!TryParseDouble(txtX0.Text, out x0))
            { ShowError("Начальное приближение 'x0' — некорректное вещественное число."); return false; }

            if (x0 < a || x0 > b) { ShowError("Начальное приближение x0 должно лежать в [a, b]."); return false; }

            if (string.IsNullOrWhiteSpace(txtF.Text)) { ShowError("Формула f(x) не задана."); return false; }

            try
            {
                f = BuildFunction(txtF.Text);
                double fa = f(a), fb = f(b);
                if (double.IsNaN(fa) || double.IsInfinity(fa) || double.IsNaN(fb) || double.IsInfinity(fb))
                    throw new FormatException("Функция не определена на концах интервала [a,b].");
            }
            catch (Exception ex)
            {
                ShowError("Ошибка в формуле f(x): " + ex.Message);
                return false;
            }

            return true;
        }

        private bool TryParseDouble(string s, out double v)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)
                || double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out v);
        }

        private Func<double, double> BuildFunction(string formula)
        {
            double test = ExpressionEvaluator.Eval(formula, 1.0);
            if (double.IsNaN(test) || double.IsInfinity(test))
                throw new FormatException("Формула возвращает недопустимое значение.");
            return x => ExpressionEvaluator.Eval(formula, x);
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Некорректный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private double D1(Func<double, double> f, double x)
        {
            double h = 1e-5;
            return (f(x + h) - f(x - h)) / (2 * h);
        }

        private double D2(Func<double, double> f, double x)
        {
            double h = 1e-4;
            return (f(x + h) - 2 * f(x) + f(x - h)) / (h * h);
        }

        private List<Step> NewtonExtremum(Func<double, double> f, double x0, double e, out string type)
        {
            type = "";
            var steps = new List<Step>();
            double x = x0;
            const int maxIter = 1000;

            for (int i = 0; i < maxIter; i++)
            {
                double d1 = D1(f, x);
                double d2 = D2(f, x);

                steps.Add(new Step { X = x, F = f(x), D1 = d1, D2 = d2 });

                if (Math.Abs(d2) < 1e-10)
                    throw new InvalidOperationException(
                        $"f''(x) ≈ 0 в x = {x:F6}. Метод Ньютона неприменим (точка перегиба/седловая).");

                double xNext = x - d1 / d2;

                if (double.IsNaN(xNext) || double.IsInfinity(xNext))
                    throw new InvalidOperationException("Расхождение метода Ньютона.");

                if (Math.Abs(xNext - x) < e)
                {
                    double d2F = D2(f, xNext);
                    steps.Add(new Step { X = xNext, F = f(xNext), D1 = D1(f, xNext), D2 = d2F });

                    if (d2F > 1e-8) type = "ЛОКАЛЬНЫЙ МИНИМУМ";
                    else if (d2F < -1e-8) type = "ЛОКАЛЬНЫЙ МАКСИМУМ";
                    else type = "не определено (f'' ≈ 0)";

                    return steps;
                }

                x = xNext;

                if (x < _a - 1e3 * (_b - _a) || x > _b + 1e3 * (_b - _a))
                    throw new InvalidOperationException(
                        "Метод расходится — точка вышла далеко за [a,b]. Попробуйте другое x0.");
            }

            throw new InvalidOperationException("Метод не сошёлся за " + maxIter + " итераций.");
        }

        private void btnPlot_Click(object sender, EventArgs e) => Plot();
        private void plotMenuItem_Click(object sender, EventArgs e) => Plot();

        private void Plot()
        {
            if (!TryParseCommon(out double a, out double b, out double e, out double x0, out Func<double, double> f))
                return;

            _a = a; _b = b; _e = e;
            _f = f;
            _plotReady = true;

            // Сбрасываем результат предыдущего расчёта
            ResetResult();

            rtbLog.Clear();
            rtbLog.AppendText("=== График построен ===\n");
            rtbLog.AppendText($"f(x) = {txtF.Text}\n");
            rtbLog.AppendText($"Интервал: [{a}; {b}]\n");
            rtbLog.AppendText("Нажмите «Рассчитать», чтобы найти экстремум.\n");

            pictureBox1.Invalidate();
        }

        private void btnCalculate_Click(object sender, EventArgs e) => Calculate();
        private void calcMenuItem_Click(object sender, EventArgs e) => Calculate();

        private void Calculate()
        {
            if (!_plotReady || _f == null)
            {
                ShowError("Сначала постройте график (кнопка «Построить график»).");
                return;
            }

            if (!TryParseDouble(txtX0.Text, out double x0))
            { ShowError("Начальное приближение 'x0' — некорректное вещественное число."); return; }

            if (x0 < _a || x0 > _b)
            { ShowError("Начальное приближение x0 должно лежать в [a, b]."); return; }

            string target = cmbTarget.SelectedIndex == 1 ? "min"
                          : cmbTarget.SelectedIndex == 2 ? "max"
                          : "any";

            try
            {
                _steps = NewtonExtremum(_f, x0, _e, out string type);

                if (target == "min" && !type.Contains("МИНИМУМ"))
                {
                    ResetResult();
                    pictureBox1.Invalidate();
                    ShowError("Найден не минимум, а " + type +
                        ". Измените x0 или выберите «Любой экстремум».");
                    return;
                }
                if (target == "max" && !type.Contains("МАКСИМУМ"))
                {
                    ResetResult();
                    pictureBox1.Invalidate();
                    ShowError("Найден не максимум, а " + type +
                        ". Измените x0 или выберите «Любой экстремум».");
                    return;
                }

                var last = _steps[_steps.Count - 1];

                _hasExtremum = true;
                _extremumX = last.X;
                _extremumType = type;
                _stepIndex = 0;

                rtbLog.Clear();
                rtbLog.AppendText("=== Результат метода Ньютона ===\n");
                rtbLog.AppendText($"f(x) = {txtF.Text}\n");
                rtbLog.AppendText($"Интервал: [{_a}; {_b}], e = {_e}, x0 = {x0}\n");
                rtbLog.AppendText($"Итераций: {_steps.Count - 1}\n");
                rtbLog.AppendText(new string('-', 45) + "\n");
                rtbLog.AppendText($"x* = {last.X:F8}\n");
                rtbLog.AppendText($"f(x*) = {last.F:F8}\n");
                rtbLog.AppendText($"f'(x*) = {last.D1:E4}\n");
                rtbLog.AppendText($"f''(x*) = {last.D2:F6}\n");
                rtbLog.AppendText($"Тип: {type}\n");
                rtbLog.AppendText(new string('-', 45) + "\n");
                rtbLog.AppendText("Пошаговый просмотр: меню «Следующий шаг».\n");

                stepMenuItem.Enabled = _steps.Count > 1;
                resetStepMenuItem.Enabled = _steps.Count > 1;

                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка расчёта: " + ex.Message);
            }
        }

        private void stepMenuItem_Click(object sender, EventArgs e)
        {
            if (_steps.Count == 0) return;
            if (_stepIndex < _steps.Count - 1)
            {
                _stepIndex++;
                var s = _steps[_stepIndex];
                rtbLog.AppendText(
                    $"[{_stepIndex}] x={s.X:F6}  f={s.F:F6}  f'={s.D1:E3}  f''={s.D2:F4}\n");
                pictureBox1.Invalidate();
            }
            if (_stepIndex >= _steps.Count - 1)
                stepMenuItem.Enabled = false;
        }

        private void resetStepMenuItem_Click(object sender, EventArgs e)
        {
            if (_steps.Count == 0) return;
            _stepIndex = 0;
            var s = _steps[0];
            rtbLog.AppendText(
                $"[0] x={s.X:F6}  f={s.F:F6}  f'={s.D1:E3}  f''={s.D2:F4}\n");
            stepMenuItem.Enabled = _steps.Count > 1;
            pictureBox1.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e) => ClearAll();
        private void clearMenuItem_Click(object sender, EventArgs e) => ClearAll();

        private void ClearAll()
        {
            _f = null;
            _plotReady = false;
            ResetResult();
            rtbLog.Clear();
            pictureBox1.Invalidate();
        }

        private void ResetResult()
        {
            _hasExtremum = false;
            _extremumX = double.NaN;
            _extremumType = "";
            _steps.Clear();
            _stepIndex = 0;
            stepMenuItem.Enabled = false;
            resetStepMenuItem.Enabled = false;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (!_plotReady || _f == null) return;

            int N = 800;
            double xmin = _a, xmax = _b;
            double[] xs = new double[N];
            double[] ys = new double[N];
            double ymin = double.MaxValue, ymax = double.MinValue;

            for (int i = 0; i < N; i++)
            {
                double x = xmin + (xmax - xmin) * i / (N - 1);
                double y = _f(x);
                xs[i] = x;
                ys[i] = y;
                if (!double.IsNaN(y) && !double.IsInfinity(y))
                {
                    if (y < ymin) ymin = y;
                    if (y > ymax) ymax = y;
                }
            }

            if (_hasExtremum && !double.IsNaN(_extremumX))
            {
                double yE = _f(_extremumX);
                if (!double.IsNaN(yE) && !double.IsInfinity(yE))
                {
                    if (yE < ymin) ymin = yE;
                    if (yE > ymax) ymax = yE;
                }
            }

            if (ymin == ymax) { ymin -= 1; ymax += 1; }
            double ypad = (ymax - ymin) * 0.1;
            ymin -= ypad; ymax += ypad;

            Rectangle r = pictureBox1.ClientRectangle;
            int padL = 40, padR = 20, padT = 20, padB = 30;
            double W = r.Width - padL - padR;
            double H = r.Height - padT - padB;

            Func<double, float> toPX = x => (float)(padL + (x - xmin) / (xmax - xmin) * W);
            Func<double, float> toPY = y => (float)(padT + (ymax - y) / (ymax - ymin) * H);

            using (var axisPen = new Pen(Color.Gray, 1))
            {
                g.DrawLine(axisPen, padL, r.Height - padB, r.Width - padR, r.Height - padB);
                g.DrawLine(axisPen, padL, padT, padL, r.Height - padB);
            }

            if (ymin < 0 && ymax > 0)
            {
                float y0 = toPY(0);
                using (var zeroPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    g.DrawLine(zeroPen, padL, y0, r.Width - padR, y0);
            }
            if (xmin < 0 && xmax > 0)
            {
                float x0 = toPX(0);
                using (var zeroPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    g.DrawLine(zeroPen, x0, padT, x0, r.Height - padB);
            }

            using (var curvePen = new Pen(Color.SteelBlue, 2))
            {
                PointF? prev = null;
                for (int i = 0; i < N; i++)
                {
                    double y = ys[i];
                    if (double.IsNaN(y) || double.IsInfinity(y) || y < ymin - 1e5 || y > ymax + 1e5)
                    { prev = null; continue; }
                    var p = new PointF(toPX(xs[i]), toPY(y));
                    if (prev.HasValue) g.DrawLine(curvePen, prev.Value, p);
                    prev = p;
                }
            }

            if (_steps.Count > 0)
            {
                using (var trailPen = new Pen(Color.Orange, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                {
                    for (int i = 0; i < _stepIndex; i++)
                    {
                        var a1 = _steps[i];
                        var a2 = _steps[i + 1];
                        g.DrawLine(trailPen, toPX(a1.X), toPY(a1.F), toPX(a2.X), toPY(a2.F));
                    }
                }

                for (int i = 0; i <= _stepIndex && i < _steps.Count; i++)
                {
                    var s = _steps[i];
                    float px = toPX(s.X), py = toPY(s.F);
                    using (var b = new SolidBrush(Color.Orange))
                        g.FillEllipse(b, px - 4, py - 4, 8, 8);
                }

                var cur = _steps[Math.Min(_stepIndex, _steps.Count - 1)];
                float cx = toPX(cur.X), cy = toPY(cur.F);
                using (var b = new SolidBrush(Color.DarkOrange))
                using (var p = new Pen(Color.DarkOrange, 2))
                {
                    g.FillEllipse(b, cx - 6, cy - 6, 12, 12);
                    g.DrawEllipse(p, cx - 8, cy - 8, 16, 16);
                }
                g.DrawString($"x={cur.X:F4}", new Font("Segoe UI", 8), Brushes.DarkOrange, cx + 8, cy - 18);
            }

            if (_hasExtremum && !double.IsNaN(_extremumX))
            {
                double yE = _f(_extremumX);
                if (!double.IsNaN(yE) && !double.IsInfinity(yE))
                {
                    var p = new PointF(toPX(_extremumX), toPY(yE));
                    Color c = _extremumType.Contains("МИНИМУМ") ? Color.Green : Color.Red;
                    using (var brush = new SolidBrush(c))
                    using (var pen = new Pen(c, 2))
                    {
                        g.FillEllipse(brush, p.X - 7, p.Y - 7, 14, 14);
                        g.DrawEllipse(pen, p.X - 9, p.Y - 9, 18, 18);
                    }
                    string label = $"{_extremumType}: ({_extremumX:F4}; {yE:F4})";
                    g.DrawString(label, new Font("Segoe UI", 8, FontStyle.Bold), Brushes.Black, p.X + 10, p.Y - 22);
                }
            }

            using (var fnt = new Font("Segoe UI", 8))
            {
                g.DrawString(_a.ToString("F2"), fnt, Brushes.Black, padL - 10, r.Height - padB + 5);
                g.DrawString(_b.ToString("F2"), fnt, Brushes.Black, r.Width - padR - 20, r.Height - padB + 5);
                g.DrawString(ymax.ToString("F2"), fnt, Brushes.Black, 2, padT);
                g.DrawString(ymin.ToString("F2"), fnt, Brushes.Black, 2, r.Height - padB - 12);
            }
        }
    }
}
