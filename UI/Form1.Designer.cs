namespace UI;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tableLayoutPanel1 = new TableLayoutPanel();
        graphControl1 = new GraphControl();
        tableLayoutPanel2 = new TableLayoutPanel();
        viewport_reset = new Button();
        equationListView = new EquationListView();
        tableLayoutPanel1.SuspendLayout();
        tableLayoutPanel2.SuspendLayout();
        SuspendLayout();
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.ColumnCount = 2;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.125F));
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 83.875F));
        tableLayoutPanel1.Controls.Add(graphControl1, 1, 0);
        tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableLayoutPanel1.Size = new Size(800, 450);
        tableLayoutPanel1.TabIndex = 2;
        // 
        // graphControl1
        // 
        graphControl1.BackColor = Color.White;
        graphControl1.Dock = DockStyle.Fill;
        graphControl1.Location = new Point(132, 3);
        graphControl1.Name = "graphControl1";
        graphControl1.Size = new Size(665, 444);
        graphControl1.TabIndex = 1;
        // 
        // tableLayoutPanel2
        // 
        tableLayoutPanel2.ColumnCount = 1;
        tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        tableLayoutPanel2.Controls.Add(viewport_reset, 0, 1);
        tableLayoutPanel2.Controls.Add(equationListView, 0, 0);
        tableLayoutPanel2.Dock = DockStyle.Fill;
        tableLayoutPanel2.Location = new Point(3, 3);
        tableLayoutPanel2.Name = "tableLayoutPanel2";
        tableLayoutPanel2.RowCount = 2;
        tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
        tableLayoutPanel2.Size = new Size(123, 444);
        tableLayoutPanel2.TabIndex = 3;
        // 
        // viewport_reset
        // 
        viewport_reset.Dock = DockStyle.Top;
        viewport_reset.Location = new Point(3, 418);
        viewport_reset.Name = "viewport_reset";
        viewport_reset.Size = new Size(117, 23);
        viewport_reset.TabIndex = 1;
        viewport_reset.Text = "Reset viewport";
        viewport_reset.UseVisualStyleBackColor = true;
        viewport_reset.Click += viewport_reset_Click;
        // 
        // equationListView
        // 
        equationListView.Dock = DockStyle.Fill;
        equationListView.Location = new Point(3, 3);
        equationListView.Name = "equationListView";
        equationListView.Size = new Size(117, 409);
        equationListView.TabIndex = 2;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(tableLayoutPanel1);
        Name = "Form1";
        Text = "Form1";
        Load += Form1_Load;
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel2.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
    private TableLayoutPanel tableLayoutPanel1;
    private GraphControl graphControl1;
    private TableLayoutPanel tableLayoutPanel2;
    private Button viewport_reset;
    private EquationListView equationListView;
}