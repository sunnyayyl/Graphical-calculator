using Calculator;
using System.Diagnostics;

namespace UI;

public partial class Form1 : Form
{
    int? mouseOldX = null, mouseOldY = null;
    float xScale, YScale = 0.0f;
    float xOffset, yOffset = 0.0f;
    IExpression? expression;
    bool hasError;
    List<PointF> points = [];

    public Form1()
    {
        InitializeComponent();
    }

    void reParse()
    {
        this.hasError = false;
        this.expression = null;
        try
        {
            var size = pictureBox1.Size;
            var width = size.Width;
            var height = size.Height;
            var l = new Lexer(textBox1.Text.Trim());
            var p = new Parser(l);
            this.expression = p.Parse();
            var result = p.Parse();
        }
        catch (Exception ex) when (ex is IErrorMessage)
        {
            Debug.WriteLine(ex.Message);
            this.hasError = true;
        }

        this.pictureBox1.Refresh();
    }

    void textBox1_TextChanged(object sender, EventArgs e)
    {
        this.reParse();
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e)
    {
        PointF? last = null;
        var size = pictureBox1.Size;
        var width = size.Width;
        var height = size.Height;
        float halfWidth = width / 2 + xOffset;
        float halfHeight = height / 2 + yOffset;
        var xScale = 20;
        var yScale = 20;
        try
        {
            if (this.expression is IExpression expression)
            {
                for (double x = -halfWidth; x < halfWidth; x += 0.1)
                {
                    var input = new Dictionary<char, double>
                    {
                        ['x'] = x
                    };
                    var yRaw = expression.Eval(input);
                    float plotX;
                    float plotY;
                    try
                    {
                        plotX = (float)((x * xScale + halfWidth));
                        plotY = (float)((-yRaw * yScale + halfHeight));
                        var current = new PointF(plotX, plotY);
                        if (last.HasValue)
                        {
                            e.Graphics.DrawLine(new Pen(Color.Red), last.Value, current);
                        }

                        last = current;
                    }
                    catch (OverflowException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        last = null;
                        continue;
                    }
                }
            }
        }
        catch (Exception ex) when (ex is IErrorMessage)
        {
            Debug.WriteLine(ex.Message);
            hasError = true;
        }

        if (hasError)
        {
            this.pictureBox1.BackColor = Color.Pink;
        }
        else
        {
            this.pictureBox1.BackColor = Color.White;
        }

        e.Graphics.DrawLine(new Pen(Color.Black), new PointF(0, halfHeight), new PointF(width, halfHeight));
        e.Graphics.DrawLine(new Pen(Color.Black), new PointF(halfWidth, 0), new PointF(halfWidth, height));
    }

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void Form1_ResizeEnd(object sender, EventArgs e)
    {
        this.reParse();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0112)
        {
            this.pictureBox1.Invalidate();
        }

        base.WndProc(ref m);
    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        if (Control.MouseButtons == MouseButtons.Left)
        {
            if (mouseOldX.HasValue && mouseOldY.HasValue)
            {
                this.xOffset += (float)e.X - mouseOldX.Value;
                this.yOffset += (float)e.Y - mouseOldY.Value;
            }

            mouseOldX = e.X;
            mouseOldY = e.Y;
            this.reParse();
        }
        else
        {
            mouseOldX = null;
            mouseOldY = null;
        }
    }
}