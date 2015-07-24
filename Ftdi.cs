using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;
using System.Windows.Forms;

namespace Nevis14 {
    // This file contains functions that allow reading/writing to the FTDIs
    public class Ftdi {
        public FTDI portA, portB;
        private string portADescription, portBDescription;
        private FTDI.FT_STATUS ftStatus;
        private bool isOpen;

        public Ftdi () {
            isOpen = false;
            portA = new FTDI();
            portB = new FTDI();
            portADescription = "Nevis13_tester A";
            portBDescription = "Nevis13_tester B";
            Open(portA, portADescription);
            Open(portB, portBDescription);
            isOpen = true;
        } // end constructor

        private void Open (FTDI port, string description) {
            uint deviceCount = 0;

            if (port == null) throw new Exception(description + "is not initialized");
            if (port.IsOpen) {
                Global.ShowError(description + " is already open");
                return;
            }

            // Determine the number of FTDI devices connected to the machine
            if ((ftStatus = port.GetNumberOfDevices(ref deviceCount)) != FTDI.FT_STATUS.FT_OK) {
                throw new FTDI.FT_EXCEPTION("Failed to get number of devices. err: " + ftStatus.ToString());
            }

            // If no devices available, return
            if (deviceCount == 0) throw new Exception("No devices on " + description);

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[deviceCount];

            // Populate our device list
            if ((ftStatus = port.GetDeviceList(ftdiDeviceList)) != FTDI.FT_STATUS.FT_OK) {
                throw new FTDI.FT_EXCEPTION("Failed to get device list. err " + ftStatus.ToString());
            }

            int foundIndex = -1;
            for (int i = 0; i < deviceCount; i++) {
                if (ftdiDeviceList[i].Description.ToString().Contains(description)) {
                    foundIndex = i;
                }
                /*
                Console.WriteLine("Device Index: " + i.ToString());
                Console.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                Console.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                Console.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                Console.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                Console.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                Console.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                Console.WriteLine("");
                */
            }
            if (foundIndex < 0) {
                throw new Exception("Failed to find device with description " + description);
            }

            if ((ftStatus = port.OpenByIndex((uint) foundIndex)) != FTDI.FT_STATUS.FT_OK) {
                throw new FTDI.FT_EXCEPTION("Failed to open device. err: " + ftStatus.ToString());
            }
        } // end Open

        public void WriteToChip (char portName, List<byte> data) {
            if (!isOpen) throw new Exception("Ftdi not initialized");

            uint bytesWritten = 0;
            uint bytesWaiting = 1;

            if (portName == 'A') {
                var dataReverse = new List<byte>();
                dataReverse.InsertRange(0, data);
                dataReverse.Reverse();

                while (bytesWaiting > 0) {
                    System.Threading.Thread.Sleep(20);
                    //Console.WriteLine("shh... sleeping");
                    if ((ftStatus = this.portB.GetTxBytesWaiting(ref bytesWaiting)) != FTDI.FT_STATUS.FT_OK) {
                        throw new FTDI.FT_EXCEPTION("Failed to get number of bytes waiting from port B. err: "
                            + ftStatus.ToString());
                    } 
                }
                if ((ftStatus = this.portA.Write(dataReverse.ToArray(), data.Count, ref bytesWritten))
                    != FTDI.FT_STATUS.FT_OK) {
                    throw new FTDI.FT_EXCEPTION("Failed to write to port A. err: " + ftStatus.ToString());
                }
            } else { // end Port A
                while (bytesWaiting > 0) {
                    System.Threading.Thread.Sleep(20);
                    //Console.WriteLine("shh... sleeping");
                    if ((ftStatus = this.portB.GetTxBytesWaiting(ref bytesWaiting)) != FTDI.FT_STATUS.FT_OK) {
                        throw new FTDI.FT_EXCEPTION("Failed to get number of bytes waiting from port A. err: "
                            + ftStatus.ToString());
                    }
                }

                if ((ftStatus = this.portB.Write(data.ToArray(), data.Count, ref bytesWritten))
                    != FTDI.FT_STATUS.FT_OK) {
                    throw new FTDI.FT_EXCEPTION("Failed to write to port B. err: " + ftStatus.ToString());
                }
            } // end Port B

            if (data.Count != bytesWritten) throw new Exception("Write length mismatch");
        } // end WriteToChip

        public void ReadFromChip (char portname, ReadFunctionType call) {
            if (portname != 'A' && portname != 'B') throw new Exception("Invalid port name "
                + portname + ". Should be A or B.");
            FTDI rdPort = (portname == 'B') ? portB : portA;
            if (rdPort == null) throw new Exception("No port " + portname + " exists");
            if (!rdPort.IsOpen) throw new Exception("Port " + portname + " is closed");

            uint numBytesAvailable = 0;
            DateTime startClock = DateTime.Now;
            while (numBytesAvailable <= 0) {
                // Wait for 2 seconds for data, then throw error
                if ((DateTime.Now - startClock).TotalSeconds > 2)
                    throw new Exception("2 seconds elapsed with no data. Was data expected?");

                System.Threading.Thread.Sleep(20);
                //Console.WriteLine("shh... sleeping");
                if ((ftStatus = rdPort.GetRxBytesAvailable(ref numBytesAvailable)) != FTDI.FT_STATUS.FT_OK) {
                    throw new FTDI.FT_EXCEPTION("Failed to get number of available bytes from "
                        + portname + ". err: " + ftStatus.ToString());
                }
            }
            byte[] readData = new byte[numBytesAvailable];
            uint numBytesRead = 0;

            if ((ftStatus = rdPort.Read(readData, numBytesAvailable, ref numBytesRead)) != FTDI.FT_STATUS.FT_OK) {
                throw new FTDI.FT_EXCEPTION("Failed to read data from "
                    + portname + ". err: " + ftStatus.ToString());
            }
            if (readData.Length != numBytesRead) throw new Exception("Read length mismatch");

            call(portname, readData.ToList());
        } // end ReadFromChip
        public void Close()
        {
            isOpen = false;
            portA.Close();
            portB.Close();
        } // End Close
    } // end class
} // end namespace
