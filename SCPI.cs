using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using NationalInstruments.VisaNS;

namespace Nevis14
{
    class SCPI
    {
        // For more information on the SCPI the function generator uses,
        // see the manual at https://www.testequipmentdepot.com/keithley/pdfs/3390_man.pdf
        private MessageBasedSession scpitalker;
        public string SCPIhostName = "192.168.1.1";
        public const double ampMax = 5.0;

        public SCPI()
        {
            scpitalker = new MessageBasedSession("TCPIP::169.254.2.20");
        }

        public SCPI(MessageBasedSession talker)
        {
            scpitalker = talker;
        }

        public bool ApplySin(double freq, double amp, double offset)
        {
            return !WriteToSCPI("APPL:SIN " + freq + ", " + amp + ", " + offset).Contains("ERROR");
        }

        public bool ClearStatus()
        {
            return !WriteToSCPI("*CLS").Contains("ERROR");
        }

        public bool Output()
        {
            string s = WriteToSCPI("OUTP?");
            return s.Contains("1");
        }

        public bool OutputOn()
        {
            return !WriteToSCPI("OUTP ON").Contains("ERROR");
        }

        public bool OutputOff()
        {
            return !WriteToSCPI("OUTP OFF").Contains("ERROR");
        }

        public bool SetFreq(double signalfreq)
        {
            return !WriteToSCPI("FREQ " + signalfreq).Contains("ERROR");
        }

        public bool SetShape(string shape)
        //  Sine = "SIN", Square = "SQU, Ramp = "RAMP", Pulse = "PULS", Noise = "NOISE"
        {
            return !WriteToSCPI("FUNC " + shape).Contains("ERROR");
        }

        public bool SetVolt(double voltage)
        {
            return !WriteToSCPI("VOLT " + voltage).Contains("ERROR");
        }

        public string ReadFromSCPI()
        {
            string responseData = scpitalker.ReadString();
            Console.WriteLine(responseData);
            return responseData;
        }

        public string WriteToSCPI(string s)
        {
            //Byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
            try
            {
                scpitalker.Write(s);
                Byte[] lf = { (Byte)'\n' };
                scpitalker.Write(lf);

                if (s.IndexOf("?") >= 0)
                    return ReadFromSCPI();

                return "";
            }
            catch
            {
                MessageBox.Show("Error Writing to generator: " + s);
                return "ERROR";
            }
        }
    }
}
