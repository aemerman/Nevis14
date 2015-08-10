using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nevis14
{
    public partial class ErrorLog : Form
    {
        string filePath;

        public ErrorLog(string path)
        {
            InitializeComponent();
            filePath = path;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string mainError = ErrorString();
            if (mainError == "")
            {
                MessageBox.Show("Please choose a problem or press \"cancel\" if there is none.");
                return;
            }

            string s = chipnumTextBox.Text + " - "
                + mainError + " - "
                + powerTextBox.Text + " - "
                + notesTextBox.Text + " - ";
            using (StreamWriter err = File.AppendText(filePath + "defects.txt")) err.WriteLine(s);
            MessageBox.Show("Defect recorded successfully");
            this.Close();
        }

        private string ErrorString()
        {
            string mainError = "";
            if (serialRadio.Checked)
                mainError = "failed to serialize";
            else if (nocalRadio.Checked)
                mainError = "failed to calibrate";
            else if (badcalRadio.Checked)
                mainError = "calibration error";
            else if (takedataRadio.Checked)
                mainError = "error taking data";
            else if (signalRadio.Checked)
                mainError = "issue with signal";
            else if (otherRadio.Checked)
                mainError = otherTextBox.Text;
            return mainError;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
