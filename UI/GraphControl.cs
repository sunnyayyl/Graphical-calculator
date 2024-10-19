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
                        var toleranceX = width * 0.2;
                        var toleranceY = height * 0.2;
                        if (Math.Abs(plotX) >= Width + toleranceX || Math.Abs(plotY) >= Height + toleranceY)
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
            var mouseX = e.Location.X / this._xScale - ((this.Width / 2f) + this._xOffset);
            var mouseY = -(e.Location.Y / this._yScale - ((this.Height / 2f) + this._yOffset));
            this._xOffset -= e.Location.X / this._xScale;
            this._yOffset -= e.Location.Y / this._yScale;
            this._xScale += e.Delta * ScrollSensitivity;
            this._yScale += e.Delta * ScrollSensitivity;
            this._xOffset += e.Location.X / this._xScale;
            this._yOffset += e.Location.Y / this._yScale;
            //this._xOffset += mouseX;
            //this._yOffset -= mouseY;
            Debug.WriteLine($"{mouseX}, {mouseY}");
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
    }
}