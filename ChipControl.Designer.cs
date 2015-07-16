namespace Nevis14 {
    partial class ChipControl {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent () {
            this.mdacControls = new MdacControl[4] {
                new MdacControl(1),
                new MdacControl(2),
                new MdacControl(3),
                new MdacControl(4)
            };
            this.dacButtons = new System.Windows.Forms.Button[2] {
                new System.Windows.Forms.Button(),
                new System.Windows.Forms.Button()
            };
            this.adcs = new AdcControl[4] {
                new AdcControl(1),
                new AdcControl(2),
                new AdcControl(3),
                new AdcControl(4)
            };
            this.SuspendLayout();
            // 
            // mdac1
            // 
            this.mdacControls[0].Location = new System.Drawing.Point(80, 45);
            this.mdacControls[0].Name = "mdac1";
            this.mdacControls[0].Size = new System.Drawing.Size(80, 90);
            this.mdacControls[0].TabIndex = 4;
            // 
            // mdac2
            // 
            this.mdacControls[1].Location = new System.Drawing.Point(160, 45);
            this.mdacControls[1].Name = "mdac2";
            this.mdacControls[1].Size = new System.Drawing.Size(80, 90);
            this.mdacControls[1].TabIndex = 5;
            // 
            // mdac3
            // 
            this.mdacControls[2].Location = new System.Drawing.Point(240, 45);
            this.mdacControls[2].Name = "mdac3";
            this.mdacControls[2].Size = new System.Drawing.Size(80, 90);
            this.mdacControls[2].TabIndex = 6;
            // 
            // mdac4
            // 
            this.mdacControls[3].Location = new System.Drawing.Point(320, 45);
            this.mdacControls[3].Name = "mdac4";
            this.mdacControls[3].Size = new System.Drawing.Size(80, 90);
            this.mdacControls[3].TabIndex = 7;
            // 
            // dac1Button
            // 
            this.dacButtons[0].Location = new System.Drawing.Point(10, 25);
            this.dacButtons[0].Name = "dac1Button";
            this.dacButtons[0].Size = new System.Drawing.Size(65, 35);
            this.dacButtons[0].TabIndex = 8;
            this.dacButtons[0].Text = "DAC1";
            this.dacButtons[0].UseVisualStyleBackColor = true;
            // 
            // dac2Button
            // 
            this.dacButtons[1].Location = new System.Drawing.Point(10, 90);
            this.dacButtons[1].Name = "dac2Button";
            this.dacButtons[1].Size = new System.Drawing.Size(65, 35);
            this.dacButtons[1].TabIndex = 9;
            this.dacButtons[1].Text = "DAC2";
            this.dacButtons[1].UseVisualStyleBackColor = true;
            // 
            // adc1Button
            // 
            this.adcs[0].Location = new System.Drawing.Point(85, 5);
            this.adcs[0].Name = "adc1Button";
            this.adcs[0].Size = new System.Drawing.Size(70, 35);
            this.adcs[0].TabIndex = 0;
            // 
            // adc2Button
            // 
            this.adcs[1].Location = new System.Drawing.Point(165, 5);
            this.adcs[1].Name = "adc2Button";
            this.adcs[1].Size = new System.Drawing.Size(70, 35);
            this.adcs[1].TabIndex = 1;
            // 
            // adc3Button
            // 
            this.adcs[2].Location = new System.Drawing.Point(245, 5);
            this.adcs[2].Name = "adc3Button";
            this.adcs[2].Size = new System.Drawing.Size(70, 35);
            this.adcs[2].TabIndex = 2;
            // 
            // adc4Button
            // 
            this.adcs[3].Location = new System.Drawing.Point(325, 5);
            this.adcs[3].Name = "adc4Button";
            this.adcs[3].Size = new System.Drawing.Size(70, 35);
            this.adcs[3].TabIndex = 3;
            //
            // ChipControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segue UI", 10F);
            this.Controls.Add(this.adcs[0]);
            this.Controls.Add(this.adcs[1]);
            this.Controls.Add(this.adcs[2]);
            this.Controls.Add(this.adcs[3]);
            this.Controls.Add(this.dacButtons[0]);
            this.Controls.Add(this.dacButtons[1]);
            this.Controls.Add(this.mdacControls[0]);
            this.Controls.Add(this.mdacControls[1]);
            this.Controls.Add(this.mdacControls[2]);
            this.Controls.Add(this.mdacControls[3]);
            this.Name = "ChipControl";
            this.Size = new System.Drawing.Size(400, 135);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button[] dacButtons;
        private MdacControl[] mdacControls;
        public AdcControl[] adcs;
    }

    partial class AdcControl {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent () {
            this.adcButton = new System.Windows.Forms.Button();
            // 
            // adcButton
            // 
            this.adcButton.Location = new System.Drawing.Point(0, 0);
            this.adcButton.Name = "adcButton";
            this.adcButton.Size = new System.Drawing.Size(70, 35);
            this.adcButton.TabIndex = 0;
            this.adcButton.Text = "ADC"+_id;
            this.adcButton.UseVisualStyleBackColor = true;
            // 
            // AdcControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.adcButton);
            this.Name = "AdcControl";
            this.Size = new System.Drawing.Size(70, 35);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button adcButton;
    }

    partial class MdacControl {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent () {
            this.label1 = new System.Windows.Forms.Label();
            this.cal2Button = new System.Windows.Forms.Button();
            this.cal1Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = false;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "MDAC"+_id;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cal2Button
            // 
            this.cal2Button.Location = new System.Drawing.Point(5, 60);
            this.cal2Button.Name = "cal2Button";
            this.cal2Button.Size = new System.Drawing.Size(70, 25);
            this.cal2Button.TabIndex = 2;
            this.cal2Button.Text = "Cal2";
            this.cal2Button.UseVisualStyleBackColor = true;
            // 
            // cal1Button
            // 
            this.cal1Button.Location = new System.Drawing.Point(5, 30);
            this.cal1Button.Name = "cal1Button";
            this.cal1Button.Size = new System.Drawing.Size(70, 25);
            this.cal1Button.TabIndex = 1;
            this.cal1Button.Text = "Cal1";
            this.cal1Button.UseVisualStyleBackColor = true;
            // 
            // MdacControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cal2Button);
            this.Controls.Add(this.cal1Button);
            this.Name = "MdacControl";
            this.Size = new System.Drawing.Size(80, 90);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cal1Button;
        private System.Windows.Forms.Button cal2Button;
    }
}