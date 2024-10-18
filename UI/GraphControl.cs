using System.Diagnostics;
using Calculator;
using System.ComponentModel;

namespace UI
{
    public partial class GraphControl : UserControl
    {
        public float ScrollSensitivity = 0.099f;
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
            var xMid = halfWidth * this._xScale;
            var yMid = halfHeight * this._yScale;
            try
            {
                pe.Graphics.DrawLine(new Pen(Color.Black), new PointF(0, yMid),
                    new PointF(width, yMid));
                pe.Graphics.DrawLine(new Pen(Color.Black), new PointF(xMid, 0),
                    new PointF(xMid, height));
            }
            catch (OverflowException exp)
            {
                this._hasError = true;
                Debug.WriteLine("Overflow exception occured while drawing axis");
            }

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
            this._xScale = this._xScale + e.Delta * this.ScrollSensitivity;
            this._yScale = this._yScale + e.Delta * this.ScrollSensitivity;
            Debug.WriteLine($"Scale: {this._xScale}, {this._yScale}");
            // this._xOffset += e.X / this._xScale - (this.Size.Width/2.0f+this._xOffset);
            // this._yOffset +=  -(e.Y/this._yScale- (this.Size.Height/2.0f+this._yOffset));
            this.ClampValues();
            this.Refresh();
            base.OnMouseWheel(e);
        }

        private void ClampValues()
        {
            this._xOffset = Math.Clamp(this._xOffset, float.MinValue, float.MaxValue);
            this._yOffset = Math.Clamp(this._yOffset, float.MinValue, float.MaxValue);
            this._xScale =
                Math.Clamp(this._xScale, float.Epsilon,
                    float.PositiveInfinity); // Prevent scale from going to 0 and below
            this._yScale = Math.Clamp(this._yScale, float.Epsilon, float.PositiveInfinity);
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

                this.ClampValues();
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

        public void ClearExpressions()
        {
            this._hasError = false;
            this._expressions.Clear();
        }

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