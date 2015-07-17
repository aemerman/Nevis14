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
            if (adcs[0].isActive || adcs[1].isActive || adcs[2].isActive || adcs[3].isActive) {
                throw new Exception("trying to activate an ADC control while another is still active");
            }
            adcs[channelNum].Activate(ref mdacControls, ref dacButtons);
        }
    }

    [Serializable]
    public partial class AdcControl : UserControl {
        public AdcControl (uint id) {
            _id = id;
            InitializeComponent();
            _dacs = null;
            mdacs = new Mdac[4] { new Mdac(1), new Mdac(2), new Mdac(3), new Mdac(4) };
        }

        public void Activate (ref MdacControl[] mdacControls, ref Button[] dacButtons) {
            if (mdacControls.Length != 4 || dacButtons.Length != 2) {
                throw new Exception("wrong number of components. mdacs: "
                    + mdacControls.Length + " dacs: " + dacButtons.Length);
            }

            // Set initial button colors
            this.Update(() => this.adcButton.BackColor = Color.DimGray);
            _dacs = dacButtons;
            for (int i = 0; i < 4; i++) {
                mdacs[i].Activate(ref mdacControls[i]);
            }
            isActive = true;
            dac1 = 0;
            dac2 = 0;
        } // End Activate
        public void Deactivate () {
            oFlag = 1;
            isActive = false; _dacs = null;
            for (int i = 0; i < 4; i++) { mdacs[i].Deactivate(); }
        } // End Deactivate
        public void IsCalibrated (bool success) {
            this.Update(() => this.adcButton.BackColor = 
                success ? System.Drawing.Color.Green : System.Drawing.Color.Red);
        } // End IsCalibrated
         
        // The Update method for AdcControl contains a check
        // against isActive. If isActive is false it will not
        // carry out the action. This prevents the AdcControl
        // from modifying the Gui while another AdcControl is
        // using it.
        public bool isActive = false;
        public Mdac[] mdacs;
        private Button[] _dacs;

        private uint _id;
        public uint dac1 {
            get {
                if (isActive) return _dac1;
                else return 0;
            }
            set {
                if (isActive) {
                    this._dac1 = value;
                    _dacs[0].Update(() => _dacs[0].BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
            }
        }
        public uint dac2 {
            get {
                if (isActive) return _dac2;
                else return 0;
            }
            set {
                if (isActive) {
                    this._dac2 = value;
                    _dacs[1].Update(() => _dacs[1].BackColor = (value > 0) ? Global.OnColor : Global.OffColor);
                }
            }
        }
        public uint cFlag = 0;
        public uint oFlag = 0;
        public uint sarInput = 0;
        public uint serializer = 0;

        private uint _dac1, _dac2;
        public class Mdac {
            public Mdac (uint id) {
                _mdac = null;
                _id = id;
                correction0 = (uint) 1 << (11 - (int) _id); // = 2^(11-_id)
                correction1 = (uint) 1 << (12 - (int) _id);
            }
            public bool isActive; // Should always mirror the isActive property of the parent AdcControl
            private MdacControl _mdac;
            private uint _id;
            public void Activate (ref MdacControl mdac) {
                this._mdac = mdac;
                if (!this._mdac.CompareId(_id)) MessageBox.Show("mdac IDs don't match");
                isActive = true;
                cal1 = 0;
                cal2 = 0;
                disable = 1;
            }

            public void Deactivate () {
                this.isActive = false;
                this._mdac = null;
            } // End Deactivate

            public uint correction0;
            public uint correction1;
            public uint bias = 0;
            public uint cal1 {
                get {
                    if (isActive) return this._mdac.cal1;
                    else return 0;
                }
                set { if (isActive) this._mdac.cal1 = value; }
            }
            public uint cal2 {
                get {
                    if (isActive) return this._mdac.cal2;
                    else return 0;
                }
                set { if (isActive) this._mdac.cal2 = value; }
            }
            public uint disable {
                get {
                    if (isActive) return this._mdac.disable;
                    else return 0;
                }
                set { if (isActive) this._mdac.disable = value; }
            }
        } // End class myMdac

        public uint GetChannel () { return _id - 1; } // End GetChannel
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
                        this.mdacs[mdacNum].cal1 = 0;
                        this.mdacs[mdacNum].cal2 = 0;
                        /*for (int i = 0; i < 3; i++) {
                            // disable mdacs lower than the current one
                            if (i < mdacNum) this.mdacs[i].disable = 1;
                            else { // reset this mdac and higher ones
                                this.mdacs[i].cal1 = 0;
                                this.mdacs[i].cal2 = 0;
                                this.mdacs[i].disable = 0;
                            }
                        }*/
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
                        break;
                    default:
                        throw new Exception("nothing to be done for calNum " + calNum);
                }
            });
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
                this.Update(() => this.Enabled = !(value > 0)); // if disable = 1, then Enabled = false
            }
        }

        private uint _cal1, _cal2, _disable;
    }
}