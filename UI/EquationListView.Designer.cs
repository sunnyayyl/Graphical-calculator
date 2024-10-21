using System.ComponentModel;

namespace UI;

partial class EquationListView
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        addNew = new Button();
        tableLayoutPanel2 = new TableLayoutPanel();
        tableLayoutPanel1 = new TableLayoutPanel();
        tableLayoutPanel2.SuspendLayout();
        SuspendLayout();
        // 
        // addNew
        // 
        addNew.Dock = DockStyle.Fill;
        addNew.Location = new Point(3, 124);
        addNew.Name = "addNew";
        addNew.Size = new Size(206, 23);
        addNew.TabIndex = 0;
        addNew.Text = "+";
        addNew.UseVisualStyleBackColor = true;
        addNew.Click += addNew_Click;
        // 
        // tableLayoutPanel2
        // 
        tableLayoutPanel2.AutoScroll = true;
        tableLayoutPanel2.ColumnCount = 1;
        tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel2.Controls.Add(addNew, 0, 1);
        tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 0, 0);
        tableLayoutPanel2.Dock = DockStyle.Fill;
        tableLayoutPanel2.Location = new Point(0, 0);
        tableLayoutPanel2.Name = "tableLayoutPanel2";
        tableLayoutPanel2.RowCount = 2;
        tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
        tableLayoutPanel2.Size = new Size(212, 150);
        tableLayoutPanel2.TabIndex = 1;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.ColumnCount = 1;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new Point(3, 3);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        tableLayoutPanel1.Size = new Size(206, 115);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // EquationListView
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(tableLayoutPanel2);
        Name = "EquationListView";
        Size = new Size(212, 150);
        tableLayoutPanel2.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
    private Button addNew;
    private TableLayoutPanel tableLayoutPanel2;
    private TableLayoutPanel tableLayoutPanel1;
}