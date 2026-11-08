namespace NewtonMethodApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem plotMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calcMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stepMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetStepMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearMenuItem;

        private System.Windows.Forms.TextBox txtA;
        private System.Windows.Forms.TextBox txtB;
        private System.Windows.Forms.TextBox txtE;
        private System.Windows.Forms.TextBox txtX0;
        private System.Windows.Forms.TextBox txtF;
        private System.Windows.Forms.Label lblA, lblB, lblE, lblX0, lblF, lblTarget;
        private System.Windows.Forms.ComboBox cmbTarget;

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
            this.plotMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calcMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stepMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetStepMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.lblA = new System.Windows.Forms.Label();
            this.lblB = new System.Windows.Forms.Label();
            this.lblE = new System.Windows.Forms.Label();
            this.lblX0 = new System.Windows.Forms.Label();
            this.lblF = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();

            this.txtA = new System.Windows.Forms.TextBox();
            this.txtB = new System.Windows.Forms.TextBox();
            this.txtE = new System.Windows.Forms.TextBox();
            this.txtX0 = new System.Windows.Forms.TextBox();
            this.txtF = new System.Windows.Forms.TextBox();
            this.cmbTarget = new System.Windows.Forms.ComboBox();

            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.rtbLog = new System.Windows.Forms.RichTextBox();

            // menuStrip1 — все действия только здесь
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.plotMenuItem, this.calcMenuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.stepMenuItem, this.resetStepMenuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.clearMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(955, 24);

            this.plotMenuItem.Name = "plotMenuItem";
            this.plotMenuItem.Text = "Построить график";
            this.plotMenuItem.Click += new System.EventHandler(this.plotMenuItem_Click);

            this.calcMenuItem.Name = "calcMenuItem";
            this.calcMenuItem.Text = "Рассчитать";
            this.calcMenuItem.Click += new System.EventHandler(this.calcMenuItem_Click);

            this.stepMenuItem.Name = "stepMenuItem";
            this.stepMenuItem.Text = "Следующий шаг";
            this.stepMenuItem.Enabled = false;
            this.stepMenuItem.Click += new System.EventHandler(this.stepMenuItem_Click);

            this.resetStepMenuItem.Name = "resetStepMenuItem";
            this.resetStepMenuItem.Text = "Сбросить шаги";
            this.resetStepMenuItem.Enabled = false;
            this.resetStepMenuItem.Click += new System.EventHandler(this.resetStepMenuItem_Click);

            this.clearMenuItem.Name = "clearMenuItem";
            this.clearMenuItem.Text = "Очистить";
            this.clearMenuItem.Click += new System.EventHandler(this.clearMenuItem_Click);

            // Labels
            this.lblA.Text = "a:"; this.lblA.Location = new System.Drawing.Point(12, 40); this.lblA.AutoSize = true;
            this.lblB.Text = "b:"; this.lblB.Location = new System.Drawing.Point(180, 40); this.lblB.AutoSize = true;
            this.lblE.Text = "e:"; this.lblE.Location = new System.Drawing.Point(350, 40); this.lblE.AutoSize = true;
            this.lblX0.Text = "x0:"; this.lblX0.Location = new System.Drawing.Point(12, 75); this.lblX0.AutoSize = true;
            this.lblF.Text = "f(x):"; this.lblF.Location = new System.Drawing.Point(12, 110); this.lblF.AutoSize = true;
            this.lblTarget.Text = "Искать:"; this.lblTarget.Location = new System.Drawing.Point(200, 75); this.lblTarget.AutoSize = true;

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

            // Валидация ввода
            this.txtA.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            this.txtB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            this.txtX0.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            this.txtE.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPositiveNumber_KeyPress);
            this.txtA.TextChanged += new System.EventHandler(this.txtNumber_TextChanged);
            this.txtB.TextChanged += new System.EventHandler(this.txtNumber_TextChanged);
            this.txtX0.TextChanged += new System.EventHandler(this.txtNumber_TextChanged);
            this.txtE.TextChanged += new System.EventHandler(this.txtPositiveNumber_TextChanged);
            this.txtF.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFormula_KeyPress);

            // ComboBox — тип искомого экстремума
            this.cmbTarget.Location = new System.Drawing.Point(260, 72);
            this.cmbTarget.Size = new System.Drawing.Size(180, 21);
            this.cmbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTarget.Items.AddRange(new object[] {
                "Любой экстремум",
                "Только минимум",
                "Только максимум"});
            this.cmbTarget.SelectedIndex = 0;

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
            this.Controls.Add(this.lblTarget); this.Controls.Add(this.cmbTarget);
            this.Controls.Add(this.lblF); this.Controls.Add(this.txtF);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.rtbLog);
            this.MainMenuStrip = this.menuStrip1;
            this.Text = "Метод Ньютона — поиск экстремумов";
            this.Name = "Form1";
        }
    }
}