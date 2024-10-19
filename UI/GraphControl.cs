using System.Diagnostics;
using Calculator;
using System.ComponentModel;

namespace UI
{
    public partial class GraphControl : UserControl
    {
        public float RenderSteps = 0.01f;
        public float ScrollSensitivity = 0.01f;
        private List<IExpression> _expressions = [];
        private bool _hasError;
        private int? _mouseOldX, _mouseOldY;
        private float _xScale = 1.0f;
        private float _yScale = 1.0f;
        private float _xOffset = 0.0f;
        private float _yOffset = 0.0f;
        private bool _renderError;

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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var outOfBoundsCount = 0;
            this._renderError = false;
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
                this._renderError = true;
                Debug.WriteLine("Overflow exception occured while drawing axis");
            }

            if (this._hasError || this._renderError)
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
                    for (var x = -halfWidth; x <= width - halfWidth; x += RenderSteps)
                    {
                        var plotX = (x + halfWidth) * this._xScale;
                        if (float.IsInfinity(plotX) || float.IsNaN(plotX) || Math.Abs(plotX) > Width)
                        {
                            last = null;
                            outOfBoundsCount++;
                            continue;
                        }

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

                        var plotY = (-yRaw + halfHeight) * this._yScale;
                        if (Math.Abs(plotY) > Height || float.IsNaN(plotY) || float.IsInfinity(plotY))
                        {
                            last = null;
                            outOfBoundsCount++;
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

            stopwatch.Stop();
            Debug.WriteLine($"Calculated {width / this.RenderSteps} points in {stopwatch.ElapsedMilliseconds}ms");
            Debug.WriteLineIf(outOfBoundsCount > 0, $"Skipped {outOfBoundsCount} out of bound points");
            base.OnPaint(pe);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // var mouseX = e.Location.X / this._xScale - ((this.Width / 2f) + this._xOffset);
            // var mouseY = -(e.Location.Y / this._yScale - ((this.Height / 2f) + this._yOffset));
            this.Zoom(e.Delta * ScrollSensitivity, e.Delta * ScrollSensitivity, e.Location.X, e.Location.Y);
            this.Refresh();
            base.OnMouseWheel(e);
        }

        public void Zoom(float xFactor, float yFactor, float midX, float midY)
        {
            var newXScale = this._xScale + xFactor;
            var newYScale = this._yScale + yFactor;
            if (newXScale < float.Epsilon || newYScale < float.Epsilon)
            {
                return;
            }

            this._xOffset -= midX / this._xScale;
            this._yOffset -= midY / this._yScale;
            this._xScale = newXScale;
            this._yScale = newYScale;
            this._xOffset += midX / this._xScale;
            this._yOffset += midY / this._yScale;
            this.ClampValues();
        }

        private void ClampValues()
        {
            this._xOffset = Math.Clamp(this._xOffset, float.MinValue, float.MaxValue);
            this._yOffset = Math.Clamp(this._yOffset, float.MinValue, float.MaxValue);
            this._xScale =
                Math.Clamp(this._xScale, float.Epsilon,
                    float.PositiveInfinity); // Prevent division by zero and negative scale
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

        protected override void OnLoad(EventArgs e)
        {
            this.ResetViewport();
            this.Invalidate();
            base.OnLoad(e);
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

        public (float x, float y) GetScale()
        {
            return (this._xScale, this._yScale);
        }

        public (float x, float y) GetOffset()
        {
            return (this._xOffset, this._yOffset);
        }

        public void SetScale(float x, float y)
        {
            this._xScale = x;
            this._yScale = y;
        }

        public void SetOffset(float x, float y)
        {
            this._xOffset = x;
            this._yOffset = y;
        }

        public void ResetViewport()
        {
            this.SetOffset(0f, 0f);
            this.SetScale(1f, 1f);
            var avg = (this.Width + this.Height) / 2f / 10f;
            this.Zoom(avg, avg, this.Width / 2f, this.Height / 2f);
        }
    }
}