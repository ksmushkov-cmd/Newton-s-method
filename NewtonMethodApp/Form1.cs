using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace NewtonMethodApp
{
    public partial class Form1 : Form
    {
        // Состояние расчёта
        private class NewtonStep
        {
            public double X;
            public double F;
            public double D1;
            public double D2;
        }

        private List<NewtonStep> _steps = new List<NewtonStep>();
        private int _stepIndex = 0;

        // Параметры последнего расчёта (для отрисовки)
        private double _a, _b, _e;
        private Func<double, double> _f;
        private double _extremum = double.NaN;
        private string _extremumType = "";

        public Form1()
        {
            InitializeComponent();
        }

        // Парсинг выражения
        // Вычисляет f(x) по строке-формуле.
        // Используется DataTable.Compute — поддерживает + - * / ^, sin, cos, exp, ln и т.д.
        private Func<double, double> BuildFunction(string formula)
        {
            // Проверка на допустимость формулы — пробный расчёт
            double test = Evaluate(formula, 1.0); // бросит исключение при ошибке
            if (double.IsNaN(test) || double.IsInfinity(test))
                throw new FormatException("Формула возвращает недопустимое значение.");

            return x => Evaluate(formula, x);
        }

        private double Evaluate(string formula, double x)
        {
            string expr = formula;

            expr = expr.Replace(" ", "");
            expr = expr.Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture));
            expr = expr.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture)); 

            return ExpressionEvaluator.Eval(expr.Replace("pi", "(" + Math.PI.ToString(CultureInfo.InvariantCulture) + ")"), x);
        }

        // Численные производные
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

        // Метод Ньютона для экстремумов
        // Решает f'(x) = 0.
        // Тип точки (min/max) определяется по знаку f''(x).
        private List<NewtonStep> NewtonExtremum(Func<double, double> f, double x0, double e, int maxIter = 1000)
        {
            var steps = new List<NewtonStep>();
            double x = x0;

            for (int i = 0; i < maxIter; i++)
            {
                double d1 = D1(f, x);
                double d2 = D2(f, x);

                double fx = f(x);
                steps.Add(new NewtonStep { X = x, F = fx, D1 = d1, D2 = d2 });

                if (Math.Abs(d2) < 1e-10)
                    throw new InvalidOperationException(
                        $"Вторая производная близка к нулю в x = {x:F6}. Метод Ньютона неприменим (седловая точка или точка перегиба).");

                double xNext = x - d1 / d2;

                if (double.IsNaN(xNext) || double.IsInfinity(xNext))
                    throw new InvalidOperationException("Расхождение метода Ньютона (получено недопустимое значение).");

                if (Math.Abs(xNext - x) < e)
                {
                    // Финальная точка
                    double d1F = D1(f, xNext);
                    double d2F = D2(f, xNext);
                    steps.Add(new NewtonStep { X = xNext, F = f(xNext), D1 = d1F, D2 = d2F });
                    return steps;
                }

                x = xNext;

                // Защита от выхода за пределы [a,b]
                if (x < _a - 1e3 * (_b - _a) || x > _b + 1e3 * (_b - _a))
                    throw new InvalidOperationException("Точка вышла далеко за пределы интервала — метод расходится. Попробуйте другое x0.");
            }

            throw new InvalidOperationException("Метод не сошёлся за " + maxIter + " итераций.");
        }

        // Валидация ввода
        private bool TryParseInputs(out double a, out double b, out double e, out double x0, out Func<double, double> f)
        {
            a = b = e = x0 = 0;
            f = null;

            if (!double.TryParse(txtA.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out a) &&
                !double.TryParse(txtA.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out a))
            { ShowError("Параметр 'a' — некорректное вещественное число."); return false; }

            if (!double.TryParse(txtB.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out b) &&
                !double.TryParse(txtB.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out b))
            { ShowError("Параметр 'b' — некорректное вещественное число."); return false; }

            if (a >= b) { ShowError("Должно выполняться a < b."); return false; }

            if (!double.TryParse(txtE.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out e) &&
                !double.TryParse(txtE.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out e))
            { ShowError("Точность 'e' — некорректное вещественное число."); return false; }

            if (e <= 0) { ShowError("Точность 'e' должна быть > 0."); return false; }

            if (!double.TryParse(txtX0.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x0) &&
                !double.TryParse(txtX0.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out x0))
            { ShowError("Начальное приближение 'x0' — некорректное вещественное число."); return false; }

            if (x0 < a || x0 > b) { ShowError("Начальное приближение x0 должно лежать в [a, b]."); return false; }

            if (string.IsNullOrWhiteSpace(txtF.Text)) { ShowError("Формула f(x) не задана."); return false; }

            try
            {
                f = BuildFunction(txtF.Text);
                // Проверка, что функция определена на концах
                double fa = f(a), fb = f(b);
                if (double.IsNaN(fa) || double.IsInfinity(fa) || double.IsNaN(fb) || double.IsInfinity(fb))
                    throw new FormatException();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка в формуле f(x): " + ex.Message);
                return false;
            }

            return true;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Некорректный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Обработчики
        private void btnCalculate_Click(object sender, EventArgs e) => Calculate();

        private void calcMenuItem_Click(object sender, EventArgs e) => Calculate();

        private void Calculate()
        {
            if (!TryParseInputs(out double a, out double b, out double e, out double x0, out Func<double, double> f))
                return;

            _a = a; _b = b; _e = e; _f = f;
            _extremum = double.NaN;
            _extremumType = "";
            _steps.Clear();
            _stepIndex = 0;
            rtbLog.Clear();

            try
            {
                _steps = NewtonExtremum(f, x0, e);

                var last = _steps[_steps.Count - 1];
                double d2 = D2(f, last.X);

                if (d2 > 1e-8) _extremumType = "ЛОКАЛЬНЫЙ МИНИМУМ";
                else if (d2 < -1e-8) _extremumType = "ЛОКАЛЬНЫЙ МАКСИМУМ";
                else _extremumType = "не определено (f''≈0)";

                _extremum = last.X;

                rtbLog.AppendText($"Метод Ньютона для f'(x)=0\n");
                rtbLog.AppendText($"f(x) = {txtF.Text}\n");
                rtbLog.AppendText($"Интервал [{a}; {b}], e = {e}, x0 = {x0}\n");
                rtbLog.AppendText(new string('-', 60) + "\n");
                rtbLog.AppendText($"Шагов: {_steps.Count - 1}\n");
                rtbLog.AppendText($"Точка: x* = {last.X:F8}\n");
                rtbLog.AppendText($"f(x*) = {last.F:F8}\n");
                rtbLog.AppendText($"f'(x*) = {last.D1:E4}\n");
                rtbLog.AppendText($"f''(x*) = {last.D2:F6}\n");
                rtbLog.AppendText($"Тип экстремума: {_extremumType}\n");

                // Первый шаг для визуализации
                _stepIndex = 0;
                UpdateStepLog();

                stepMenuItem.Enabled = _steps.Count > 1;
                resetStepMenuItem.Enabled = _steps.Count > 1;
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка расчёта: " + ex.Message);
            }
        }

        private void UpdateStepLog()
        {
            rtbLog.AppendText(new string('-', 60) + "\n");
            rtbLog.AppendText($"Итерации (показана шаги 0..{_stepIndex}):\n");
            for (int i = 0; i <= _stepIndex && i < _steps.Count; i++)
            {
                var s = _steps[i];
                rtbLog.AppendText(
                    $"[{i}] x = {s.X:F6}, f = {s.F:F6}, f' = {s.D1:E3}, f'' = {s.D2:F4}\n");
            }
        }

        private void stepMenuItem_Click(object sender, EventArgs e)
        {
            if (_steps.Count == 0) return;
            if (_stepIndex < _steps.Count - 1)
            {
                _stepIndex++;
                UpdateStepLog();
                pictureBox1.Invalidate();
            }
            if (_stepIndex >= _steps.Count - 1)
                stepMenuItem.Enabled = false;
        }

        private void resetStepMenuItem_Click(object sender, EventArgs e)
        {
            if (_steps.Count == 0) return;
            _stepIndex = 0;
            rtbLog.Clear();
            var last = _steps[_steps.Count - 1];
            rtbLog.AppendText($"Перезапуск пошагового просмотра.\nТочка: x* = {last.X:F8}, тип: {_extremumType}\n");
            UpdateStepLog();
            stepMenuItem.Enabled = _steps.Count > 1;
            pictureBox1.Invalidate();
        }

        private void btnClear_Click(object sender, EventArgs e) => ClearAll();
        private void clearMenuItem_Click(object sender, EventArgs e) => ClearAll();

        private void ClearAll()
        {
            _steps.Clear();
            _stepIndex = 0;
            _extremum = double.NaN;
            _extremumType = "";
            _f = null;
            rtbLog.Clear();
            pictureBox1.Invalidate();
            stepMenuItem.Enabled = false;
            resetStepMenuItem.Enabled = false;
        }

        // Отрисовка графика
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_f == null) return;

            // Собираем значения f по сетке
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

            // Учитываем точку экстремума
            if (!double.IsNaN(_extremum))
            {
                double yE = _f(_extremum);
                if (yE < ymin) ymin = yE;
                if (yE > ymax) ymax = yE;
            }

            if (ymin == ymax) { ymin -= 1; ymax += 1; }
            double ypad = (ymax - ymin) * 0.1;
            ymin -= ypad; ymax += ypad;

            // Преобразование координат
            Rectangle r = pictureBox1.ClientRectangle;
            int padL = 40, padR = 20, padT = 20, padB = 30;
            double W = r.Width - padL - padR;
            double H = r.Height - padT - padB;

            Func<double, float> toPX = x => (float)(padL + (x - xmin) / (xmax - xmin) * W);
            Func<double, float> toPY = y => (float)(padT + (ymax - y) / (ymax - ymin) * H);


            using (var axisPen = new Pen(Color.Gray, 1))
            {
                g.DrawLine(axisPen, padL, r.Height - padB, r.Width - padR, r.Height - padB); // X
                g.DrawLine(axisPen, padL, padT, padL, r.Height - padB);                       // Y
            }

            // Нулевые линии
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

            if (_steps.Count > 0 && _stepIndex < _steps.Count)
            {
                var cur = _steps[_stepIndex];
                var p = new PointF(toPX(cur.X), toPY(cur.F));
                using (var brush = new SolidBrush(Color.Orange))
                using (var pen = new Pen(Color.DarkOrange, 2))
                {
                    g.FillEllipse(brush, p.X - 6, p.Y - 6, 12, 12);
                    g.DrawEllipse(pen, p.X - 6, p.Y - 6, 12, 12);
                }
                g.DrawString($"x={cur.X:F4}", new Font("Segoe UI", 8), Brushes.DarkOrange, p.X + 8, p.Y - 18);
            }

            if (!double.IsNaN(_extremum))
            {
                double yE = _f(_extremum);
                var p = new PointF(toPX(_extremum), toPY(yE));
                Color c = _extremumType.Contains("МИНИМУМ") ? Color.Green : Color.Red;
                using (var brush = new SolidBrush(c))
                using (var pen = new Pen(c, 2))
                {
                    g.FillEllipse(brush, p.X - 7, p.Y - 7, 14, 14);
                    g.DrawEllipse(pen, p.X - 9, p.Y - 9, 18, 18);
                }
                string label = $"{_extremumType}: ({_extremum:F4}; {yE:F4})";
                g.DrawString(label, new Font("Segoe UI", 8, FontStyle.Bold), Brushes.Black, p.X + 10, p.Y - 22);
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
