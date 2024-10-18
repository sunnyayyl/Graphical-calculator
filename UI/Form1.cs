using Calculator;
using System.Diagnostics;

namespace UI;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void ReParse()
    {
        var stopwatch = new Stopwatch();
        IExpression? expression = null;
        try
        {
            stopwatch.Start();
            var l = new Lexer(textBox1.Text.Trim());
            var p = new Parser(l);
            expression = p.Parse();
            stopwatch.Stop();
            Debug.WriteLine("Successfully parsed expression in " + stopwatch.ElapsedMilliseconds + "ms");
        }
        catch (Exception ex) when (ex is IErrorMessage)
        {
            Debug.WriteLine(ex.Message);
            this.graphControl1.MarkError();
        }

        if (expression != null)
        {
            this.graphControl1.ClearExpressions();
            this.graphControl1.AddExpression(expression);
        }

        this.graphControl1.Refresh();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        this.ReParse();
    }
}