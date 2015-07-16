using System.Collections.Generic;

namespace Nevis14 {
    // This file contains functions to facilitate communicating with the FPGA
    public partial class Form1 : System.Windows.Forms.Form {
        /*
            Before one can use the FPGA I2C master one needs to initialize I2C Wishbone registers
            and also to set the “10bit” address  mode inside the Nevis13 chipControl1.
            The sequence consists of the following steps:
            1.USB_cntr_in = 
            {  {W_INIT,   1'b0, 4'd3}, {8'h0f},{8'h00},{8'h80},  //prescalers for 40MHz clock and 0.5MHz I2C speed
               {I2C_WR,  1'b1, 4'd2}, {chip_id,4'b0000,WR},  {8'h20},   //set "10bit" address mode
               {584{1'b0}}  };
            2. Send USB_cntr_in command stream to the FPGA.
            3. Start the command stream execution (set status5[0] to “1”).  
            4. Check that all I2C commands have been executed (status5[4] is “1”).
            5. Reset status5[0].

            Addressing mode commands.
            After the reset, Nevis13 I2C interface is in “7bit” addressing mode. 
            To change it to “10bit” mode, USB command queue must contain the command:
            {I2C_WR,  1'b1, 4'd2}, {chip_id,4'b0000,WR},  {8'h20},  
            The command to go back to the “7bit” addressing mode from “10bit” mode is:
            {I2C_WR,  STP,4'd3}, {5'b11110, chip_id[2:1],WR}, {chip_id[0],7'h00},{8'h00}
        */

        public byte Header (uint opCode, uint isStop, uint length) {
            return (byte) (((opCode & 7) << 5) | ((isStop & 1) << 4) | (length & 15));
        } // end Header
        
        public List<byte> Header7 (uint opCode, uint isStop, uint length) {
            return new List<byte>
            {
                Header(opCode, isStop, length + 1),
                (byte)(((chipId & 7) << 5) | (opCode == (uint)I2C.Write ? i2cWrite : i2cRead))
            };
        } // end Header7
        
        public List<byte> Header10 (uint opCode, uint isStop, uint length, uint address) {
            return new List<byte>
            {
                Header(opCode, isStop, length),
                (byte)(i2c10Bit | (chipId & 6) | (opCode == (uint)I2C.Write ? i2cWrite : i2cRead)),
                (byte)(((chipId & 1) << 7) | address)
            };
        } // end Header10
        
        public List<byte> Header11 (uint opCode, uint isStop, uint length, uint address) {
            return new List<byte>
            {
                Header(opCode, isStop, length),
                (byte)(i2c10Bit | (chipId & 6) | (opCode == (uint)I2C.Write ? i2cWrite : i2cRead))
            };
        } // end Header11

        public List<byte> CscDataWrite (uint adcSelect) {
            if (adcSelect == selectAdc12) {
                return new List<byte> 
                { 
                    (byte)((selectAdc12 | 16) << 1),                // 7'b1010000, 1'b0
                    26,                                             // 3'b000, 5'b11010
                    0, 0, 0, 0,                                     // 4 x 8'b00000000
                    (byte)((chipControl1.refControl >> 2) & 3),             // 6'b000000, Ref_cntrl[3:2]
                    (byte)(((chipControl1.refControl & 3) << 6) | chipControl1.pllControl) // Ref_cntrl[1:0], Pll
                };
            } else {
                return new List<byte> 
                { 
                    (byte)((selectAdc34 | 16) << 1),    // 7'b0110000, 1'b0
                    26,                                 // 3'b000, 5'b11010
                    0, 0, 0, 0, 0,                      // 5 x 8'b00000000
                    (byte)(chipControl1.slvsControl & 15)              // 4'b0000, Slvs
                };
            }
        } // end CscDataWrite

        public List<byte> CscDataRead (uint adcSelect) {
            return new List<byte> 
            { 
                (byte)((adcSelect | 16) << 1),  // 7'b1010000, 1'b0 or 7'b0110000, 1'b0
                10,                             // 3'b000, 5'b01010
                0, 0, 0, 0, 0, 0                // 6 x 8'b00000000
            };
        } // end CscDataRead

        public List<byte> AdcHeader (uint adc, uint other) {
            uint adcGroupSel = adc < 2 ? selectAdc12 : selectAdc34;
            uint adcNumber = adc < 2 ? adc + 1 : adc - 1; // adc is either 1 or 2

            return new List<byte> 
            {
                (byte)(((adcGroupSel | 16) << 1) | ((adcNumber >> 3) & 1)),     // 7'b1010000, ADC[3]
                (byte)(((adcNumber & 7) << 5) | (other & 31))                   // ADC[2:0], 5'b10110
            };
        } // end AdcHeader

        public List<byte> Repeat (uint count, byte value) {
            var data = new List<byte>();
            for (int i = 0; i < count; i++) data.Add(value);
            return data;
        } // end Repeat

        public void I2cInit () {
            var d = new List<byte>();
            //prescalers for 40MHz clock and 0.4MHz speed; prescaler value = clk/(5*i2c_clk) - 1
            d.Add(Header((uint) I2C.Init, (uint) I2C.NoStop, 3)); d.Add(0x13); d.Add(0x00); d.Add(0x80);
            //set the "10bit" addresing mode for Nevis13 ;
            d.AddRange(Header7((uint) I2C.Write, (uint) I2C.Stop, 1)); d.Add(0x20);
            //            for (int i = 0; i < (584 / 8); i++) d.Add(0);
            d.Add(0);
            DoFifoAOperation(1, d);

            // TODO: readback
            // readback what? I2C wishbone registers? chip config?
        } // end I2cInit
    }
}