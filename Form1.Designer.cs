namespace Nevis14 {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.connectButton = new System.Windows.Forms.Button();
            this.calibButton = new System.Windows.Forms.Button();
            this.dataButton = new System.Windows.Forms.Button();
            this.seeButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.commandBox = new System.Windows.Forms.TextBox();
            this.dataBox = new System.Windows.Forms.TextBox();
            this.chipNumBox = new System.Windows.Forms.MaskedTextBox();
            this.chipNumLabel = new System.Windows.Forms.Label();
            this.fftBox = new System.Windows.Forms.PictureBox();
            this.resultBox = new System.Windows.Forms.TextBox();
            this.chipControl1 = new Nevis14.ChipControl();
            this.fftButton = new System.Windows.Forms.Button();
            this.triggerButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.fftBox)).BeginInit();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(10, 40);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(85, 35);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // calibButton
            // 
            this.calibButton.Location = new System.Drawing.Point(100, 40);
            this.calibButton.Name = "calibButton";
            this.calibButton.Size = new System.Drawing.Size(85, 35);
            this.calibButton.TabIndex = 11;
            this.calibButton.Text = "Calibrate";
            this.calibButton.UseVisualStyleBackColor = true;
            this.calibButton.Click += new System.EventHandler(this.calibButton_Click);
            // 
            // dataButton
            // 
            this.dataButton.Location = new System.Drawing.Point(10, 80);
            this.dataButton.Name = "dataButton";
            this.dataButton.Size = new System.Drawing.Size(85, 35);
            this.dataButton.TabIndex = 12;
            this.dataButton.Text = "Take Data";
            this.dataButton.UseVisualStyleBackColor = true;
            this.dataButton.Click += new System.EventHandler(this.dataButton_Click);
            // 
            // seeButton
            // 
            this.seeButton.Location = new System.Drawing.Point(100, 80);
            this.seeButton.Name = "seeButton";
            this.seeButton.Size = new System.Drawing.Size(85, 35);
            this.seeButton.TabIndex = 13;
            this.seeButton.Text = "Run SEE";
            this.seeButton.UseVisualStyleBackColor = true;
            this.seeButton.Click += new System.EventHandler(this.seeButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(65, 200);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(70, 25);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // commandBox
            // 
            this.commandBox.Location = new System.Drawing.Point(605, 10);
            this.commandBox.Multiline = true;
            this.commandBox.Name = "commandBox";
            this.commandBox.ReadOnly = true;
            this.commandBox.Size = new System.Drawing.Size(385, 130);
            this.commandBox.TabIndex = 1;
            // 
            // dataBox
            // 
            this.dataBox.Location = new System.Drawing.Point(10, 360);
            this.dataBox.Multiline = true;
            this.dataBox.Name = "dataBox";
            this.dataBox.ReadOnly = true;
            this.dataBox.Size = new System.Drawing.Size(470, 380);
            this.dataBox.TabIndex = 5;
            // 
            // chipNumBox
            // 
            this.chipNumBox.Location = new System.Drawing.Point(105, 10);
            this.chipNumBox.Name = "chipNumBox";
            this.chipNumBox.Size = new System.Drawing.Size(85, 29);
            this.chipNumBox.TabIndex = 7;
            this.chipNumBox.ValidatingType = typeof(int);
            this.chipNumBox.TypeValidationCompleted += new System.Windows.Forms.TypeValidationEventHandler(this.chipNumBox_TypeValidationCompleted);
            // 
            // chipNumLabel
            // 
            this.chipNumLabel.Font = new System.Drawing.Font("Consolas", 9F);
            this.chipNumLabel.Location = new System.Drawing.Point(10, 10);
            this.chipNumLabel.Name = "chipNumLabel";
            this.chipNumLabel.Size = new System.Drawing.Size(90, 25);
            this.chipNumLabel.TabIndex = 8;
            this.chipNumLabel.Text = "Chip No:";
            this.chipNumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // fftBox
            // 
            this.fftBox.Location = new System.Drawing.Point(490, 360);
            this.fftBox.Name = "fftBox";
            this.fftBox.Size = new System.Drawing.Size(500, 380);
            this.fftBox.TabIndex = 9;
            this.fftBox.TabStop = false;
            // 
            // resultBox
            // 
            this.resultBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultBox.Location = new System.Drawing.Point(605, 150);
            this.resultBox.Multiline = true;
            this.resultBox.Name = "resultBox";
            this.resultBox.ReadOnly = true;
            this.resultBox.Size = new System.Drawing.Size(385, 205);
            this.resultBox.TabIndex = 10;
            // 
            // chipControl1
            // 
            this.chipControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.chipControl1.Location = new System.Drawing.Point(200, 10);
            this.chipControl1.Name = "chipControl1";
            this.chipControl1.pllControl = ((uint)(16u));
            this.chipControl1.refControl = ((uint)(0u));
            this.chipControl1.Size = new System.Drawing.Size(400, 135);
            this.chipControl1.slvsControl = ((uint)(0u));
            this.chipControl1.TabIndex = 3;
            // 
            // fftButton
            // 
            this.fftButton.Location = new System.Drawing.Point(100, 120);
            this.fftButton.Name = "fftButton";
            this.fftButton.Size = new System.Drawing.Size(85, 35);
            this.fftButton.TabIndex = 17;
            this.fftButton.Text = "FFT";
            this.fftButton.UseVisualStyleBackColor = true;
            this.fftButton.Click += new System.EventHandler(this.fftButton_Click);
            // 
            // triggerButton
            // 
            this.triggerButton.Location = new System.Drawing.Point(10, 120);
            this.triggerButton.Name = "triggerButton";
            this.triggerButton.Size = new System.Drawing.Size(85, 35);
            this.triggerButton.TabIndex = 16;
            this.triggerButton.Text = "Trigger";
            this.triggerButton.UseVisualStyleBackColor = true;
            this.triggerButton.Click += new System.EventHandler(this.triggerButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(100, 160);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(85, 35);
            this.button3.TabIndex = 15;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(10, 160);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(85, 35);
            this.button4.TabIndex = 14;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 750);
            this.Controls.Add(this.fftButton);
            this.Controls.Add(this.triggerButton);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.seeButton);
            this.Controls.Add(this.dataButton);
            this.Controls.Add(this.calibButton);
            this.Controls.Add(this.resultBox);
            this.Controls.Add(this.fftBox);
            this.Controls.Add(this.chipNumLabel);
            this.Controls.Add(this.chipNumBox);
            this.Controls.Add(this.dataBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.chipControl1);
            this.Controls.Add(this.commandBox);
            this.Controls.Add(this.connectButton);
            this.Font = new System.Drawing.Font("Consolas", 9F);
            this.Name = "Form1";
            this.Text = "Nevis14 Test Interface";
            ((System.ComponentModel.ISupportInitialize)(this.fftBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button calibButton;
        private System.Windows.Forms.Button dataButton;
        private System.Windows.Forms.Button seeButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox commandBox;
        private ChipControl chipControl1;
        private System.Windows.Forms.TextBox dataBox;
        private System.Windows.Forms.MaskedTextBox chipNumBox;
        private System.Windows.Forms.Label chipNumLabel;
        private System.Windows.Forms.PictureBox fftBox;
        private System.Windows.Forms.TextBox resultBox;
        private System.Windows.Forms.Button fftButton;
        private System.Windows.Forms.Button triggerButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}

