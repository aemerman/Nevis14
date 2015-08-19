using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nevis14
{
    public partial class VoltageDialog : Form
    {
        Form1 mainform;

        public VoltageDialog(Form1 main)
        {
            InitializeComponent();
            mainform = main;
        }

        private void okbutton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            double ampstart = Convert.ToDouble(startTextBox.Text);
            double ampstop = Convert.ToDouble(endTextBox.Text);
            double ampstep = Convert.ToDouble(stepTextBox.Text);
            mainform.RunOnBkgWorker((obj, args) =>
            {
                mainform.VoltageRangeTest(ampstart, ampstop, ampstep);
                args.Result = true;
            });
            this.Close();
        }

        private void cancelbutton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
