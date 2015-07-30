﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Nevis14 {
    // This file contains the main functions used by the code
    // and the event handlers of the GUI
    public partial class Form1 : Form {
        // The file path is initialized to the output folder
        // The .exe file is located in ./bin/Debug so the path
        // must move back to the base of the directory structure
        string filePath = Application.StartupPath + "/../../OUTPUT/";
        List<BackgroundWorker> runningWorkers = new List<BackgroundWorker>();

        //TODO: add calibration constant requirements 
        const double calBound = 0.10;  // maximum % for the calibration parameters
        const double enobBound = 9.5; // minimum ENOB required for the chip to be good
        const string chipYear = "14";

        // Define constants for I2C
        const uint selectAdc12 = 64; // 1 << 6
        const uint selectAdc34 = 32; // 1 << 5
        const uint chipId = 5; // don't need anymore
        const uint i2cAddr = 30; // 11110
        const uint i2c10Bit = 0xf0;
        const uint i2cWrite = 0;
        const uint i2cRead = 1;
        const uint writeAddr = 0;
        const uint readAddr = 8; // 1 << 3
        // Define constants for I2C operations
        // Unfortunately, they need to be cast back to uint to use
        enum I2C : uint {
            NoOp = 0,
            Init = 1,
            Write = 2,
            Read = 4,
            Error = 3,
            Delay = 7,
            Stop = 1,
            NoStop = 0
        };

        const int seeWaitTimeInSeconds = 1;
        const int samplesForCalib = 3000;
        int samplesUsedForCalib = samplesForCalib - 20;

        // Variables used to keep track of the data being output
        // ------------
        private string[] chipdata = new string[5];  // Used to store the QA info output

        // Constants sent to FIFO B to control the FPGA
        // See the FPGA code for more information
        // byte[0] = 0xff, byte[1] = 0xfe
        private uint fifoAOperation = 0; // byte[2][0:2]
        private uint adcFilter = 0;      // byte[2][6]
        private uint pllReset = 0;       // byte[2][7]
        private uint fifoACounter = 0;   // byte[3] = fifoACounter[0:7], byte[4] = fifoACounter[8:12]
        private uint startControlOperation; // byte[5][0]
        private uint pulseCommand;        // byte[5][1]
        private uint startMeasurement;    // byte[5][2]
        private uint resetTrigger;        // byte[5][3]
        private uint startFifoAOperation; // byte[5][6]
        private uint readStatus;          // byte[5][7]
        // byte[6] = 0xff

        // Lists to store the data read from the chip
        List<byte> bufferA = new List<byte>();
        List<byte> bufferB = new List<byte>();

        // Lists to store calibration constant data
        List<double> sarForMdac = new List<double>();
        List<double> avgMdac = new List<double>();

        Ftdi ftdi;

        Stopwatch fullcalibtime = new Stopwatch();
        Stopwatch docaltime = new Stopwatch();
        Stopwatch sendcalibconttime = new Stopwatch();
        Stopwatch getadccalibtime = new Stopwatch();
        Stopwatch totaltime = new Stopwatch();
        Stopwatch ffttime = new Stopwatch();
        Stopwatch checkcaltime = new Stopwatch();
        Stopwatch takedatatime = new Stopwatch();
        Stopwatch inittime = new Stopwatch();



        // begin functions
        public Form1 () { 
            InitializeComponent();
        } // End constructor 

        private void InitializeConnection () {
            ftdi = new Ftdi();

            SendChipResetCommand();
            I2cInit();
            SendSoftwareResetCommand();

            SendChipConfig();
            for (uint i = 0; i < 4; i++) SendCalibControl(i);
        } // End InitializeConnection

        // Simple diagnostic to check that the chip can send and receive signals
        public bool SerializerTest () {
            for (uint iCh = 0; iCh < 4; iCh++) {
                chipControl1.adcs[iCh].serializer = 1;
                SendCalibControl(iCh);
            }
            GetAdcData(1);
            if (bufferA.Count != 8) throw new Exception("Buffer size: " + bufferA.Count + ", not correct for serializer test.");
            for (int i = 0; i < bufferA.Count; i += 2) {
                if (bufferA[i] != ((1 << 7) + (7 << 1))) return false; // byte should be 8'b10001110
                if (bufferA[i + 1] != (15 << 4)) return false; // byte should be 8'b11110000
            }
            for (uint iCh = 0; iCh < 4; iCh++) {
                chipControl1.adcs[iCh].serializer = 0;
                chipControl1.adcs[iCh].serializer = 0;
                chipControl1.adcs[iCh].serializer = 0;
                chipControl1.adcs[iCh].serializer = 0;
                SendCalibControl(iCh);
            }
            return true;
        } // End SerializerTest

        /// <summary>
        /// Manage the calibration calculation
        /// Returns true if successful, false otherwise
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public void DoCalibration (object sender, DoWorkEventArgs e) {
            BackgroundWorker bw = (BackgroundWorker)sender;
            sarForMdac.Clear();
            avgMdac.Clear();
            int channelNum = 0;
            int calNum = 0;
            int mdacNum = 3;

            while (channelNum <= 3) {
                chipControl1.Update(() => chipControl1.Activate((uint) channelNum), true);
                while (mdacNum >= 0) {
                    if (bw.CancellationPending) { e.Cancel = true; return; }
                    docaltime.Start();
                    if (!DoCalWork(ref chipControl1.adcs[channelNum], calNum, mdacNum)) { e.Result = false; return; }
                    docaltime.Stop();
                    calNum++;
                    if (calNum == 4) {
                        chipControl1.Update(() => chipControl1.adcs[channelNum].SetCalInfo(calNum, mdacNum), true);
                        mdacNum--;
                        calNum = 0;
                    }
                } // End loop over MDACs
                chipControl1.Update(() => chipControl1.adcs[channelNum].Deactivate(), true);
                // calculate and store calibration constants
                GetConst(channelNum);
                // Send calibration constants to the chip
                SendCalibControl((uint) channelNum);

                // Check that calibration constants match the numbers on the chip
                Console.WriteLine("check channel " + channelNum);
                bool success = CheckCalibration(channelNum);
                chipControl1.Update(() => chipControl1.adcs[channelNum].IsCalibrated(success), true);

                // Get ready for next channel
                //if (!success) { e.Result = false; return false; }
                if (bw.CancellationPending) { e.Cancel = true; return; }

                channelNum++; mdacNum = 3; calNum = 0;
                sarForMdac.Clear();
                avgMdac.Clear();
            } // End loop over channels

            e.Result = true;
        } // End DoCalibration

        /// <summary>
        /// The main calibration worker. Sends signals to the ADC and reads data back
        /// Stores data for use by GetConst
        /// </summary>
        /// <param name="adc"></param>
        /// <param name="calNum"></param>
        /// <param name="mdacNum"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public bool DoCalWork (ref AdcControl adc, int calNum, int mdacNum, int retry = 0) {
            if (adc.isActive == false) throw new Exception("Trying to calibrate inactive ADC");
            double totSar = 0;
            string mdacbuffer = "";
            string[] thisMdac = new string[4] { "99", "99", "99", "99" };
            double[] totMdac = new double[4] { 0, 0, 0, 0 };

            adc.SetCalInfo(calNum, mdacNum);

            sendcalibconttime.Start();
            SendCalibControl(adc.GetChannel());
            sendcalibconttime.Stop();

            getadccalibtime.Start();
            GetAdcData(samplesForCalib);
            getadccalibtime.Stop();
            // If the buffer is not full, then retry up to two times
            if (bufferA.Count != samplesForCalib * 8) {
                Console.WriteLine(Environment.NewLine + "Retrying MDAC: " + (mdacNum + 1) + " calnum: " + calNum
                    + Environment.NewLine + "buffer.Count: " + bufferA.Count);
                if (retry < 3) return DoCalWork(ref adc, calNum, mdacNum, retry + 1);
                else {
                    adc.Deactivate();
                    adc.IsCalibrated(false);
                    return false;
                }
            }

            for (int i = 20; i < samplesForCalib; i++) {
                int chOffset = 0;
                switch (adc.GetChannel()) {
                    case 0:
                        break;
                    case 1:
                        chOffset = 2;
                        break;
                    case 2:
                        chOffset = 4;
                        break;
                    case 3:
                        chOffset = 6;
                        break;
                    default:
                        throw new Exception("invalid channelNum: " + adc.GetChannel());
                }
                //read 2nd entry in each line, or 4th entry for channel 2
                totSar += bufferA[8 * i + (1 + chOffset)];
                // read the 1st column for channel 1, 3rd for channel 2
                mdacbuffer = Global.NumberToString(bufferA[8 * i + chOffset], 2, 8);

                // Parse all mdacs so they can be averaged later
                for (int j = 0; j < 4; j++) {
                    // each mdac is 2 chars long
                    thisMdac[j] = mdacbuffer.Substring(2 * j, 2);
                    // convert 2 char "binary" string to decimal
                    switch (thisMdac[j]) {
                        case "00":
                            totMdac[j] += 0;
                            break;
                        case "01":
                            totMdac[j] += 1;
                            break;
                        case "11":
                            totMdac[j] += 2;
                            break;
                        default:
                            throw new Exception("Substring " + thisMdac[j] + " sent to mdac " + j + " is invalid.");
                    }
                }
            } // End loop over samples
            sarForMdac.Add(totSar / samplesUsedForCalib);
            for (int j = 0; j < 4; j++) {
                avgMdac.Add(totMdac[j] / samplesUsedForCalib);
            }

            // Print the average MDAC readings for this stage
            Console.WriteLine(" MDAC" + mdacNum + " avgCal" + calNum + ": " + (totSar / samplesUsedForCalib));
            Console.WriteLine(" mdac1:" + (totMdac[0] / samplesUsedForCalib) +
                    "  mdac2:" + (totMdac[1] / samplesUsedForCalib) +
                    "  mdac3:" + (totMdac[2] / samplesUsedForCalib) +
                    "  mdac4:" + (totMdac[3] / samplesUsedForCalib));

            return true;
        } // End DoCalWork

        /// <summary>
        /// Uses the calibration data calculated in doCalWork() and
        /// calculates the calibration constants, puts them in GUI
        /// if Apply clicked and saves them to a file if Cal clicked
        /// </summary>
        public void GetConst (int channelNum) {
            // Initializing SAR and MDAC arrays from the calibration process
            // sarForMdac[] stores 16 sar readings, one from each calibration step
            // avgMdac[] stores 64 mdac readings, 4 mdacs for each of 16 steps in cal
            // Details in DblPrecisionCalib.C

            //---------------------MDAC 4---------------------//
            double[] sarForMdac4 = new double[4] { sarForMdac[0], sarForMdac[1], sarForMdac[2], sarForMdac[3] };

            //---------------------MDAC 3---------------------//
            double[] MDAC4_for_mdac3 = new double[4] { avgMdac[19], avgMdac[23], avgMdac[27], avgMdac[31] };
            double[] sarForMdac3 = new double[4] { sarForMdac[4], sarForMdac[5], sarForMdac[6], sarForMdac[7] };

            //---------------------MDAC 2---------------------// 
            double[] MDAC3_for_mdac2 = new double[4] { avgMdac[34], avgMdac[38], avgMdac[42], avgMdac[46] };
            double[] MDAC4_for_mdac2 = new double[4] { avgMdac[35], avgMdac[39], avgMdac[43], avgMdac[47] };
            double[] sarForMdac2 = new double[4] { sarForMdac[8], sarForMdac[9], sarForMdac[10], sarForMdac[11] };

            //---------------------MDAC 1---------------------//
            double[] MDAC2_for_mdac1 = new double[4] { avgMdac[49], avgMdac[53], avgMdac[57], avgMdac[61] };
            double[] MDAC3_for_mdac1 = new double[4] { avgMdac[50], avgMdac[54], avgMdac[58], avgMdac[62] };
            double[] MDAC4_for_mdac1 = new double[4] { avgMdac[51], avgMdac[55], avgMdac[59], avgMdac[63] };
            double[] sarForMdac1 = new double[4] { sarForMdac[12], sarForMdac[13], sarForMdac[14], sarForMdac[15] };

            //------------------------------- Initialization ---------------------------------------//

            double[] mdac4_reading = new double[4] { -999, -999, -999, -999 };
            double[] mdac3_reading = new double[4] { -999, -999, -999, -999 };
            double[] mdac2_reading = new double[4] { -999, -999, -999, -999 };
            double[] mdac1_reading = new double[4] { -999, -999, -999, -999 };

            //these will be the final calibration constants at the end
            double[] calib00_mdac = new double[4] { -1, -1, -1, -1 };
            double[] calib01_mdac = new double[4] { -1, -1, -1, -1 };
            double[] finalCalib = new double[4] { -1, -1, -1, -1 };

            //12 bit numbers 1000 0000 0000 = 2048
            //11 bit numbers  100 0000 0000 = 1024  
            //10 bit numbers   10 0000 0000 = 512 
            // 9 bit numbers    1 0000 0000 = 256
            double[] position = new double[4] { 256, 512, 1024, 2048 };

            double mdac4_difference00 = sarForMdac4[0] - sarForMdac4[1];
            double mdac4_difference01 = sarForMdac4[2] - sarForMdac4[3];

            calib00_mdac[3] = position[0] / 2.0 - mdac4_difference00;
            calib01_mdac[3] = position[0] / 2.0 - mdac4_difference01;
            Console.WriteLine("==============Starting Calibration===============");
            Console.WriteLine(" MDAC4: calib00 = " + calib00_mdac[3] + ", calib01 = " + calib01_mdac[3] + "");

            double CALIB4 = calib00_mdac[3] + calib01_mdac[3];

            //------------ Next is MDAC3: -------------//

            for (int i = 0; i < 4; ++i) {
                if (MDAC4_for_mdac3[i] == 0) {
                    //do nothing
                } else if (MDAC4_for_mdac3[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m4 calib00 only
                    sarForMdac3[i] += (position[0] / 2 - calib00_mdac[3]);
                } else if (MDAC4_for_mdac3[i] == 2) {
                    //2^x spot, correct with m4 calib00 and calib01
                    sarForMdac3[i] += (position[0] - calib00_mdac[3] - calib01_mdac[3]);
                }
                mdac3_reading[i] = sarForMdac3[i];
            }

            double mdac3_difference00 = mdac3_reading[0] - mdac3_reading[1];
            double mdac3_difference01 = mdac3_reading[2] - mdac3_reading[3];

            //now need to compare those differences to 
            //the full range we could have gotten:
            calib00_mdac[2] = position[0] - CALIB4 - mdac3_difference00;
            calib01_mdac[2] = position[0] - CALIB4 - mdac3_difference01;

            Console.WriteLine(" MDAC3: calib00 = " + calib00_mdac[2] + ", calib01 = " + calib01_mdac[2] + "");

            double CALIB3 = calib00_mdac[2] + calib01_mdac[2];

            //------------ Next is MDAC2: -------------//

            for (int i = 0; i < 4; ++i) {

                if (MDAC3_for_mdac2[i] == 0) {
                    //do nothing
                } else if (MDAC3_for_mdac2[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m3 calib00 only
                    //...note max range of MDAC3 would have been position[1]/2 - 2*CALIB4,
                    //   then minus the calibration constant for the MDAC3 reading
                    sarForMdac2[i] += (position[1] / 2 - 2 * CALIB4 - calib00_mdac[2]);
                } else if (MDAC3_for_mdac2[i] == 2) {
                    //2^x spot, correct with m3 calib00 and calib01
                    sarForMdac2[i] += (position[1] - 2 * CALIB4 - calib00_mdac[2] - calib01_mdac[2]);
                }


                if (MDAC4_for_mdac2[i] == 0) {
                    //do nothing 
                } else if (MDAC4_for_mdac2[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m4 calib00 only
                    sarForMdac2[i] += (position[0] / 2 - calib00_mdac[3]);
                } else if (MDAC4_for_mdac2[i] == 2) {
                    //2^x spot, correct with m4 calib00 and calib01
                    sarForMdac2[i] += (position[0] - calib00_mdac[3] - calib01_mdac[3]);
                }

                mdac2_reading[i] = sarForMdac2[i];

            }

            double mdac2_difference00 = mdac2_reading[0] - mdac2_reading[1];
            double mdac2_difference01 = mdac2_reading[2] - mdac2_reading[3];

            //now need to compare those differences to 
            //the full range we could have gotten:
            calib00_mdac[1] = position[1] - (2 * CALIB4 + CALIB3) - mdac2_difference00;
            calib01_mdac[1] = position[1] - (2 * CALIB4 + CALIB3) - mdac2_difference01;

            Console.WriteLine(" MDAC2: calib00 = " + calib00_mdac[1] + ", calib01 = " + calib01_mdac[1] + "");

            double CALIB2 = calib00_mdac[1] + calib01_mdac[1];

            //------------ Finally is MDAC1: -------------//

            for (int i = 0; i < 4; ++i) {

                if (MDAC2_for_mdac1[i] == 0) {
                    //do nothing
                } else if (MDAC2_for_mdac1[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m2 calib00 only
                    //...note max range of MDAC2 would have been position[1]/2 - 2*(2*CALIB4+CALIB3),
                    //   then minus the calibration constant for the MDAC2 reading
                    sarForMdac1[i] += (position[2] / 2 - 4 * CALIB4 - 2 * CALIB3 - calib00_mdac[1]);
                } else if (MDAC2_for_mdac1[i] == 2) {
                    //2^x spot, correct with m3 calib00 and calib01
                    sarForMdac1[i] += (position[2] - 4 * CALIB4 - 2 * CALIB3 - calib00_mdac[1] - calib01_mdac[1]);
                }

                if (MDAC3_for_mdac1[i] == 0) {
                    //do nothing
                } else if (MDAC3_for_mdac1[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m3 calib00 only
                    //...note max range of MDAC3 would have been position[1]/2 - 2*CALIB4,
                    //   then minus the calibration constant for the MDAC3 reading
                    sarForMdac1[i] += (position[1] / 2 - 2 * CALIB4 - calib00_mdac[2]);
                } else if (MDAC3_for_mdac1[i] == 2) {
                    //2^x spot, correct with m3 calib00 and calib01
                    sarForMdac1[i] += (position[1] - 2 * CALIB4 - calib00_mdac[2] - calib01_mdac[2]);
                }

                if (MDAC4_for_mdac1[i] == 0) {
                    //do nothing 
                } else if (MDAC4_for_mdac1[i] == 1) {
                    //need 2^(x-1) spot rather than 2^x, correct with m4 calib00 only
                    sarForMdac1[i] += (position[0] / 2 - calib00_mdac[3]);
                } else if (MDAC4_for_mdac1[i] == 2) {
                    //2^x spot, correct with m4 calib00 and calib01
                    sarForMdac1[i] += (position[0] - calib00_mdac[3] - calib01_mdac[3]);
                }

                mdac1_reading[i] = sarForMdac1[i];

            }


            double mdac1_difference00 = mdac1_reading[0] - mdac1_reading[1];
            double mdac1_difference01 = mdac1_reading[2] - mdac1_reading[3];

            //now need to compare those differences to 
            //the full range we could have gotten:
            calib00_mdac[0] = position[2] - (2 * (2 * CALIB4 + CALIB3) + CALIB2) - mdac1_difference00;
            calib01_mdac[0] = position[2] - (2 * (2 * CALIB4 + CALIB3) + CALIB2) - mdac1_difference01;

            Console.WriteLine(" MDAC1: calib00 = " + calib00_mdac[0] + ", calib01 = " + calib01_mdac[0] + "");

            double CALIB1 = calib00_mdac[0] + calib01_mdac[0];
            Console.WriteLine(" Calib numbers for 00 = " + calib00_mdac[0] + " " + calib00_mdac[1] + " " + calib00_mdac[2] + " " + calib00_mdac[3] + "");
            Console.WriteLine(" Calib numbers for 01 = " + calib01_mdac[0] + " " + calib01_mdac[1] + " " + calib01_mdac[2] + " " + calib01_mdac[3] + "");

            //for mdac4, do nothing

            //for the rest, do mdac3 += (mdac4_c1 + mdac4_c2), in order!
            calib00_mdac[2] += (calib00_mdac[3] + calib01_mdac[3]);
            calib01_mdac[2] += (calib00_mdac[3] + calib01_mdac[3]);

            calib00_mdac[1] += (calib00_mdac[2] + calib01_mdac[2]);
            calib01_mdac[1] += (calib00_mdac[2] + calib01_mdac[2]);

            calib00_mdac[0] += (calib00_mdac[1] + calib01_mdac[1]);
            calib01_mdac[0] += (calib00_mdac[1] + calib01_mdac[1]);
            Console.Write(" ***FINAL CALIB CONSTANTS (stick this in apply_calibration.C)***"
                + Environment.NewLine + " int calib00_mdac[4] = {" + calib00_mdac[0] + " " + calib00_mdac[1] + " " + calib00_mdac[2] + " " + calib00_mdac[3] + "}"
                + Environment.NewLine + " int calib01_mdac[4] = {" + calib01_mdac[0] + " " + calib01_mdac[1] + " " + calib01_mdac[2] + " " + calib01_mdac[3] + "}"
                + Environment.NewLine + " !!! NOTE !!!: calib00 should = calib01 for a given MDAC (because gain is constant)...if they are not, take the average");

            for (int i = 0; i < 4; i++) {
                finalCalib[i] = (calib00_mdac[i] + calib01_mdac[i]) / 2;
            }

            Console.WriteLine("GUI constants:");

            for (int j = 0; j < 4; j++) {
                // Round the doubles so we can put them in the GUI
                int constDown = Convert.ToInt32(Math.Round(Math.Pow(2, (10 - j)) - finalCalib[j], 0 /*, MidpointRounding.AwayFromZero*/));
                int constUp = 2 * constDown;        // Top constant is just twice the rounded, averaged bottom constant
                //int constUp = Convert.ToInt32(Math.Round(2*(Math.Pow(2, (10 - j)) - finalCalib[j]), 0 /*, MidpointRounding.AwayFromZero*/));        // Top constant is just twice the rounded, averaged bottom constant
                Console.WriteLine("Top:" + constUp + "   Bottom:" + constDown);

                // Set the correction constants in the GUI
                chipControl1.adcs[channelNum].mdacs[j].correction1 = (uint) constUp;
                chipControl1.adcs[channelNum].mdacs[j].correction0 = (uint) constDown;
            }
            using (StreamWriter corrections = File.AppendText(filePath + "corrections.txt"))
            {
                corrections.WriteLine(System.DateTime.Now + ", channel: " + channelNum + ", "
                + Math.Round(finalCalib[0], 3) + ", "
                + Math.Round(finalCalib[1], 3) + ", "
                + Math.Round(finalCalib[2], 3) + ", "
                + Math.Round(finalCalib[3], 3));
            }
            chipdata[channelNum + 1] = (channelNum + 1) + ", "
                + Math.Round(finalCalib[0], 3) + ", "
                + Math.Round(finalCalib[1], 3) + ", "
                + Math.Round(finalCalib[2], 3) + ", "
                + Math.Round(finalCalib[3], 3) + ", ";
            Console.WriteLine("================Ended Calibration================");
        }   // End GetConst

        /// <summary>
        /// Reads the constants and checks if they are different
        /// Than what was sent to the chipControl1. If they are different
        /// We flag the event, try once more. If they are still different
        /// We move on to take FFT data. We then loop through reading constants
        /// and writing FFT data until 3 minutes is up and then recalibrate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public bool CheckCalibration (int iCh, int retry = 0) {
            if (retry > 10) { return false; }
            checkcaltime.Start();
            SendCalibControl((uint) iCh);

            // Read back calibration constants
            // order is most-to-least significant
            List<uint> corr = ReadCalibControl((uint) iCh);
            if (corr.Count != 8) return CheckCalibration(iCh, retry + 1);

            // Now we compare the 12-bit binary triply redundant constants the chip reads and compare them to the
            // values that have been set already. If they disagree, we flag and try again. For order see GetConst()
            if (corr[0] != chipControl1.adcs[iCh].mdacs[0].correction1 || corr[1] != chipControl1.adcs[iCh].mdacs[0].correction0
                || corr[2] != chipControl1.adcs[iCh].mdacs[1].correction1 || corr[3] != chipControl1.adcs[iCh].mdacs[1].correction0
                || corr[4] != chipControl1.adcs[iCh].mdacs[2].correction1 || corr[5] != chipControl1.adcs[iCh].mdacs[2].correction0
                || corr[6] != chipControl1.adcs[iCh].mdacs[3].correction1 || corr[7] != chipControl1.adcs[iCh].mdacs[3].correction0) {
                System.IO.File.AppendAllText(filePath + "mdacRangeError.txt",
                    Environment.NewLine + System.DateTime.Now
                    + Environment.NewLine + "MDAC reading in channel " + (iCh + 1) + "is incorrect. Chip reads,"
                    + Environment.NewLine + "MDAC[4] corr0: " + corr[7] + "\t corr1: " + corr[6]
                    + Environment.NewLine + "MDAC[3] corr0: " + corr[5] + "\t corr1: " + corr[4]
                    + Environment.NewLine + "MDAC[2] corr0: " + corr[3] + "\t corr1: " + corr[2]
                    + Environment.NewLine + "MDAC[1] corr0: " + corr[1] + "\t corr1: " + corr[0]
                    + Environment.NewLine + "Stored values are,"
                    + Environment.NewLine + "MDAC[4] corr0: " + chipControl1.adcs[iCh].mdacs[3].correction0 + "\t corr1: " + chipControl1.adcs[iCh].mdacs[3].correction1
                    + Environment.NewLine + "MDAC[3] corr0: " + chipControl1.adcs[iCh].mdacs[2].correction0 + "\t corr1: " + chipControl1.adcs[iCh].mdacs[2].correction1
                    + Environment.NewLine + "MDAC[2] corr0: " + chipControl1.adcs[iCh].mdacs[1].correction0 + "\t corr1: " + chipControl1.adcs[iCh].mdacs[1].correction1
                    + Environment.NewLine + "MDAC[1] corr0: " + chipControl1.adcs[iCh].mdacs[0].correction0 + "\t corr1: " + chipControl1.adcs[iCh].mdacs[0].correction1);

                // Calibration didn't match, so retry
                return CheckCalibration(iCh, retry + 1);
            } else { // Calibration matched. Move on to take data.
                System.IO.File.AppendAllText(filePath + "mdacRangeUsed.txt",
                        Environment.NewLine + System.DateTime.Now
                        + Environment.NewLine + "MDAC reading in channel " + (iCh + 1)
                        + Environment.NewLine + "MDAC[4] corr0: " + corr[7] + "\t corr1: " + corr[6]
                        + Environment.NewLine + "MDAC[3] corr0: " + corr[5] + "\t corr1: " + corr[4]
                        + Environment.NewLine + "MDAC[2] corr0: " + corr[3] + "\t corr1: " + corr[2]
                        + Environment.NewLine + "MDAC[1] corr0: " + corr[1] + "\t corr1: " + corr[0]);
               
                // Check the dynamic range of the channel, if more than calBound% of the counts
                // are lost then the chip is defective
                chipControl1.adcs[iCh].dynamicRange = corr[0] + corr[2] + corr[4] + corr[6] + 255;
                Console.WriteLine("Dynamic range of chip is: " + chipControl1.adcs[iCh].dynamicRange);
                checkcaltime.Stop();
                return ((1 - (chipControl1.adcs[iCh].dynamicRange / 4096.0)) < calBound);
            }
        }   // End CheckCalibration

        /// <summary>
        /// Simple method to take 6200 data points. Appends the data
        /// to the file adcData.txt, and prints it to the GUI
        /// </summary>
        /// <returns></returns>
        public bool TakeData () {
            GetAdcData(12400);
            if (bufferA.Count != 12400 * 8) { Console.WriteLine(bufferA.Count); return false; }

            List<string> lines = bufferA.ToDecimal();
            WriteDataToGui(lines);

            StreamWriter adcwrite = new StreamWriter(filePath + "adcData.txt");
            /*StringBuilder s = new StringBuilder("", bufferA.Count * 3);
            for (int i = 0; i < bufferA.Count; i += 8) {
                for (int j = 0; j < 8; j += 2) {
                    s.Append(((bufferA[i + j] << 8) + bufferA[i + j + 1]) + " ");
                }
                s.Append(Environment.NewLine);
            }*/
            StringBuilder s = new StringBuilder(lines.Count * 22);
            for (int i = 0; i < lines.Count; i += 8) {
                s.Append(lines[i]);
                s.Append(Environment.NewLine);
            }
            adcwrite.Write(s);
            adcwrite.Close();

            //System.IO.File.AppendAllText(filePath + "adcData.txt", s);
            return true;
        } // End TakeData

        public void SeeTest (object sender, DoWorkEventArgs e) {
            adcFilter = 0; SendStatus();
            for (uint iCh = 0; iCh < 4; iCh++) {
                chipControl1.SafeInvoke(() => {
                    chipControl1.adcs[iCh].oFlag = 0;
                    chipControl1.adcs[iCh].serializer = 1;
                });
                SendCalibControl(iCh);
            }
            GetAdcData(1000); // Fill buffer
            dataBox.Update(() => dataBox.Text = (bufferA.To16BitBinary()).ToSeparatedString(), true);

            adcFilter = 1; SendStatus();
            for (uint iCh = 0; iCh < 4; iCh++) {
                chipControl1.SafeInvoke(() => {
                    chipControl1.adcs[iCh].oFlag = 1;
                    chipControl1.adcs[iCh].serializer = 0;
                });
                SendCalibControl(iCh);
            }
            SendPllResetCommand();
            GetAdcData(1000); // Initialize SEE data taking
            dataBox.Update(() => dataBox.Text = (bufferA.To16BitBinary()).ToSeparatedString(), true);
            
            string seePath = CreateNewDirectory("SEE");
            string pllPath = CreateNewDirectory("PLL");
            int i = 0;
            while (!((BackgroundWorker)sender).CancellationPending) {
                FileWriteDelegate seeWriter = new FileWriteDelegate(File.AppendAllLines);
                FileWriteDelegate pllWriter = new FileWriteDelegate(File.AppendAllLines);
                System.Threading.Thread.Sleep(seeWaitTimeInSeconds * 1000);
                GetAdcData(1000);
                List<string> seeData = bufferA.To16BitBinary();
                dataBox.Update(() => dataBox.Text = seeData.ToSeparatedString());
                List<string> seeErrors;
                if ((seeErrors = CheckSeeData(seeData)).Count > 0){
                seeWriter.BeginInvoke(filePath + seePath + "seeData_" + i.ToString().PadLeft(3,'0') + ".txt", seeErrors, null, null);
                }

                GetPllData(10);
                List<string> pllData = bufferA.To16BitBinary();
                List<string> pllErrors;
                if ((pllErrors = CheckPllData(pllData)).Count > 0)
                {
                    pllWriter.BeginInvoke(filePath + pllPath + "pllData_" + i.ToString().PadLeft(3, '0') + ".txt", pllErrors, null, null);
                }
                SendPllResetCommand();
                i++;
            }
            adcFilter = 0; SendStatus();
            e.Cancel = true;
        } // End SeeTest
        private List<string> CheckSeeData(List<string> data)
        {
            List<string> errors = new List<string>();
            foreach (string sample in data)
            {
                if (sample == "") Console.WriteLine(data.IndexOf(sample) + " is empty");
                else if (sample != "10001110 11110000 10001110 11110000 10001110 11110000 10001110 11110000 ")
                {
                    errors.Add("Line " + data.IndexOf(sample) + ": " + sample);
                }
            }
            return errors;
        }
        private List<string> CheckPllData(List<string> data)
        {
            List<string> errors = new List<string>();
            foreach (string sample in data)
            {
                if (sample == "") Console.WriteLine(data.IndexOf(sample) + " is empty");
                else if (sample != "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 ")
                {
                    errors.Add("Line " + data.IndexOf(sample) + ": " + sample);
                }
            }
            return errors;
        }
        private List<string> ParseSee(List<byte> data) {
            if ((data.Count % 8) != 0) throw new Exception("SEE data size is not a multiple of 8.");
            List<string> lines = new List<string>();
            StringBuilder s = new StringBuilder(72);

            for (int i = 0; i < data.Count; i += 8) {
                s.Clear();
                for (int j = 0; j < 8; j++) {
                    // Show the binary numbers
                    s.Append(Convert.ToString(data[i + j], 2).PadLeft(8, '0') + " ");
                }
                lines.Add(s.ToString());
            }
            return lines;
        } // End ParseSee

        // Parses data and it to the data box (right side of GUI)
        private void WriteDataToGui (List<string> data) {
            // ADC data should be output in multiples of 8
            // (b.c. there are 4 channels on the chip and there are 2 bytes per channel)

            dataBox.Update(() => dataBox.AppendLines(data));
        } // End WriteDataToGui

        // Parses commands and writes them to the command box (upper left on GUI)
        private void WriteCommandToGui (string port, List<byte> data) {

            StringBuilder s = new StringBuilder("", data.Count * 3);
            using (StreamWriter commandsout = File.AppendText(filePath + "commands.log"))
            {
                commandsout.Write(Environment.NewLine + port + " ");
                for (int i = 0; i < data.Count; i++)
                {
                    s.Append(Global.NumberToString((uint)data[i], 16, 2) + " ");
                }
                commandsout.Write(s);
            };

            commandBox.Update(() => commandBox.AppendText(Environment.NewLine + port + " " + s));

            /*System.IO.File.AppendAllText(filePath + "commands.log",
                Environment.NewLine + port + " " + s);*/
        } // End WriteCommandToGui

        public string CreateNewDirectory (string prefix, int width = 2) {
            int dirNum = 0;
            string path = prefix + "_" + dirNum.ToString().PadLeft(width, '0');
            while (System.IO.Directory.Exists(filePath + path)) {
                dirNum++;
                path = path.Remove(path.Length - width);
                path += dirNum.ToString().PadLeft(width, '0');
            }
            System.IO.Directory.CreateDirectory(filePath + path);
            return path + "/";
        } // End CreateNewDirectory

        public void RunOnBkgWorker (DoWorkEventHandler work) {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(work);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BkgWorker_Completed);
            bw.WorkerSupportsCancellation = true;
            runningWorkers.Add(bw);
            bw.RunWorkerAsync();
        } // End RunOnBkgWorker

        private void BkgWorker_Completed (object sender, RunWorkerCompletedEventArgs e) {
            runningWorkers.Remove((BackgroundWorker) sender);
            if (e.Error != null) {
                Global.ShowError(e.Error.Message);
                ResetGui();
            } else if (e.Cancelled == true) {
                // Anything?
            } else {
                DialogResult answer = Global.AskError("Result of operation is " + e.Result.ToString() + ". Continue?");
                if (answer == DialogResult.Retry) {
                    runningWorkers.Add((BackgroundWorker) sender);
                    ((BackgroundWorker) sender).RunWorkerAsync();
                }
                // Treat Ignore as Continue
            }
        } // End BkgWorker_Completed
        
        private void connectButton_Click (object sender, EventArgs e) {
            // The background worker will only be started if there is a valid chip number (any int)
            connectButton.Update(() => connectButton.Enabled = false);
            if (this.chipNumBox.Text == "" || this.chipNumBox.BackColor == System.Drawing.Color.Red) {
                MessageBox.Show("Invalid chip id. Please enter the number of the chip you are testing before running the code.");
            } else {
                ResetGui();
                filePath += "Nevis14_" + chipNumBox.Text.PadLeft(5, '0') + "/";
                chipdata[0] = String.Format("* {0}, {1},{2}, ", chipNumBox.Text, DateTime.Now.Date, DateTime.Now.TimeOfDay);
                // Create folder to store output files, if it doesn't already exist
                if (!System.IO.Directory.Exists(filePath)) {
                    System.IO.Directory.CreateDirectory(filePath);
                }
                filePath += CreateNewDirectory("Run");
                chipdata[0] += filePath.Substring(filePath.Length - 3, 2);
                try {
                    InitializeConnection();
                } catch (FTD2XX_NET.FTDI.FT_EXCEPTION exc) { // handles FTDI exceptions
                    Global.ShowError("FTDI exception: " + exc.Message);
                }
                while (!SerializerTest()) {
                    // User has 3 options: 
                    //   Abort immediately quits program (in AskError), 
                    //   Retry continues the loop,
                    //   Ignore breaks the loop and continues the program
                    if (Global.AskError("Failed serializer test. Retry?") != DialogResult.Retry) break;
                }
            }
            connectButton.Update(() => connectButton.Enabled = true);
        } // End connectButton_Click

        private void calibButton_Click (object sender, EventArgs e) {
            RunOnBkgWorker(DoCalibration);
        }

        private void seeButton_Click (object sender, EventArgs e) {
            RunOnBkgWorker(SeeTest);
        }

        private void dataButton_Click (object sender, EventArgs e) {
            RunOnBkgWorker((obj, args) => {
                AdcData[] adcData;
                TakeData(); 
                adcData = FFT3(40, chipdata);
                WriteResult(adcData);
            });
        }

        private void cancelButton_Click (object sender, EventArgs e) {
            foreach (BackgroundWorker bw in runningWorkers) {
                bw.CancelAsync();
            }
            for (int channelnum = 0; channelnum < 4; channelnum++ ) {
                if (chipControl1.adcs[channelnum].isActive)
                    chipControl1.adcs[channelnum].Deactivate();
            }
        } // End cancelButton_Click

        private void chipNumBox_TypeValidationCompleted (object sender, System.Windows.Forms.TypeValidationEventArgs e) {
            this.chipNumBox.BackColor = default(System.Drawing.Color);
            if (!e.IsValidInput) {
                this.chipNumBox.BackColor = System.Drawing.Color.Red;
                e.Cancel = true;
            }
        } // End chipNumBox_TypeValidationCompleted

        private void WriteDataToFile () {
            fftBox.Image = Image.FromFile(filePath + "fft.png");
            File.WriteAllLines(filePath + "QAparams.txt", chipdata);
            File.AppendAllLines(filePath.Remove(filePath.Length - 6) + "QAparams_" + chipNumBox.Text + ".txt", chipdata);
            File.AppendAllLines(filePath.Remove(filePath.Length - 20) + "QAparams_all.txt", chipdata);
            Console.WriteLine("QA Parameters Written to File");

        } // End WriteDataToFile

        private void WriteResult (AdcData[] adcData) {
            if (adcData.Length != 4) throw new Exception("Invalid input to WriteResult.");
            bool underperf = false;
            bool defect = false;
            for (int i = 0; i < 4; i++) {
                if (adcData[i].enob < (enobBound * 0.9) || (1 - chipControl1.adcs[i].dynamicRange / 4096.0) > calBound)  
                    defect = true;
                else if (adcData[i].enob < enobBound)
                    underperf = true;
                resultBox.Update(() => resultBox.Text += "Channel " + (i + 1) 
                    + Environment.NewLine + "   ENOB = " + Math.Round(adcData[i].enob,4)
                    + Environment.NewLine + "   Range = " + chipControl1.adcs[i].dynamicRange 
                    + Environment.NewLine, true);
            }
            if (!underperf && !defect) {
                resultBox.Update(() => { resultBox.BackColor = Color.Green; 
                    resultBox.Text += "Chip fully operational"; });
            }
            else {
                if (defect){
                    resultBox.Update(() => { resultBox.BackColor = Color.Red; 
                        resultBox.Text += "Defective Chip"; });
                }else{
                    resultBox.Update(() => { resultBox.BackColor = Color.Yellow;
                        resultBox.Text += "Chip Underperforming"; });
                    
                }
            }
        } // End WriteResult

        public void ResetGui()
        {
            filePath = Application.StartupPath + "/../../OUTPUT/";
            for (int i = 0; i < 4; i++)
            {
                chipControl1.Update(() => chipControl1.adcs[i].ResetButtonColor());
            }
            resultBox.Update(() => { resultBox.Clear(); resultBox.ResetBackColor(); });
            commandBox.Update(() => commandBox.Clear());
            dataBox.Update(() => dataBox.Clear());
            fftBox.Update(() => fftBox.Image = null);
            if(ftdi != null)
                ftdi.Close();

            // Reset Timers
            totaltime.Reset();
            ffttime.Reset();
            takedatatime.Reset();
            sendcalibconttime.Reset();
            getadccalibtime.Reset();
            fullcalibtime.Reset();
            docaltime.Reset();
            checkcaltime.Reset();
            inittime.Reset();
            
        } // End resetGui
    } // End Form1
}
