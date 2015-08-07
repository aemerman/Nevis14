namespace Nevis14 {
    partial class FunctionGenerator {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.freqBox = new System.Windows.Forms.MaskedTextBox();
            this.ampBox = new System.Windows.Forms.MaskedTextBox();
            this.freqLabel = new System.Windows.Forms.Label();
            this.ampLabel = new System.Windows.Forms.Label();
            this.controlLabel = new System.Windows.Forms.Label();
            this.onButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // freqBox
            // 
            this.freqBox.Location = new System.Drawing.Point(5, 95);
            this.freqBox.Name = "freqBox";
            this.freqBox.Size = new System.Drawing.Size(100, 25);
            this.freqBox.TabIndex = 0;
            this.freqBox.ValidatingType = typeof(double);
            this.freqBox.TypeValidationCompleted += new System.Windows.Forms.TypeValidationEventHandler(ValidationCompleted);
            this.freqBox.Text = "5006510";
            // 
            // ampBox
            // 
            this.ampBox.Location = new System.Drawing.Point(110, 95);
            this.ampBox.Name = "ampBox";
            this.ampBox.Size = new System.Drawing.Size(100, 25);
            this.ampBox.TabIndex = 1;
            this.ampBox.ValidatingType = typeof(double);
            this.ampBox.TypeValidationCompleted += new System.Windows.Forms.TypeValidationEventHandler(ValidationCompleted);
            this.ampBox.Text = "10.0";
            // 
            // freqLabel
            // 
            this.freqLabel.AutoSize = true;
            this.freqLabel.Location = new System.Drawing.Point(5, 70);
            this.freqLabel.Name = "freqLabel";
            this.freqLabel.Size = new System.Drawing.Size(60, 25);
            this.freqLabel.TabIndex = 2;
            this.freqLabel.Text = "Frequency";
            // 
            // ampLabel
            // 
            this.ampLabel.AutoSize = true;
            this.ampLabel.Location = new System.Drawing.Point(110, 70);
            this.ampLabel.Name = "ampLabel";
            this.ampLabel.Size = new System.Drawing.Size(60, 25);
            this.ampLabel.TabIndex = 3;
            this.ampLabel.Text = "Amplitude";
            // 
            // controlLabel
            // 
            this.controlLabel.AutoSize = true;
            this.controlLabel.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
            this.controlLabel.Location = new System.Drawing.Point(10, 5);
            this.controlLabel.Name = "controlLabel";
            this.controlLabel.Size = new System.Drawing.Size(100, 30);
            this.controlLabel.TabIndex = 4;
            this.controlLabel.Text = "Function Generator";
            // 
            // onButton
            // 
            this.onButton.Location = new System.Drawing.Point(80, 40);
            this.onButton.Name = "onButton";
            this.onButton.Size = new System.Drawing.Size(60, 25);
            this.onButton.TabIndex = 5;
            this.onButton.Text = "Off";
            this.onButton.UseVisualStyleBackColor = true;
            this.onButton.Click += new System.EventHandler(onButton_Click);
            // 
            // FunctionGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Consolas", 9F);
            this.Controls.Add(this.onButton);
            this.Controls.Add(this.controlLabel);
            this.Controls.Add(this.ampLabel);
            this.Controls.Add(this.freqLabel);
            this.Controls.Add(this.ampBox);
            this.Controls.Add(this.freqBox);
            this.Name = "FunctionGenerator";
            this.Size = new System.Drawing.Size(215, 125);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MaskedTextBox freqBox;
        private System.Windows.Forms.MaskedTextBox ampBox;
        private System.Windows.Forms.Label freqLabel;
        private System.Windows.Forms.Label ampLabel;
        private System.Windows.Forms.Label controlLabel;
        private System.Windows.Forms.Button onButton;
    }
}
