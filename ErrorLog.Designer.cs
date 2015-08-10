namespace Nevis14
{
    partial class ErrorLog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serialRadio = new System.Windows.Forms.RadioButton();
            this.nocalRadio = new System.Windows.Forms.RadioButton();
            this.signalRadio = new System.Windows.Forms.RadioButton();
            this.takedataRadio = new System.Windows.Forms.RadioButton();
            this.badcalRadio = new System.Windows.Forms.RadioButton();
            this.chipnumTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.notesTextBox = new System.Windows.Forms.TextBox();
            this.otherRadio = new System.Windows.Forms.RadioButton();
            this.otherTextBox = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.radioBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.powerTextBox = new System.Windows.Forms.TextBox();
            this.radioBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialRadio
            // 
            this.serialRadio.AutoSize = true;
            this.serialRadio.Location = new System.Drawing.Point(6, 9);
            this.serialRadio.Name = "serialRadio";
            this.serialRadio.Size = new System.Drawing.Size(130, 17);
            this.serialRadio.TabIndex = 0;
            this.serialRadio.TabStop = true;
            this.serialRadio.Text = "Chip does not serialize";
            this.serialRadio.UseVisualStyleBackColor = true;
            // 
            // nocalRadio
            // 
            this.nocalRadio.AutoSize = true;
            this.nocalRadio.Location = new System.Drawing.Point(6, 32);
            this.nocalRadio.Name = "nocalRadio";
            this.nocalRadio.Size = new System.Drawing.Size(133, 17);
            this.nocalRadio.TabIndex = 1;
            this.nocalRadio.TabStop = true;
            this.nocalRadio.Text = "Chip does not calibrate";
            this.nocalRadio.UseVisualStyleBackColor = true;
            // 
            // signalRadio
            // 
            this.signalRadio.AutoSize = true;
            this.signalRadio.Location = new System.Drawing.Point(6, 101);
            this.signalRadio.Name = "signalRadio";
            this.signalRadio.Size = new System.Drawing.Size(104, 17);
            this.signalRadio.TabIndex = 2;
            this.signalRadio.TabStop = true;
            this.signalRadio.Text = "Issue with Signal";
            this.signalRadio.UseVisualStyleBackColor = true;
            // 
            // takedataRadio
            // 
            this.takedataRadio.AutoSize = true;
            this.takedataRadio.Location = new System.Drawing.Point(6, 78);
            this.takedataRadio.Name = "takedataRadio";
            this.takedataRadio.Size = new System.Drawing.Size(109, 17);
            this.takedataRadio.TabIndex = 3;
            this.takedataRadio.TabStop = true;
            this.takedataRadio.Text = "Error Taking Data";
            this.takedataRadio.UseVisualStyleBackColor = true;
            // 
            // badcalRadio
            // 
            this.badcalRadio.AutoSize = true;
            this.badcalRadio.Location = new System.Drawing.Point(6, 55);
            this.badcalRadio.Name = "badcalRadio";
            this.badcalRadio.Size = new System.Drawing.Size(145, 17);
            this.badcalRadio.TabIndex = 4;
            this.badcalRadio.TabStop = true;
            this.badcalRadio.Text = "Chip calibrates incorrectly";
            this.badcalRadio.UseVisualStyleBackColor = true;
            // 
            // chipnumTextBox
            // 
            this.chipnumTextBox.Location = new System.Drawing.Point(90, 9);
            this.chipnumTextBox.Name = "chipnumTextBox";
            this.chipnumTextBox.Size = new System.Drawing.Size(100, 20);
            this.chipnumTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Defective Chip:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "What problem does the chip have?";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 474);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(454, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Note: chips with poor dynamic range or bad ENOB will automatically be processed a" +
    "s defective";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 334);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Notes:";
            // 
            // notesTextBox
            // 
            this.notesTextBox.Location = new System.Drawing.Point(19, 350);
            this.notesTextBox.Multiline = true;
            this.notesTextBox.Name = "notesTextBox";
            this.notesTextBox.Size = new System.Drawing.Size(447, 72);
            this.notesTextBox.TabIndex = 10;
            // 
            // otherRadio
            // 
            this.otherRadio.AutoSize = true;
            this.otherRadio.Location = new System.Drawing.Point(6, 124);
            this.otherRadio.Name = "otherRadio";
            this.otherRadio.Size = new System.Drawing.Size(54, 17);
            this.otherRadio.TabIndex = 11;
            this.otherRadio.TabStop = true;
            this.otherRadio.Text = "Other:";
            this.otherRadio.UseVisualStyleBackColor = true;
            // 
            // otherTextBox
            // 
            this.otherTextBox.Location = new System.Drawing.Point(25, 147);
            this.otherTextBox.Name = "otherTextBox";
            this.otherTextBox.Size = new System.Drawing.Size(169, 20);
            this.otherTextBox.TabIndex = 12;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(131, 428);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(91, 34);
            this.saveButton.TabIndex = 13;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(251, 428);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(91, 34);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // radioBox
            // 
            this.radioBox.Controls.Add(this.serialRadio);
            this.radioBox.Controls.Add(this.nocalRadio);
            this.radioBox.Controls.Add(this.badcalRadio);
            this.radioBox.Controls.Add(this.otherTextBox);
            this.radioBox.Controls.Add(this.takedataRadio);
            this.radioBox.Controls.Add(this.otherRadio);
            this.radioBox.Controls.Add(this.signalRadio);
            this.radioBox.Location = new System.Drawing.Point(41, 77);
            this.radioBox.Name = "radioBox";
            this.radioBox.Size = new System.Drawing.Size(200, 180);
            this.radioBox.TabIndex = 15;
            this.radioBox.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 270);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Power Draw:";
            // 
            // powerTextBox
            // 
            this.powerTextBox.Location = new System.Drawing.Point(19, 286);
            this.powerTextBox.Name = "powerTextBox";
            this.powerTextBox.Size = new System.Drawing.Size(64, 20);
            this.powerTextBox.TabIndex = 17;
            // 
            // ErrorLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 499);
            this.Controls.Add(this.powerTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.radioBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.notesTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chipnumTextBox);
            this.Name = "ErrorLog";
            this.Text = "ErrorLog";
            this.radioBox.ResumeLayout(false);
            this.radioBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton serialRadio;
        private System.Windows.Forms.RadioButton nocalRadio;
        private System.Windows.Forms.RadioButton signalRadio;
        private System.Windows.Forms.RadioButton takedataRadio;
        private System.Windows.Forms.RadioButton badcalRadio;
        private System.Windows.Forms.TextBox chipnumTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox notesTextBox;
        private System.Windows.Forms.RadioButton otherRadio;
        private System.Windows.Forms.TextBox otherTextBox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox radioBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox powerTextBox;
    }
}