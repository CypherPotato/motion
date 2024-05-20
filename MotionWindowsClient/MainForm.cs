namespace MotionWindowsClient;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new Forms.CreateConnectionForm().ShowDialog(this);
    }
}
