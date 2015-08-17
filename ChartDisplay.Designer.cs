namespace Nevis14
{
    partial class ChartDisplay
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.signalTab = new System.Windows.Forms.TabPage();
            this.signalchart = new ChiptestChart(1);
            this.fftTab = new System.Windows.Forms.TabPage();
            this.fftchart = new ChiptestChart(2);
            this.voltageTab = new System.Windows.Forms.TabPage();
            this.voltagechart = new ChiptestChart(3);
            this.tabControl1.SuspendLayout();
            this.signalTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.signalchart)).BeginInit();
            this.fftTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fftchart)).BeginInit();
            this.voltageTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.voltagechart)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.signalTab);
            this.tabControl1.Controls.Add(this.fftTab);
            this.tabControl1.Controls.Add(this.voltageTab);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(494, 374);
            this.tabControl1.TabIndex = 0;
            // 
            // signalTab
            // 
            this.signalTab.BackColor = System.Drawing.SystemColors.Control;
            this.signalTab.Controls.Add(this.signalchart);
            this.signalTab.Location = new System.Drawing.Point(4, 22);
            this.signalTab.Name = "signalTab";
            this.signalTab.Padding = new System.Windows.Forms.Padding(3);
            this.signalTab.Size = new System.Drawing.Size(486, 348);
            this.signalTab.TabIndex = 0;
            this.signalTab.Text = "Signal";
            // 
            // signalchart
            // 
            this.signalchart.Location = new System.Drawing.Point(6, 6);
            this.signalchart.Name = "signalchart";
            this.signalchart.Size = new System.Drawing.Size(474, 336);
            this.signalchart.TabIndex = 1;
            // 
            // fftTab
            // 
            this.fftTab.BackColor = System.Drawing.SystemColors.Control;
            this.fftTab.Controls.Add(this.fftchart);
            this.fftTab.Location = new System.Drawing.Point(4, 22);
            this.fftTab.Name = "fftTab";
            this.fftTab.Padding = new System.Windows.Forms.Padding(3);
            this.fftTab.Size = new System.Drawing.Size(486, 348);
            this.fftTab.TabIndex = 1;
            this.fftTab.Text = "FFT";
            // 
            // fftchart
            // 
            this.fftchart.Location = new System.Drawing.Point(6, 6);
            this.fftchart.Name = "fftchart";
            this.fftchart.Size = new System.Drawing.Size(474, 336);
            this.fftchart.TabIndex = 1;
            // 
            // voltageTab
            // 
            this.voltageTab.Controls.Add(this.voltagechart);
            this.voltageTab.Location = new System.Drawing.Point(4, 22);
            this.voltageTab.Name = "voltageTab";
            this.voltageTab.Size = new System.Drawing.Size(486, 348);
            this.voltageTab.TabIndex = 0;
            this.voltageTab.Text = "Voltage Test";
            // 
            // voltagechart
            // 
            this.voltagechart.Location = new System.Drawing.Point(6, 6);
            this.voltagechart.Name = "voltagechart";
            this.voltagechart.Size = new System.Drawing.Size(474, 336);
            this.voltagechart.TabIndex = 1;
            // 
            // ChartDisplay
            // 
            //this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "ChartDisplay";
            this.Size = new System.Drawing.Size(500, 380);
            this.tabControl1.ResumeLayout(false);
            this.signalTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.signalchart)).EndInit();
            this.fftTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fftchart)).EndInit();
            this.voltageTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.voltagechart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage signalTab;
        private System.Windows.Forms.TabPage fftTab;
        private System.Windows.Forms.TabPage voltageTab;
        public ChiptestChart signalchart;
        public ChiptestChart voltagechart;
        public ChiptestChart fftchart;

    }
}