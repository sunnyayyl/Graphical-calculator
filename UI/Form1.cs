using Calculator;
using System.Diagnostics;

namespace UI;

public struct LineEquation(IExpression expression, Color color)
{
    public IExpression Expression { get; set; } = expression;
    public Color Color { get; set; } = color;
}

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void ReParse()
    {
        var stopwatch = new Stopwatch();
        this.graphControl1.ClearExpressions();
        try
        {
            stopwatch.Start();
            foreach (var equation in this.equationListView.Equations)
            {
                var l = new Lexer(equation.Equation);
                var p = new Parser(l);
                this.graphControl1.AddExpression(p.Parse(), equation.Color);
            }

            stopwatch.Stop();
            Debug.WriteLine("Successfully parsed expression in " + stopwatch.ElapsedMilliseconds + "ms");
        }
        catch (Exception ex) when (ex is IErrorMessage)
        {
            Debug.WriteLine(ex.Message);
            this.graphControl1.MarkError();
        }

        this.graphControl1.Refresh();
    }


    private void viewport_reset_Click(object sender, EventArgs e)
    {
        this.graphControl1.ResetViewport();
        this.graphControl1.Refresh();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        this.equationListView.GraphRefresh = this.ReParse;
    }
}