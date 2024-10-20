using System.Diagnostics;

namespace UI;

public partial class EquationTextBox : UserControl
{
    public Action? GraphRefresh;
    public Action<EquationTextBox> Delete;
    public string Equation { get; set; }
    public bool Enable { get; set; }
    public Color Color { get; set; }

    public EquationTextBox(Action graphRefresh, Action<EquationTextBox> delete, string equation, bool enable,
        Color color)
    {
        InitializeComponent();
        this.GraphRefresh = graphRefresh;
        Equation = equation;
        Enable = enable;
        Color = color;
    }

    public EquationTextBox(Action graphRefresh, Action<EquationTextBox> delete)
    {
        InitializeComponent();
        this.GraphRefresh = graphRefresh;
        this.Delete = delete;
        this.Equation = "";
        this.Enable = true;
        this.Color = Color.Red;
    }

    public EquationTextBox()
    {
        InitializeComponent();
        this.Equation = "";
        this.Enable = true;
        this.Color = Color.Red;
        this.GraphRefresh = null;
    }

    private void button1_SizeChanged(object sender, EventArgs e)
    {
        this.button1.Font = new Font(this.button1.Font.FontFamily, this.button1.Width / 2f);
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        this.Equation = this.textBox1.Text;
        GraphRefresh?.Invoke();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        this.Delete(this);
        GraphRefresh?.Invoke();
    }
}