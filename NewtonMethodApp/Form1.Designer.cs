namespace NewtonMethodApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem calcMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stepMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetStepMenuItem;

        private System.Windows.Forms.TextBox txtA;
        private System.Windows.Forms.TextBox txtB;
        private System.Windows.Forms.TextBox txtE;
        private System.Windows.Forms.TextBox txtX0;
        private System.Windows.Forms.TextBox txtF;
        private System.Windows.Forms.Label lblA, lblB, lblE, lblX0, lblF;

        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Button btnClear;

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RichTextBox rtbLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.calcMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stepMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetStepMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.lblA = new System.Windows.Forms.Label();
            this.lblB = new System.Windows.Forms.Label();
            this.lblE = new System.Windows.Forms.Label();
            this.lblX0 = new System.Windows.Forms.Label();
            this.lblF = new System.Windows.Forms.Label();

            this.txtA = new System.Windows.Forms.TextBox();
            this.txtB = new System.Windows.Forms.TextBox();
            this.txtE = new System.Windows.Forms.TextBox();
            this.txtX0 = new System.Windows.Forms.TextBox();
            this.txtF = new System.Windows.Forms.TextBox();

            this.btnCalculate = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();

            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();

            // menuStrip1
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.calcMenuItem, this.clearMenuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.stepMenuItem, this.resetStepMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(950, 24);

            this.calcMenuItem.Name = "calcMenuItem";
            this.calcMenuItem.Text = "Рассчитать";
            this.calcMenuItem.Click += new System.EventHandler(this.calcMenuItem_Click);

            this.clearMenuItem.Name = "clearMenuItem";
            this.clearMenuItem.Text = "Очистить";
            this.clearMenuItem.Click += new System.EventHandler(this.clearMenuItem_Click);

            this.stepMenuItem.Name = "stepMenuItem";
            this.stepMenuItem.Text = "Следующий шаг";
            this.stepMenuItem.Enabled = false;
            this.stepMenuItem.Click += new System.EventHandler(this.stepMenuItem_Click);

            this.resetStepMenuItem.Name = "resetStepMenuItem";
            this.resetStepMenuItem.Text = "Сбросить шаги";
            this.resetStepMenuItem.Enabled = false;
            this.resetStepMenuItem.Click += new System.EventHandler(this.resetStepMenuItem_Click);

            // Labels
            this.lblA.Text = "a:"; this.lblA.Location = new System.Drawing.Point(12, 40); this.lblA.AutoSize = true;
            this.lblB.Text = "b:"; this.lblB.Location = new System.Drawing.Point(180, 40); this.lblB.AutoSize = true;
            this.lblE.Text = "e:"; this.lblE.Location = new System.Drawing.Point(350, 40); this.lblE.AutoSize = true;
            this.lblX0.Text = "x0:"; this.lblX0.Location = new System.Drawing.Point(12, 75); this.lblX0.AutoSize = true;
            this.lblF.Text = "f(x):"; this.lblF.Location = new System.Drawing.Point(12, 110); this.lblF.AutoSize = true;

            // TextBoxes
            this.txtA.Location = new System.Drawing.Point(40, 37); this.txtA.Size = new System.Drawing.Size(120, 20);
            this.txtB.Location = new System.Drawing.Point(205, 37); this.txtB.Size = new System.Drawing.Size(120, 20);
            this.txtE.Location = new System.Drawing.Point(375, 37); this.txtE.Size = new System.Drawing.Size(120, 20);
            this.txtX0.Location = new System.Drawing.Point(40, 72); this.txtX0.Size = new System.Drawing.Size(120, 20);
            this.txtF.Location = new System.Drawing.Point(40, 107); this.txtF.Size = new System.Drawing.Size(455, 20);

            this.txtA.Text = "-5";
            this.txtB.Text = "5";
            this.txtE.Text = "0.001";
            this.txtX0.Text = "3";
            this.txtF.Text = "x*x - 4*x + 3"; 

            // Buttons
            this.btnCalculate.Location = new System.Drawing.Point(510, 37);
            this.btnCalculate.Size = new System.Drawing.Size(110, 26);
            this.btnCalculate.Text = "Рассчитать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);

            this.btnClear.Location = new System.Drawing.Point(510, 69);
            this.btnClear.Size = new System.Drawing.Size(110, 26);
            this.btnClear.Text = "Очистить";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            // pictureBox1
            this.pictureBox1.Location = new System.Drawing.Point(12, 145);
            this.pictureBox1.Size = new System.Drawing.Size(610, 380);
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);

            // rtbLog
            this.rtbLog.Location = new System.Drawing.Point(630, 145);
            this.rtbLog.Size = new System.Drawing.Size(310, 380);
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 9F);

            // Form1
            this.ClientSize = new System.Drawing.Size(955, 540);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.lblA); this.Controls.Add(this.txtA);
            this.Controls.Add(this.lblB); this.Controls.Add(this.txtB);
            this.Controls.Add(this.lblE); this.Controls.Add(this.txtE);
            this.Controls.Add(this.lblX0); this.Controls.Add(this.txtX0);
            this.Controls.Add(this.lblF); this.Controls.Add(this.txtF);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.rtbLog);
            this.MainMenuStrip = this.menuStrip1;
            this.Text = "Метод Ньютона — поиск экстремумов";
            this.Name = "Form1";
        }
    }
}
