using Calculator;
using System.Diagnostics;

namespace UI;

public partial class Form1 : Form
{
    private int? _mouseOldX, _mouseOldY;
    private float _xScale = 1.0f;
    private float _yScale = 1.0f;
    private float _xOffset = 0.0f;
    private float _yOffset = 0.0f;
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
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        this.ReParse();
        this.pictureBox1.Refresh();
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
            for (var x = -halfWidth; x <= width - halfWidth; x += 0.1f)
            {
                input['x'] = x;
                float yRaw;
                try
                {
                    yRaw = (float)expression.Eval(input);
                }
                catch (Exception ex) when (ex is IErrorMessage)
                {
                    this._hasError = true;
                    Console.WriteLine(ex.Message);
                    break;
                }

                var plotX = (x + halfWidth) * this._xScale;
                var plotY = (-yRaw + halfHeight) * this._yScale;
                if (Math.Abs(plotX) > Width || Math.Abs(plotY) > Height)
                {
                    last = null;
                    continue;
                }

                var current = new PointF(plotX, plotY);
                if (last.HasValue)
                {
                    e.Graphics.DrawLine(pen, last.Value, current);
                }

                last = current;
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

        //var maxY = Math.Clamp(halfHeight * this._yScale, 0, height);
        e.Graphics.DrawLine(new Pen(Color.Black), new PointF(0, halfHeight * this._yScale),
            new PointF(width, halfHeight * this._yScale));
        e.Graphics.DrawLine(new Pen(Color.Black), new PointF(halfWidth * this._xScale, 0),
            new PointF(halfWidth * this._xScale, height));
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
                this._xOffset +=
                    ((float)e.X - _mouseOldX.Value) * (1 / this._xScale); // Change sensitivity based on scale
                this._yOffset += ((float)e.Y - _mouseOldY.Value) * (1 / this._xScale);
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

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        this._xScale =
            Math.Clamp(this._xScale + e.Delta * 0.001f, float.Epsilon,
                float.PositiveInfinity); // Prevent scale from going to 0 and below
        this._yScale = Math.Clamp(this._yScale + e.Delta * 0.001f, float.Epsilon, float.PositiveInfinity);
        Debug.WriteLine($"Scale: {this._xScale}, {this._yScale}");
        var mouseX = (float)e.X;
        this.pictureBox1.Refresh();
        base.OnMouseWheel(e);
    }
}