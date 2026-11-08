using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace NewtonMethodApp
{
    public partial class Form1 : Form
    {
        private double _a = -5, _b = 5, _e = 0.001;
        private Func<double, double> _f = null;

        private bool _plotReady = false;

        private bool _hasExtremum = false;
        private double _extremumX = double.NaN;
        private string _extremumType = "";

        public Form1()
        {
            InitializeComponent();
        }

        private bool TryParseCommon(out double a, out double b, out double e, out double x0, out Func<double, double> f)
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

        private Func<double, double> BuildFunction(string formula)
        {
            double test = ExpressionEvaluator.Eval(formula, 1.0); // проверка синтаксиса
            if (double.IsNaN(test) || double.IsInfinity(test))
                throw new FormatException("Формула возвращает недопустимое значение.");
            return x => ExpressionEvaluator.Eval(formula, x);
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Некорректный ввод", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ============ ЧИСЛЕННЫЕ ПРОИЗВОДНЫЕ ============
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

        // Решает f'(x) = 0 через x_{n+1} = x_n - f'(x_n)/f''(x_n)
        private double NewtonExtremum(Func<double, double> f, double x0, double e, out string type)
        {
            type = "";
            double x = x0;
            const int maxIter = 1000;

            for (int i = 0; i < maxIter; i++)
            {
                double d1 = D1(f, x);
                double d2 = D2(f, x);

                if (Math.Abs(d2) < 1e-10)
                    throw new InvalidOperationException(
                        $"f''(x) ≈ 0 в x = {x:F6}. Метод Ньютона неприменим (точка перегиба/седловая).");

                double xNext = x - d1 / d2;

                if (double.IsNaN(xNext) || double.IsInfinity(xNext))
                    throw new InvalidOperationException("Расхождение метода Ньютона (недопустимое значение x).");

                if (Math.Abs(xNext - x) < e)
                {
                    double d2F = D2(f, xNext);
                    if (d2F > 1e-8) type = "ЛОКАЛЬНЫЙ МИНИМУМ";
                    else if (d2F < -1e-8) type = "ЛОКАЛЬНЫЙ МАКСИМУМ";
                    else type = "не определено (f'' ≈ 0)";
                    return xNext;
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

            // Сброс предыдущего результата
            _plotReady = true;
            _hasExtremum = false;
            _extremumX = double.NaN;
            _extremumType = "";

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
            // Если график ещё не построен — сначала строим его
            if (!_plotReady || _f == null)
            {
                Plot();
                if (!_plotReady) return; 
            }

            // Повторно валидируем x0 (a,b,e,f уже взяты при построении)
            double x0;
            if (!double.TryParse(txtX0.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x0) &&
                !double.TryParse(txtX0.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out x0))
            { ShowError("Начальное приближение 'x0' — некорректное вещественное число."); return; }

            if (x0 < _a || x0 > _b)
            { ShowError("Начальное приближение x0 должно лежать в [a, b]."); return; }

            try
            {
                double xStar = NewtonExtremum(_f, x0, _e, out string type);
                double yStar = _f(xStar);
                double d1 = D1(_f, xStar);
                double d2 = D2(_f, xStar);

                _hasExtremum = true;
                _extremumX = xStar;
                _extremumType = type;

                rtbLog.Clear();
                rtbLog.AppendText("=== Результат метода Ньютона ===\n");
                rtbLog.AppendText($"f(x) = {txtF.Text}\n");
                rtbLog.AppendText($"Интервал: [{_a}; {_b}], e = {_e}, x0 = {x0}\n");
                rtbLog.AppendText(new string('-', 45) + "\n");
                rtbLog.AppendText($"x* = {xStar:F8}\n");
                rtbLog.AppendText($"f(x*) = {yStar:F8}\n");
                rtbLog.AppendText($"f'(x*) = {d1:E4}\n");
                rtbLog.AppendText($"f''(x*) = {d2:F6}\n");
                rtbLog.AppendText($"Тип экстремума: {type}\n");

                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка расчёта: " + ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e) => ClearAll();
        private void clearMenuItem_Click(object sender, EventArgs e) => ClearAll();

        private void ClearAll()
        {
            _f = null;
            _plotReady = false;
            _hasExtremum = false;
            _extremumX = double.NaN;
            _extremumType = "";
            rtbLog.Clear();
            pictureBox1.Invalidate();
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

            // Точка экстремума — только если рассчитана
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
