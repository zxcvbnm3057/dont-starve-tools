using System.Windows.Forms;

namespace TEXTool
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }
        public void ReportProgress(int i)
        {
            progressBar.Value = i;
        }
    }
}
