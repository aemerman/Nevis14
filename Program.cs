using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nevis14 {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main () {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public static partial class Global {
        // Display the error, then keep running once the user hits Okay
        public static bool ShowError (string message) {
            MessageBox.Show(message, "Oops", MessageBoxButtons.OK);
            return false;
        }
        // Display the error and ask if the user wants to keep going
        public static DialogResult AskError (string message) {
            DialogResult answer = MessageBox.Show(message, "", MessageBoxButtons.AbortRetryIgnore);
            if (answer == DialogResult.Abort) {
                Application.Exit(); // Restart instead??
                return DialogResult.None;
            } else return answer;
        }
        // Display the error, then close the program
        public static void FatalError (string message) {
            MessageBox.Show(message, "Fatal Error");
            Application.Exit();
        }
        // Convert input a to a number with base b
        // If a width is provided it will pad the string
        public static string NumberToString (uint a, int b, int width = 1) {
            string s = Convert.ToString(a, b).PadLeft(width, '0');
            switch (b) {
                case 2:
                    return s+"b";
                case 16:
                    return s;
                default:
                    return s;
            }
        }
        //TODO? StringToNumber
        public static uint StringToNumber (string s) {
            s = s.Trim();
            return 0;
        }

        public static System.Drawing.Color OnColor = System.Drawing.Color.Lime;
        public static System.Drawing.Color OffColor = default(System.Drawing.Color);
    }

    public delegate void FileWriteDelegate(string path, IEnumerable<string> contents);

    public static class MyExtensionMethods {

        // Perform the action, invoking it on the control's owner thread if necessary
        public static void SafeInvokeAsync (this System.ComponentModel.ISynchronizeInvoke control,
            Action action) {
            if (control.InvokeRequired) {
                control.BeginInvoke(action, null);
            } else {
                action();
            }
        }
        public static void SafeInvoke (this System.ComponentModel.ISynchronizeInvoke control,
            Action action) {
            if (control.InvokeRequired) {
                // Wait for action to return
                control.Invoke(action, null);
            } else {
                action();
            }
        }
        // Call SafeInvoke, then update the GUI
        public static void Update (this Control control, Action action, bool synchronous = false) {
            if (synchronous) {
                control.SafeInvoke(() => { action(); control.Update(); });
            } else {
                control.SafeInvokeAsync(() => { action(); control.Update(); });
            }
        }

        public static void AppendLines (this TextBox box, List<string> lines) {
            foreach (string s in lines) {
                box.AppendText(s + Environment.NewLine);
            }
        }

        public static List<string> To16BitBinary (this List<byte> buffer) {
            if ((buffer.Count % 8) != 0) throw new Exception("Buffer size is " + buffer.Count + ", not a multiple of 8.");
            List<string> lines = new List<string>();
            StringBuilder s = new StringBuilder(72);

            for (int i = 0; i < buffer.Count; i += 8) {
                s.Clear();
                for (int j = 0; j < 8; j++) {
                    // Show the binary numbers
                    s.Append(Convert.ToString(buffer[i + j], 2).PadLeft(8, '0') + " ");
                }
                lines.Add(s.ToString());
            }
            return lines;
        }
        public static List<string> ToDecimal (this List<byte> buffer) {
            if ((buffer.Count % 8) != 0) throw new Exception("Buffer size is " + buffer.Count + ", not a multiple of 8.");
            List<string> lines = new List<string>();
            StringBuilder s = new StringBuilder(20);

            for (int i = 0; i < buffer.Count; i += 8) {
                s.Clear();
                for (int j = 0; j < 8; j += 2) {
                    s.Append(Convert.ToString(((buffer[i + j] << 8) + buffer[i + j + 1]), 10).PadLeft(4, '0') + " ");
                }
                lines.Add(s.ToString());
            }
            return lines;
        }
        public static string ToSeparatedString (this List<string> list) {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                s.Append(list[i]);
                s.Append(Environment.NewLine);
            }
            return s.ToString();
        }
    } // End ExtensionMethods
}