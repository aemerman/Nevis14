using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace Nevis14
{
    public partial class ChartDisplay : UserControl
    {
        public ChartDisplay()
        {
            InitializeComponent();
        }
    }

    public class ChiptestChart : Chart
    {
        private double sampFreq = 40.0;
        private int charttype; // 1 = signal, 2 = FFT, 3 = voltage test
        private int numchannels;

        public ChiptestChart(int type)
        {
            charttype = type;
            if (charttype == 3) { numchannels = 1; Initialize(1); }
            else { numchannels = 4; Initialize(); }
        }

        public void Initialize(int numchannels = 4)
        {
            //this.Size = new Size(1000, 1000);
            this.Titles.Add(DateTime.Now.ToString());
            for (int isig = 0; isig < numchannels; isig++)
            {
                this.ChartAreas.Add(new ChartArea());
                Title atlastitle = new Title
                {
                    Text = "ATLAS Upgrade",
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold | FontStyle.Italic),
                    DockedToChartArea = this.ChartAreas[isig].Name,
                    Alignment = ContentAlignment.TopRight
                };
                var dataseries = new Series
                {
                    Color = Color.Red,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = true,
                    MarkerStyle = MarkerStyle.Square,
                    MarkerColor = Color.Red,
                    MarkerBorderWidth = 0,
                    ChartArea = ChartAreas[isig].Name
                };
                dataseries.Points.Clear();
                this.Series.Add(dataseries);
                this.Titles.Add(atlastitle);
            }
        }

        public void SetData(double[][] vals)
        {
            for (int channel = 0; channel < numchannels; channel++)
                for (int i = 0; i < vals[channel].Length; i++)
                    this.Series[channel].Points.Add(vals[channel][i]);
        }

        public void SetData(Dictionary<double, double>[] data)
        {
            for (int channel = 0; channel < numchannels; channel++)
            {
                foreach (KeyValuePair<double, double> dictentry in data[channel])
                {
                    this.Series[channel].Points.Add(new DataPoint(dictentry.Key, dictentry.Value));
                }
                //double[] xvals = data[channel].Keys.ToArray<double>();
                //for (int i = 0; i < xvals.Length; i++)
                //    this.Series[channel].Points.AddXY(xvals[i], data[channel][xvals[i]]);
            }
        }

        public void Reset()
        {
            this.ChartAreas.Clear();
            this.Series.Clear();
            this.Titles.Clear();
            this.Legends.Clear();
            this.Initialize(numchannels);
        }

        public void Format(int chipnum)
        {
            Font axisFont = new Font(FontFamily.GenericSansSerif, 10);
            Font idFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Italic);
            //  ----- Signal Formatting -----
            if (charttype == 1)
            {
                for (int isig = 0; isig < 4; isig++)
                {
                    this.Series[isig].ChartType = SeriesChartType.Point;

                    this.ChartAreas[isig].AxisY.Minimum = 0;
                    this.ChartAreas[isig].AxisY.Maximum = 4000;
                    this.ChartAreas[isig].AxisX.MajorGrid.Enabled = false;
                    this.ChartAreas[isig].AxisY.MajorGrid.Enabled = false;
                    this.ChartAreas[isig].AxisX.Title = "Samples (40 MHz, 1 mV signal)";
                    this.ChartAreas[isig].AxisY.Title = "ADC Counts";
                    this.ChartAreas[isig].AxisX.TitleFont = axisFont;
                    this.ChartAreas[isig].AxisY.TitleFont = axisFont;
                }
            }   //  End of Signal Formatting

            //  ----- FFT Formatting -----
            else if (charttype == 2)
            {
                for (int isig = 0; isig < 4; isig++)
                {

                    this.Series[isig].ChartType = SeriesChartType.Line;

                    this.ChartAreas[isig].BorderWidth = 2;
                    this.ChartAreas[isig].BorderColor = Color.Black;
                    this.ChartAreas[isig].BorderDashStyle = ChartDashStyle.Solid;
                    this.ChartAreas[isig].AxisX.Title = "Freq. [MHz]";
                    this.ChartAreas[isig].AxisX.TitleAlignment = StringAlignment.Far;
                    this.ChartAreas[isig].AxisX.TitleFont = axisFont;
                    this.ChartAreas[isig].AxisX.Minimum = 0;
                    this.ChartAreas[isig].AxisX.Maximum = this.Series[isig].Points.Count / 2 * 1.1;
                    this.ChartAreas[isig].AxisY.Title = "20 log\x2081\x2080(|fft|/|max|) [dB]";
                    this.ChartAreas[isig].AxisY.TitleAlignment = StringAlignment.Far;
                    this.ChartAreas[isig].AxisY.TitleFont = axisFont;
                    this.ChartAreas[isig].AxisY.Minimum = -140;
                    this.ChartAreas[isig].AxisY.Maximum = 10;
                    double gridwidth = this.Series[isig].Points.Count / 10;
                    for (int i = 0; i < 11; i++)
                    {
                        // Make the X-axis labels actually correspond to frequency
                        this.ChartAreas[isig].AxisX.CustomLabels.Add(new CustomLabel());
                        this.ChartAreas[isig].AxisX.CustomLabels[i].FromPosition = gridwidth * i;
                        this.ChartAreas[isig].AxisX.CustomLabels[i].RowIndex = 0;
                        this.ChartAreas[isig].AxisX.CustomLabels[i].Text = (2 * i).ToString();
                        this.ChartAreas[isig].AxisX.CustomLabels[i].GridTicks = GridTickTypes.None;
                        this.ChartAreas[isig].AxisX.LabelStyle.Angle = 0;
                    }
                    this.ChartAreas[isig].AxisX.MajorTickMark.Interval
                        = this.ChartAreas[isig].AxisX2.MajorTickMark.Interval
                        = this.Series[isig].Points.Count * 20 / sampFreq / 10;
                    this.ChartAreas[isig].AxisX.MinorTickMark.Interval
                        = this.ChartAreas[isig].AxisX2.MinorTickMark.Interval
                        = this.Series[isig].Points.Count * 20 / sampFreq / 10 / 4;
                    this.ChartAreas[isig].AxisX.MinorTickMark.Enabled
                        = this.ChartAreas[isig].AxisX2.MinorTickMark.Enabled
                        = true;
                    this.ChartAreas[isig].AxisX.MinorTickMark.Size
                        = this.ChartAreas[isig].AxisX2.MinorTickMark.Size
                        = this.ChartAreas[isig].AxisX.MajorTickMark.Size / 2;
                    this.ChartAreas[isig].AxisX.MajorGrid.Enabled
                        = this.ChartAreas[isig].AxisX2.MajorGrid.Enabled
                        = false;
                    this.ChartAreas[isig].AxisX2.MajorTickMark.TickMarkStyle
                        = this.ChartAreas[isig].AxisX2.MinorTickMark.TickMarkStyle
                        = TickMarkStyle.InsideArea;
                    this.ChartAreas[isig].AxisX2.LabelStyle.Enabled = false;
                    this.ChartAreas[isig].AxisX2.Enabled = AxisEnabled.True;
                    this.ChartAreas[isig].AxisY.MajorTickMark.Interval
                        = this.ChartAreas[isig].AxisY.Interval
                        = this.ChartAreas[isig].AxisY2.MajorTickMark.Interval
                        = 20;
                    this.ChartAreas[isig].AxisY.MinorTickMark.Interval
                        = this.ChartAreas[isig].AxisY2.MinorTickMark.Interval
                        = 5;
                    this.ChartAreas[isig].AxisY.MinorTickMark.Enabled
                        = this.ChartAreas[isig].AxisY2.MinorTickMark.Enabled
                        = true;
                    this.ChartAreas[isig].AxisY.MinorTickMark.Size
                        = this.ChartAreas[isig].AxisY2.MinorTickMark.Size
                        = this.ChartAreas[isig].AxisY.MajorTickMark.Size / 2;
                    this.ChartAreas[isig].AxisY.MajorGrid.Enabled
                        = this.ChartAreas[isig].AxisY2.MajorGrid.Enabled
                        = false;
                    this.ChartAreas[isig].AxisY2.MajorTickMark.TickMarkStyle
                        = this.ChartAreas[isig].AxisY2.MinorTickMark.TickMarkStyle
                        = TickMarkStyle.InsideArea;
                    this.ChartAreas[isig].AxisY2.LabelStyle.Enabled = false;
                    this.ChartAreas[isig].AxisY2.Enabled = AxisEnabled.True;

                    // Add ID information to the chart
                    var fftID = new Legend
                    {
                        BackColor = Color.Transparent,
                        Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Left,
                        InsideChartArea = this.ChartAreas[isig].Name,
                        LegendStyle = LegendStyle.Column,
                        Name = "ID Legend " + (isig + 1)
                    };

                    for (int i = 0; i < 4; i++)
                        fftID.CustomItems.Add(new LegendItem());
                    fftID.CustomItems[0].Cells.Add(new LegendCell(""));
                    fftID.CustomItems[1].Cells.Add(new LegendCell("Nevis14"));
                    fftID.CustomItems[2].Cells.Add(new LegendCell(String.Format("Chip {0}", chipnum)));
                    fftID.CustomItems[3].Cells.Add(new LegendCell(String.Format("Ch. {0}", isig + 1)));
                    for (int i = 1; i <= 3; i++)
                    {
                        fftID.CustomItems[i].Cells[0].Font = idFont;
                        fftID.CustomItems[i].Cells[0].Alignment = ContentAlignment.MiddleLeft;
                    }

                    this.Legends.Add(fftID);
                }
            }   //  End of FFT Formatting

            //  ----- Voltage Test Formatting -----
            else if (charttype == 3)
            {
                this.ChartAreas[0].AxisX.Minimum = 0;
                this.ChartAreas[0].AxisX.Maximum = this.Series[0].Points.Count;
                this.ChartAreas[0].AxisX.Title = "Signal Amplitude [V]";
                this.ChartAreas[0].AxisY.Minimum = 9.5;
                this.ChartAreas[0].AxisY.Maximum = 10.5;
                this.ChartAreas[0].AxisY.Title = "ENOB";
                this.ChartAreas[0].AxisX.TitleFont = axisFont;
                this.ChartAreas[0].AxisY.TitleFont = axisFont;
                this.ChartAreas[0].Name = "main";

                this.Series[0].ChartType = SeriesChartType.Point;

                double gridwidth = this.Series[0].Points.Count / 10;
                double ampmin = this.Series[0].Points[0].XValue;
                double ampmax = this.Series[0].Points[(int)(gridwidth * 10) - 1].XValue;
                double gridwidth_in_volts = Math.Round((ampmax - ampmin) / 10, 2);
                for (int i = 0; i < 11; i++)
                {
                    // Make the X-axis labels actually correspond to frequency
                    this.ChartAreas[0].AxisX.CustomLabels.Add(new CustomLabel());

                    if (i == 0) this.ChartAreas[0].AxisX.CustomLabels[i].FromPosition = 1;
                    else this.ChartAreas[0].AxisX.CustomLabels[i].FromPosition = gridwidth * i * 2;

                    this.ChartAreas[0].AxisX.CustomLabels[i].RowIndex = 0;
                    this.ChartAreas[0].AxisX.CustomLabels[i].Text = (ampmin + gridwidth_in_volts * i).ToString();
                    this.ChartAreas[0].AxisX.CustomLabels[i].GridTicks = GridTickTypes.None;
                    this.ChartAreas[0].AxisX.LabelStyle.Angle = 0;
                }

                this.ChartAreas[0].AxisX.MajorTickMark.Interval
                    = this.ChartAreas[0].AxisX.Interval 
                    = gridwidth;
                /*this.ChartAreas[0].AxisX.MinorTickMark.Interval 
                    = gridwidth / 4;
                this.ChartAreas[0].AxisX.MinorTickMark.Enabled 
                    = true;
                this.ChartAreas[0].AxisX.MinorTickMark.Size 
                    = this.ChartAreas[0].AxisX.MajorTickMark.Size / 2;*/
                this.ChartAreas[0].AxisY.MajorTickMark.Interval
                    = this.ChartAreas[0].AxisY.Interval
                    = 0.1;
                /*this.ChartAreas[0].AxisY.MinorTickMark.Interval
                    = 0.025;
                this.ChartAreas[0].AxisY.MinorTickMark.Enabled
                    = true;
                this.ChartAreas[0].AxisY.MinorTickMark.Size
                    = this.ChartAreas[0].AxisY.MajorTickMark.Size / 2;*/

                var voltID = new Legend
                {
                    BackColor = Color.Transparent,
                    Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Left,
                    InsideChartArea = this.ChartAreas[0].Name,
                    LegendStyle = LegendStyle.Column,
                    Name = "ID Legend"
                };

                for (int i = 0; i < 3; i++)
                    voltID.CustomItems.Add(new LegendItem());
                voltID.CustomItems[0].Cells.Add(new LegendCell(""));
                voltID.CustomItems[1].Cells.Add(new LegendCell("Nevis14"));
                voltID.CustomItems[2].Cells.Add(new LegendCell(String.Format("Chip {0}", chipnum)));
                for (int i = 1; i <= 2; i++)
                {
                    voltID.CustomItems[i].Cells[0].Font = idFont;
                    voltID.CustomItems[i].Cells[0].Alignment = ContentAlignment.MiddleLeft;
                }

                //this.Legends.Add(voltID);
            }   //  End of Voltage Test Formatting
            this.Invalidate();
        } //  End Format

        public void AddLegends(Legend[] legends)
        {
            for (int isig = 0; isig < numchannels; isig++)
            {
                legends[isig].InsideChartArea = this.ChartAreas[isig].Name;
                this.Legends.Add(legends[isig]);
            }
                
        }

        public void Save(string filePath)
        {
            Size guisize = new Size(this.Size.Width, this.Size.Height);
            this.Size = new Size(1000, 1000);
            switch (charttype)
            {
                case 1:
                    SaveImage(filePath + "signal.png", ChartImageFormat.Png);
                    break;
                case 2:
                    SaveImage(filePath + "fft.png", ChartImageFormat.Png);
                    break;
                case 3:
                    SaveImage(filePath + "ENOB_vs_amplitude.png", ChartImageFormat.Png);
                    break;
            }
            this.Size = guisize;
        }
    }
}
