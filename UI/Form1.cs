using Calculator;
using System.Diagnostics;

namespace UI;

public partial class Form1 : Form
{
    private int? _mouseOldX, _mouseOldY = null;
    float _xScale, _yScale = 1.0f;
    private float _xOffset, _yOffset = 0.0f;
    private IExpression? _expression;
    private bool _hasError;

    public Form1()
    {
        InitializeComponent();
    }

    private void ReParse()
    {
        var stopwatch = new Stopwatch();
        this._hasError = false;
        this._expression = null;
        try
        {
            stopwatch.Start();
            var l = new Lexer(textBox1.Text.Trim());
            var p = new Parser(l);
            this._expression = p.Parse();
            stopwatch.Stop();
            Debug.WriteLine("Successfully parsed expression in " + stopwatch.ElapsedMilliseconds + "ms");
        }
        catch (Exception ex) when (ex is IErrorMessage)
        {
            Debug.WriteLine(ex.Message);
            this._hasError = true;
        }

        this.pictureBox1.Refresh();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        this.ReParse();
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e)
    {
        PointF? last = null;
        var size = this.pictureBox1.Size;
        var width = size.Width;
        var height = size.Height;
        var halfWidth = (float)width / 2 + this._xOffset;
        var halfHeight = (float)height / 2 + this._yOffset;
        var pen = new Pen(Color.Red);
        var input = new Dictionary<char, double>
        {
            ['x'] = 0
        };
        if (this._expression is { } expression)
        {
            for (float x = -halfWidth; x <= width - halfWidth; x += 0.1f)
            {
                input['x'] = (double)x;
                float yRaw;
                float plotX;
                float plotY;
                try
                {
                    yRaw = (float)expression.Eval(input);
                    plotX = x + halfWidth;
                    plotY = -yRaw + halfHeight;
                }
                catch (OverflowException ex)
                {
                    Debug.WriteLine("Warning: Overflow exception occured while calculating value to plot");
                    last = null;
                    continue;
                }

                try
                {
                    var current = new PointF(plotX, plotY);
                    if (last.HasValue)
                    {
                        e.Graphics.DrawLine(pen, last.Value, current);
                    }

                    last = current;
                }
                catch (OverflowException ex)
                {
                    Debug.WriteLine("Fatal: Overflow exception occured while plotting");
                    last = null;
                    break;
                }
            }
        }

        if (this._hasError)
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
        this.pictureBox1.Refresh();
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
            if (_mouseOldX.HasValue && _mouseOldY.HasValue)
            {
                this._xOffset += (float)e.X - _mouseOldX.Value;
                this._yOffset += (float)e.Y - _mouseOldY.Value;
            }

            _mouseOldX = e.X;
            _mouseOldY = e.Y;
            this.pictureBox1.Refresh();
        }
        else
        {
            _mouseOldX = null;
            _mouseOldY = null;
        }
    }
}