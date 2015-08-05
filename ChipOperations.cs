using System;
using System.Collections.Generic;

namespace Nevis14 {
    // This file contains functions to send commands to the chip
    public delegate void ReadFunctionType (char port, List<byte> data);

    public partial class Form1 : System.Windows.Forms.Form {

        // Determine what to do with data based on the port in came in on
        private void ReceivedData (char port, List<byte> data) {
            if (port == 'A') {
                bufferA.AddRange(data);
                //WriteDataToGui(data); // different port A outputs are parsed differently
            } else if (port == 'B') {
                WriteCommandToGui("B RD", data);
            } else throw new Exception("invalid port: " + port);
        } // End ReceivedData

        // Return the state that the chip should be in
        public List<byte> GetStatus () {
            return new List<byte> {
                0xff, 0xfe,
                (byte)
                (((fifoAOperation & 7) << 0) |
                 ((chipControl1.chipReset & 1) << 3) |
                 ((0 & 1) << 4) | // reserved
                 ((0 & 1) << 5) | // async pll reset
                 ((adcFilter & 1) << 6) |
                 ((pllAndTriggerReset & 1) << 7)),
                (byte) (fifoACounter & 255),
                (byte) ((fifoACounter >> 8) & 63),
                (byte)
                (((startControlOperation & 1) << 0) |
                ((pulseCommand & 1) << 1) |
                ((startMeasurement & 1) << 2) |
                ((0 & 1) << 3) | // ack bit from last I2C operation
                ((0 & 1) << 4) | // all I2C commands executed
                ((chipControl1.softwareReset & 1) << 5) |
                ((startFifoAOperation & 1) << 6) |
                ((readStatus & 1) << 7)),
                0xff
            };
        } // End GetStatus

        // Send a state to the chip
        /* NB: The chip will trigger on rising edges so
         * any state change should have the form
         * <property> = 0; SendStatus();
         * <property> = 1; SendStatus();
         * <property> = 0;
         */
        public void SendStatus () {
            var data = GetStatus();
            ftdi.WriteToChip('B', data);
            WriteCommandToGui("B WR", data);
        } // End SendStatus

        public void SendChipResetCommand () { // byte[2]
            chipControl1.chipReset = 0; SendStatus(); // 00
            chipControl1.chipReset = 1; SendStatus(); // 0x08
            chipControl1.chipReset = 0;
        } // End ChipReset
        public void SendSoftwareResetCommand () { // byte[5]
            chipControl1.softwareReset = 0; SendStatus(); // 00
            chipControl1.softwareReset = 1; SendStatus(); // 0x20
            chipControl1.softwareReset = 0;
        } // End SoftwareReset
        public void SendPllResetCommand () { // byte[2]
            pllAndTriggerReset = 0; SendStatus(); // 00
            pllAndTriggerReset = 1; SendStatus(); // 0x80
            pllAndTriggerReset = 0;
            //GetPLLData(1);
        } // End PllReset
        public void SendReadStatusCommand () { // byte[5]
            readStatus = 0; SendStatus(); // 00
            readStatus = 1; SendStatus(); // 0x80
            readStatus = 0;
        } // End ReadStatus
        public void SendStartFifoACommand () { // byte[5]
            startFifoAOperation = 0; SendStatus(); // 00
            startFifoAOperation = 1; SendStatus(); // 0x40
            startFifoAOperation = 0;
        } // End StartFifoAOperation
        public void SendStartMeasurementCommand () {
            startMeasurement = 0; SendStatus();
            startMeasurement = 1; SendStatus();
            startMeasurement = 0;
        } // End StartMeasurement
        public void SendTriggerPulseCommand () { // byte5
            startMeasurement = 0; pulseCommand = 0; SendStatus(); // 00
            startMeasurement = 1; pulseCommand = 1; SendStatus(); // 0x06
            startMeasurement = 0; pulseCommand = 0;
        } // End TriggerPulse

        public void DoFifoAOperation (uint op, List<byte> data, int length = -1) {
            if (length == -1) length = data.Count;

            fifoACounter = (uint) length; // length shouldn't be negative, but be careful
            fifoAOperation = op;
            startControlOperation = 0;

            SendStartFifoACommand();
            SendReadStatusCommand();

            fifoAOperation = 0;
            fifoACounter = 0;
            if (op != 2 && data.Count > 0) {
                ftdi.WriteToChip('A', data);
                WriteCommandToGui("A   ", data);
                startControlOperation = 1; SendStatus();
                startControlOperation = 0;
                ftdi.ReadFromChip('B', ReceivedData);
                //ftdi.ReadFromChip('A', ReceivedData);
            }
        } // End DoFifoAOperation

        public void SendChipConfig () {
            var d = new List<byte>();
            // write configuration register command for ADC 1 & 2
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(CscDataWrite(selectAdc12));

            // write configuration register command for ADC 3 & 4
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(CscDataWrite(selectAdc34));

            d.Add(0);   // terminates the command queue

            DoFifoAOperation(1, d);
        } // End SendChipConfig

        //TODO
        public List<uint> ReadChipConfig () {
            var d = new List<byte>();
            var config = new List<uint>();

            //read back CSC 1&2 config reg into Nevis13 internal I2C buffer                
            //CSC1_2  = { 7'b1010000, 4'b0000, 5'b01010, {48{1'b0}} }  ;
            //{I2C.Write,  I2C.Stop,   4'd10}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addrw}, CSC1_2[63:0],
            //read back parameters into PC shift register
            ////{I2C.Write, I2C.NoStop, 4'd2}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addrr},
            //{I2C.Read,       I2C.Stop,4'd10}, {5'b11110,ChipId[2:1],RD},
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(CscDataRead(selectAdc12));
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.NoStop, 2, readAddr));
            d.AddRange(Header11((uint) I2C.Read, (uint) I2C.Stop, 9, readAddr));

            //read back CSC 3&4 config reg into Nevis13 internal I2C buffer
            //CSC3_4  = { 7'b0110000, 4'b0000 ,5'b01010, {48{1'b0}} }  ;
            //read back parameters into PC shift register
            //{I2C.Write, I2C.NoStop, 4'd 2}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addrr},
            //{I2C.Read,    I2C.Stop,4'd10}, {5'b11110,ChipId[2:1],RD},
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(CscDataRead(selectAdc34));
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.NoStop, 2, readAddr));
            d.AddRange(Header11((uint) I2C.Read, (uint) I2C.Stop, 9, readAddr));

            d.Add(0);   // terminates the command queue

            DoFifoAOperation(1, d);
            DoFifoAOperation(2, null, 80);

            bufferA.Clear();
            ftdi.ReadFromChip('B', ReceivedData);
            ftdi.ReadFromChip('A', ReceivedData);
            List<uint> parsedConfig = ParseConfig(bufferA);
            dataBox.Update(() => dataBox.AppendText(""));
            config.AddRange(parsedConfig);
            return config;
        } // End ReadChipConfig

        //TODO
        private List<uint> ParseConfig (List<byte> data) {
            List<uint> config = new List<uint>();
            return config;
        } // End ParseConfig

        // ddpu = 1, 2
        public void SendCalibControl (uint adc) {
            var d = new List<byte>();

            //Md1    =  {7'b1010000,  ADC_1,  5'b10110, 
            //12'b100000000001, 12'b100000000010, 12'b100000000011, 12'bxxx010000001};
            //write MDAC control register to Nevis13		      
            //{I2C.Write,  I2C.Stop, 4'd10}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addr}, Md1[63:0],
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(AdcHeader(adc, 22));
            d.AddRange(chipControl1.adcs[adc].GetMdacControl());

            //Dw1_c0 = { 7'b1010000, ADC, 5'b10100, 
            //12'b010000000001, 12'b001000000001, 12'b000100000001, 12'b000010000001};
            //write cor 0 parameters to Nevis13 ADC		      
            //{I2C.Write,  I2C.Stop, 4'd10}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addr}, Dw1_c0[63:0],
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(AdcHeader(adc, 20));
            d.AddRange(chipControl1.adcs[adc].GetCorrection0());

            //Dw1_c1 = { 7'b1010000, ADC, 5'b10101, 
            //12'b100000000001, 12'b010000000001, 12'b001000000001, 12'b000100000001};
            //write cor 1 parameters to Nevis13 ADC     
            //{I2C.Write,  I2C.Stop, 4'd10}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addr}, Dw1_c1[63:0], 
            d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
            d.AddRange(AdcHeader(adc, 21));
            d.AddRange(chipControl1.adcs[adc].GetCorrection1());

            d.Add(0);   // terminates the command queue

            //DoFifoAOperationWait(1, d, sender, e);
            DoFifoAOperation(1, d);
        } // End SendCalibControl

        public List<uint> ReadCalibControl (uint adc) {
            var d = new List<byte>();
            var corr = new List<uint>();

            //Dw1_c0 = { 7'b1010000, ADC, 5'b00100, {48{1'b0}} }  ;
            //Dw1_c1 = { 7'b1010000, ADC, 5'b00101, {48{1'b0}} }  ;
            //read back calibration and mdac parameters of ADC  into  Nevis13 I2C buffer		      
            //{I2C.Write,  I2C.Stop,   4'd10}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addrw}, Dw1_c0[63:0],
            //read back parameters into PC shift register
            //{I2C.Write, I2C.NoStop, 4'd2}, {5'b11110,ChipId[2:1],WR}, {ChipId[0],addrr},
            //{I2C.Read,       I2C.Stop,4'd10}, {5'b11110,ChipId[2:1],RD},
            for (uint index = 4; index <= 5; index++) {
                // index = 4 corresponds to Dw1_c0 => reads first 4 calibration parameters
                // index = 5 corresponds to Dw1_c1 => reads second 4 calibration parameters
                d.Clear();
                d.AddRange(Header10((uint) I2C.Write, (uint) I2C.Stop, 10, writeAddr));
                d.AddRange(AdcHeader(adc, index));
                d.AddRange(Repeat(6, 0));

                d.AddRange(Header10((uint) I2C.Write, (uint) I2C.NoStop, 2, readAddr));
                d.AddRange(Header11((uint) I2C.Read, (uint) I2C.Stop, 9, readAddr));

                d.Add(0);   // need this terminate the command queue

                DoFifoAOperation(1, d);
                DoFifoAOperation(2, null, 1);

                bufferA.Clear();
                ftdi.ReadFromChip('B', ReceivedData);
                ftdi.ReadFromChip('A', ReceivedData);
                List<uint> parsedCalib = ParseCalib(bufferA);
                dataBox.Update(() => dataBox.AppendText(
                    Environment.NewLine + parsedCalib[0] + " " + parsedCalib[1]
                    + Environment.NewLine + parsedCalib[2] + " " + parsedCalib[3]));
                // Sorted most-significant-bit to least-significant-bit
                corr.AddRange(parsedCalib);
            }
            return corr;
        } // End ReadCalibControl

        private List<uint> ParseCalib (List<byte> data) {
            if (data.Count < 6) throw new Exception("Did not read in enough calibration parameters.");

            List<uint> corrections = new List<uint>(4) { 0, 0, 0, 0 };
            corrections[0] = (uint) (data[3] + ((data[4] & 15) << 8)); // correction1 on MDAC1 or MDAC3
            corrections[1] = (uint) ((data[4] >> 4) + (data[5] << 4)); // correction0 on MDAC1 or MDAC3
            corrections[2] = (uint) (data[0] + ((data[1] & 15) << 8)); // correction1 on MDAC2 or MDAC4
            corrections[3] = (uint) ((data[1] >> 4) + (data[2] << 4)); // correction0 on MDAC2 or MDAC4
            return corrections;
        } // End ParseCalib

        public void GetAdcData (uint samples) {
            SendStartMeasurementCommand();

            fifoAOperation = 3;
            fifoACounter = samples;  // see the Verilog code
            // number of bytes sent to fifo is 16bytes/sample
            readStatus = 0;
            SendStartFifoACommand();

            bufferA.Clear();
            while (bufferA.Count < samples * 8) {
                ftdi.ReadFromChip('A', ReceivedData);
            }

            fifoAOperation = 0;
            fifoACounter = 0;
        } // End GetAdcData

        public void GetPllData (uint samples) {
            SendStartMeasurementCommand();

            fifoAOperation = 4;
            fifoACounter = samples;  //samples // see the Verilog code
            // number of bytes sent to fifo is 16bytes/sample
            readStatus = 0;
            SendStartFifoACommand();

            bufferA.Clear();
            while (bufferA.Count < samples * 8) {
                ftdi.ReadFromChip('A', ReceivedData);
            }

            fifoAOperation = 0;
            fifoACounter = 0;
        } // End GetPllData
    }
}