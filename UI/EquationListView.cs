namespace UI;

public partial class EquationListView : UserControl
{
    public Action GraphRefresh;

    public List<EquationTextBox> Equations
    {
        get { return this.tableLayoutPanel1.Controls.Cast<EquationTextBox>().ToList(); }
    }

    public EquationListView(Action graphRefresh)
    {
        InitializeComponent();
        this.AddEquationTextBox();
        this.GraphRefresh = graphRefresh;
    }

    void AddEquationTextBox()
    {
        var e = new EquationTextBox(this.GraphRefresh, et => this.tableLayoutPanel1.Controls.Remove(et));
        this.tableLayoutPanel1.Controls.Add(e);
    }

    public EquationListView()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    private void addNew_Click(object sender, EventArgs e)
    {
        this.AddEquationTextBox();
    }
}