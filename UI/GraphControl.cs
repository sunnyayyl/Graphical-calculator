using System.Diagnostics;
using Calculator;
using System.ComponentModel;

namespace UI
{
    public partial class GraphControl : UserControl
    {
        private List<IExpression> _expressions = [];
        private bool _hasError;
        private int? _mouseOldX, _mouseOldY;
        private float _xScale = 1.0f;
        private float _yScale = 1.0f;
        private float _xOffset = 0.0f;
        private float _yOffset = 0.0f;

        public GraphControl()
        {
            InitializeComponent();
            this.EnableDoubleBuffering();
        }

        private void EnableDoubleBuffering()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            var size = this.Size;
            var width = size.Width;
            var height = size.Height;
            var halfWidth = (float)width / 2 + this._xOffset;
            var halfHeight = (float)height / 2 + this._yOffset;
            pe.Graphics.DrawLine(new Pen(Color.Black), new PointF(0, halfHeight * this._yScale),
                new PointF(width, halfHeight * this._yScale));
            pe.Graphics.DrawLine(new Pen(Color.Black), new PointF(halfWidth * this._xScale, 0),
                new PointF(halfWidth * this._xScale, height));
            if (this._hasError)
            {
                this.BackColor = Color.Pink;
            }
            else
            {
                this.BackColor = Color.White;
                var pen = new Pen(Color.Red);
                var input = new Dictionary<char, double>
                {
                    ['x'] = 0
                };
                PointF? last = null;
                foreach (var expression in this._expressions)
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
                            pe.Graphics.DrawLine(pen, last.Value, current);
                        }

                        last = current;
                    }
                }
            }

            base.OnPaint(pe);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this._xScale =
                Math.Clamp(this._xScale + e.Delta * 0.001f, float.Epsilon,
                    float.PositiveInfinity); // Prevent scale from going to 0 and below
            this._yScale = Math.Clamp(this._yScale + e.Delta * 0.001f, float.Epsilon, float.PositiveInfinity);
            Debug.WriteLine($"Scale: {this._xScale}, {this._yScale}");
            var mouseX = (float)e.X;
            this.Refresh();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
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
                this.Refresh();
            }
            else
            {
                _mouseOldX = null;
                _mouseOldY = null;
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            this.Refresh();
            base.OnResize(e);
        }

        [Browsable(true)]
        public void ClearExpressions()
        {
            this._hasError = false;
            this._expressions.Clear();
        }

        [Browsable(true)]
        public void AddExpression(IExpression expression)
        {
            this._expressions.Add(expression);
        }

        public void MarkError()
        {
            this._hasError = true;
        }
    }
}