using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nevis14 {
    public partial class ChipControl : UserControl {
        public ChipControl () {
            InitializeComponent();
            refControl = 0;
            pllControl = 16; //??
            slvsControl = 0;
        }

        public uint chipReset;
        public uint softwareReset;
        public uint refControl { get; set; }
        public uint pllControl { get; set; }
        public uint slvsControl { get; set; }

        public void Activate (uint channelNum) {
            for (int iCh = 0; iCh < 4; iCh++) {
                adcs[iCh].Deactivate ();
            }
            adcs[channelNum].Activate(ref mdacControls, ref dacButtons, ref oFlagButton, ref serializerButton);
        } // End Activate
        private void button_Click (object s, EventArgs e) {
            // If button is on, turn off. Else turn on.
            (s as Button).BackColor = ((s as Button).BackColor == Global.OnColor) ? Global.OffColor : Global.OnColor;
            OnValueChanged(s, e);
        } // End button_Click
        public void OnValueChanged (object s, EventArgs e) {
            for (int iCh = 0; iCh < 4; iCh++) {
                if (adcs[iCh].isActive) adcs[iCh].MatchAdcToGui();
            }
        } // End OnValueChanged
        public void ResetGuiValues () {
            serializerButton.BackColor = default(Color);
            oFlagButton.BackColor = default(Color);
            dacButtons[0].BackColor = default(Color);
            dacButtons[1].BackColor = default(Color);
            for (int i = 0; i < 4; i++) {
                mdacControls[i].cal1 = 0;
                mdacControls[i].cal2 = 0;
                mdacControls[i].disable = 1;
            }
        } // End ResetGuiValues
    }

    [Serializable]
    public partial class AdcControl : UserControl {
        public AdcControl (uint id) {
            _id = id;
            InitializeComponent();
            _dacs = null;
            mdacs = new Mdac[4] { new Mdac(1), new Mdac(2), new Mdac(3), new Mdac(4) };
            oFlag = 0;
            serializer = 0;
            dac1 = 0;
            dac2 = 0;
        }

        public void Activate (ref MdacControl[] mdacControls, ref Button[] dacButtons, ref Button oFlagButton, ref Button serializerButton) {
            if (mdacControls.Length != 4 || dacButtons.Length != 2) {
                throw new Exception("wrong number of components. mdacs: "
                    + mdacControls.Length + " dacs: " + dacButtons.Length);
            }

            // Set initial button color
            this.Update(() => this.adcButton.BackColor = Color.DimGray);
            _oFlagButton = oFlagButton;
            _serializerButton = serializerButton;
            _dacs = dacButtons;
            for (int i = 0; i < 4; i++) {
                mdacs[i].Activate(ref mdacControls[i]);
            }
            isActive = true;
            MatchGuiToAdc();
        } // End Activate
        private void adcButton_Click (object s, EventArgs e) {
            if (this.isActive) {
                this.Deactivate(); (this.Parent as ChipControl).ResetGuiValues();
            } else (this.Parent as ChipControl).Activate(_id - 1);
        } // End adcButton_Click
        public void Deactivate () {
            isActive = false; _dacs = null;
            for (int i = 0; i < 4; i++) { mdacs[i].Deactivate(); }
            ResetButtonColor();
        } // End Deactivate
        public void IsCalibrated (bool success) {
            this.Update(() => this.adcButton.BackColor = 
                success ? System.Drawing.Color.Green : System.Drawing.Color.Red, true);
        } // End IsCalibrated
         
        // The Update method for AdcControl contains a check
        // against isActive. If isActive is false it will not
        // carry out the action. This prevents the AdcControl
        // from modifying the Gui while another AdcControl is
        // using it.
        public bool isActive = false;
        public Mdac[] mdacs;
        private Button[] _dacs;
        private Button _oFlagButton;
        private Button _serializerButton;

        private uint _id;
        public uint dac1 {
            get {
                if (isActive) return (uint)((_dacs[0].BackColor == Global.OnColor) ? 1 : 0);
                else return _dac1;
            }
            set {
                if (isActive) {
                    _dacs[0].Update(() => _dacs[0].BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
                this._dac1 = value;
            }
        }
        public uint dac2 {
            get {
                if (isActive) return (uint)((_dacs[1].BackColor == Global.OnColor) ? 1 : 0);
                else return _dac2;
            }
            set {
                if (isActive) {
                    _dacs[1].Update(() => _dacs[1].BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
                this._dac2 = value;
            }
        }
        public uint oFlag {
            get {
                if (isActive) return (uint)((_oFlagButton.BackColor == Global.OnColor) ? 1 : 0);
                else return _oFlag;
            }
            set {
                if (isActive) {
                    _oFlagButton.Update(() => _oFlagButton.BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
                this._oFlag = value;
            }
        }
        public uint serializer {
            get {
                if (isActive) return (uint)((_serializerButton.BackColor == Global.OnColor) ? 1 : 0);
                else return _serializer;
            }
            set {
                if (isActive) {
                    _serializerButton.Update(() => _serializerButton.BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
                this._serializer = value;
            }
        }
        public uint cFlag = 0;
        public uint sarInput = 0;
        public uint dynamicRange = 0;

        private uint _dac1, _dac2, _oFlag, _serializer;
        public class Mdac {
            public Mdac (uint id) {
                _mdac = null;
                _id = id;
                correction0 = (uint) 1 << (11 - (int) _id); // = 2^(11-_id)
                correction1 = (uint) 1 << (12 - (int) _id);
                cal1 = 0;
                cal2 = 0;
                disable = 1;
            }
            public bool isActive; // Should always mirror the isActive property of the parent AdcControl
            private MdacControl _mdac;
            private uint _id;
            public void Activate (ref MdacControl mdac) {
                this._mdac = mdac;
                if (!this._mdac.CompareId(_id)) MessageBox.Show("mdac IDs don't match");
                isActive = true;
            }

            public void Deactivate () {
                this.isActive = false;
                this._mdac = null;
            } // End Deactivate

            private void onClick (object sender, uint value) {
                if (this.isActive) {

                } 
            }
            public void MatchMdacToGui () {
                cal1 = cal1;
                cal2 = cal2;
                disable = disable;
            } // End MatchMdacToGui
            public void MatchGuiToMdac () {
                cal1 = _cal1;
                cal2 = _cal2;
                disable = _disable;
            } // End MatchGuiToMdac
            public uint correction0;
            public uint correction1;
            public uint bias = 0;
            public uint cal1 {
                get {
                    if (isActive) return this._mdac.cal1;
                    else return this._cal1;
                }
                set { if (isActive) this._mdac.cal1 = value; this._cal1 = value; }
            }
            public uint cal2 {
                get {
                    if (isActive) return this._mdac.cal2;
                    else return this._cal2;
                }
                set { if (isActive) this._mdac.cal2 = value; this._cal2 = value; }
            }
            public uint disable {
                get {
                    if (isActive) return this._mdac.disable;
                    else return this._disable;
                }
                set { if (isActive) this._mdac.disable = value; this._disable = value; }
            }

            private uint _cal1, _cal2, _disable;
        } // End class myMdac

        public void ResetButtonColor()
        {
            this.adcButton.ResetBackColor();
        }

        public uint GetChannel () { return _id - 1; } // End GetChannel
        public void MatchAdcToGui () {
            // Match stored state info to gui values
            dac1 = dac1;
            dac2 = dac2;
            oFlag = oFlag;
            serializer = serializer;
            for (int i = 0; i < 4; i++) {
                mdacs[i].MatchMdacToGui();
            }
        } // End MatchAdcToGui
        public void MatchGuiToAdc () {
            // Match gui values to stored state info
            dac1 = _dac1;
            dac2 = _dac2;
            oFlag = _oFlag;
            serializer = _serializer;
            for (int i = 0; i < 4; i++) {
                mdacs[i].MatchGuiToMdac();
            }
        } // End MatchAdcToGui
        public void SetCalInfo (int calNum, int mdacNum) {
            if (this.isActive == false) throw new Exception("Trying to modify an inactive ADC");
            this.Update(() => {
                switch (calNum) {
                    case 0:
                        if (mdacNum == 3) {
                            for (int i = 0; i < 3; i++) this.mdacs[i].disable = 1;
                        }
                        this.oFlag = 0;
                        this.mdacs[mdacNum].disable = 0;
                        // stage 1 only has cal1
                        this.mdacs[mdacNum].cal1 = 1;
                        this.mdacs[mdacNum].cal2 = 0;
                        this.dac1 = 0;
                        this.dac2 = 0;
                        break;
                    case 1:
                        this.dac1 = 1;  // stage 2 has cal1, and dac1
                        break;
                    case 2:
                        this.mdacs[mdacNum].cal1 = 0;    // turn off cal1
                        this.mdacs[mdacNum].cal2 = 1;    // stage 3 has cal2 and dac1
                        break;
                    case 3:
                        this.dac2 = 1;  // stage 4 has cal2, dac1 and dac2
                        break;
                    case 4:
                        this.dac1 = 0; // turn all off after stage 4 is finished
                        this.dac2 = 0;
                        this.mdacs[mdacNum].cal2 = 0;
                        this.oFlag = 1;
                        break;
                    default:
                        throw new Exception("nothing to be done for calNum " + calNum);
                }
            }, true);
        }   // End SetCalInfo
        public List<byte> GetMdacControl () {
            var calib = new byte[4];
            for (uint i = 0; i < 4; i++) 
                calib[i] = (byte) ((mdacs[i].cal1 > 0) || (mdacs[i].cal2 > 0) ? 1 : 0);
            return new List<byte> {
                (byte)
                ((((mdacs[3].bias >> 1) & 1) << 7) +
                 (((mdacs[3].bias >> 0) & 1) << 6) +
                 (((mdacs[3].bias >> 3) & 1) << 5) +//changed from 4
                 (((mdacs[3].bias >> 2) & 1) << 4) +//changed from 3
                 (((mdacs[2].bias >> 1) & 1) << 3) +//changed from 2
                 (((mdacs[2].bias >> 0) & 1) << 2) +//changed from 1
                 (((mdacs[2].bias >> 3) & 1) << 1) +
                 (((mdacs[2].bias >> 2) & 1) << 0)),
                (byte)
                (((mdacs[2].disable & 1) << 7) +
                    ((mdacs[2].cal1 & 1) << 6) +
                    ((mdacs[2].cal2 & 1) << 5) +
                         ((calib[2] & 1) << 4) +
                 ((mdacs[3].disable & 1) << 3) +
                    ((mdacs[3].cal1 & 1) << 2) +
                    ((mdacs[3].cal2 & 1) << 1) +
                         ((calib[3] & 1) << 0)),
                (byte)
                ((((mdacs[1].bias >> 1) & 1) << 7) +
                 (((mdacs[1].bias >> 0) & 1) << 6) +
                 (((mdacs[1].bias >> 3) & 1) << 5) +
                 (((mdacs[1].bias >> 2) & 1) << 4) +
                     ((mdacs[1].disable & 1) << 3) +
                        ((mdacs[1].cal1 & 1) << 2) +
                        ((mdacs[1].cal2 & 1) << 1) +
                             ((calib[1] & 1) << 0)),
                (byte)
                           (((calib[0] & 1) << 7) +
                                ((dac1 & 1) << 6) +
                                ((dac2 & 1) << 5) +
                    ((mdacs[0].disable & 1) << 4) +
                       ((mdacs[0].cal1 & 1) << 3) +
                       ((mdacs[0].cal2 & 1) << 2) +
                (((mdacs[0].bias >> 1) & 1) << 1) +
                (((mdacs[0].bias >> 0) & 1) << 0)),
                (byte)
                ((((mdacs[0].bias >> 2) & 1) << 7) +
                 (((mdacs[0].bias >> 3) & 1) << 6) +
                 (((mdacs[0].bias >> 4) & 1) << 5) +
                 (((mdacs[0].bias >> 5) & 1) << 4) +
                                ((cFlag & 1) << 0)),
                (byte)
                    (((oFlag & 1) << 7) +
                  ((sarInput & 1) << 6) +
                ((serializer & 1) << 5))
            };
        } // End GetMdacControl
        public List<byte> GetCorrection0 () {
            return new List<byte> {
                (byte)
                ((mdacs[0].correction0 >> 4) & 0xff),
                (byte)
                (((mdacs[0].correction0 << 4) & 0xf0) |
                 ((mdacs[1].correction0 >> 8) & 0x0f)),
                (byte)
                (mdacs[1].correction0 & 0xff),
                (byte)
                ((mdacs[2].correction0 >> 4) & 0xff),
                (byte)
                (((mdacs[2].correction0 << 4) & 0xf0) |
                 ((mdacs[3].correction0 >> 8) & 0x0f)),
                (byte)
                (mdacs[3].correction0 & 0xff)
            };
        } // End GetCorrection0
        public List<byte> GetCorrection1 () {
            return new List<byte> {
                (byte)
                ((mdacs[0].correction1 >> 4) & 0xff),
                (byte) // << instead of >> ?
                (((mdacs[0].correction1 << 4) & 0xf0) |
                 ((mdacs[1].correction1 >> 8) & 0x0f)),
                (byte)
                (mdacs[1].correction1 & 0xff),
                (byte)
                ((mdacs[2].correction1 >> 4) & 0xff),
                (byte)
                (((mdacs[2].correction1 << 4) & 0xf0) |
                 ((mdacs[3].correction1 >> 8) & 0x0f)),
                (byte)
                (mdacs[3].correction1 & 0xff)
            };
        } // End GetCorrection1
    }

    [Serializable]
    public partial class MdacControl : UserControl {
        public MdacControl (uint id) {
            _id = id;
            InitializeComponent();
            cal1 = 0;
            cal2 = 0;
        }

        private void cal1_Click (object s, EventArgs e) { 
            this.cal1 = (uint)((this.cal1 == 0) ? 1 : 0);
            (this.Parent as ChipControl).OnValueChanged(s, e);
        }
        private void cal2_Click (object s, EventArgs e) { 
            this.cal2 = (uint)((this.cal2 == 0) ? 1 : 0);
            (this.Parent as ChipControl).OnValueChanged(s, e);
        }
        private void label_Click (object s, EventArgs e) { 
            this.disable = (uint)((this.disable == 0) ? 1 : 0);
            (this.Parent as ChipControl).OnValueChanged(s, e);
        }
        private uint _id;
        public bool CompareId (uint otherId) { return (_id == otherId); }
        public uint cal1 {
            get { return this._cal1; }
            set { 
                this._cal1 = value; 
                this.Update(() => this.cal1Button.BackColor = (value > 0) ? Global.OnColor : Global.OffColor); 
            }
        }
        public uint cal2 {
            get { return this._cal2; }
            set {
                this._cal2 = value;
                this.Update(() => this.cal2Button.BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
            }
        }
        public uint disable {
            get { return this._disable; }
            set {
                this._disable = value;
                this.Update(() => this.label1.BackColor = (value > 0) ? default(Color) : Color.DimGray, true); // if disable = 1, then color = default
            }
        }

        private uint _cal1, _cal2, _disable;
    }
}