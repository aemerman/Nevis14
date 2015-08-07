using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.VisaNS;

namespace Nevis14 {
    public partial class FunctionGenerator : UserControl {
        public FunctionGenerator () {
            InitializeComponent();
        }
        public FunctionGenerator (string IP) {
            InitializeComponent();
            signalIP = IP;
            if (SCPIconnect()) isConnected = true;
        }

        private const double ampMax = 5.0;
        private bool isOn = false;
        public bool isConnected = false;
        private MessageBasedSession scpiTalker;
        public double signalFreq;
        public double signalAmp;
        public string signalIP = "169.254.2.20"; //"192.168.1.1";
        public void Reset () {
            this.OutputOff();
            scpiTalker = null;
            isConnected = false;
        }
        private void onButton_Click (object sender, EventArgs e) {
            if (this.isOn) this.OutputOff();
            else this.OutputOn();
        }
        private void ValidationCompleted (object sender, System.Windows.Forms.TypeValidationEventArgs e) {
            if (e.IsValidInput) {
                (sender as MaskedTextBox).BackColor = default(Color);
                if (sender.Equals(this.freqBox)) signalFreq = (double)e.ReturnValue;
                else if (sender.Equals(this.ampBox)) signalAmp = (double)e.ReturnValue;
                else throw new Exception("Validated unknown text box.");
            } else {
                (sender as MaskedTextBox).BackColor = Color.Red;
                e.Cancel = true;
            }
        } // End ValidationCompleted

        public bool SCPIconnect () {
            if (scpiTalker == null) {
                try {
                    scpiTalker = new MessageBasedSession("TCPIP::" + signalIP);
                } catch (System.ArgumentException) {
                    if (Global.AskError("Check your signal generator connection settings") == DialogResult.Retry)
                        SCPIconnect();
                }
            }
            return scpiTalker != null;
        } // End SCPIconnect
        private string WriteToSCPI (string s) {
            try {
                scpiTalker.Write(s);
                Byte[] lf = { (Byte)'\n' };
                scpiTalker.Write(lf);
            } catch (Exception e) {
                throw new ScpiException("SCPI Write Error", e);
            }

            if (s.IndexOf("?") >= 0) return ReadFromSCPI();
            else return "";
        } // End WriteToSCPI
        private string ReadFromSCPI () {
            string responseData;
            try {
                responseData = scpiTalker.ReadString();
            } catch (Exception e) {
                throw new ScpiException("SCPI Read Error", e);
            }
            Console.WriteLine(responseData);
            return responseData;
        } // End ReadFromSCPI
        public void OutputOn () {
            WriteToSCPI("OUTP ON");
            this.onButton.Text = "On";
            this.onButton.BackColor = Global.OnColor;
        }
        public void OutputOff () {
            WriteToSCPI("OUTP OFF");
            this.onButton.Text = "Off";
            this.onButton.BackColor = Global.OffColor;
        }
        public bool CheckOutput () {
            return (WriteToSCPI("OUTP?")).Contains("1"); ;
        }
        public void ApplySin (double freq, double amp, double offset) {
            WriteToSCPI("APPL:SIN " + freq + ", " + amp + ", " + offset);
        }
        public void ApplySin (){
            WriteToSCPI("APPL:SIN " + signalFreq + ", " + signalAmp + ", 0");
        }
        public void ClearStatus () {
            WriteToSCPI("*CLS");
        }
        private class ScpiException : Exception {
            public ScpiException () { }
            public ScpiException (string message) : base(message) { }
            public ScpiException (string message, Exception inner) : base(message, inner) { }
        }
    }
}
