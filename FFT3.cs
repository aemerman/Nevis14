using FFTWSharp;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Nevis14 {
    public partial class Form1 : Form {
        // Chip data constants

        const int sampLength = 12288; //Number of data points used
        const string chipyear = "14";
        double sampFreq;     //Sampling Frequency
        double freq;
        int numchannels = 4;
        //Chart chart1;

        public struct AdcData {
            public double enob;
            public double sfdr;
            public double sinad;
            public double sinadNoHarm;
            public string[] outFreq;
            public double[] fourierdata;

            public string Print () {
                string s = Math.Round(enob, 4) + ", " 
                    + Math.Round(sfdr,4) + ", " 
                    + Math.Round(sinad,4) + ", " 
                    + Math.Round(sinadNoHarm,4);
                for (int i = 0; i <= 4; i++) {
                    s += ", " + outFreq[i];
                }
                return s;
            }
        } // End struct AdcData


        public AdcData[] FFT3(double infreq, int channels = 4)
        {
            numchannels = channels;
            freq = infreq;
            int chipnum = Convert.ToInt32(chipNumBox.Text);
            double[][] signalHisto; // For populating our signal histogram
            double[][] fourierHisto = new double[4][]; // To put the FFT in
            double[][] chartdata = new double[4][];

            //----Setting Sampling Frequency----//

            switch ((int)(5 * freq))      //To bypass C# rules about switching doubles
            {
                case 25:
                    sampFreq = 40; freq = 5.0065104167; break;
                case 90:
                    sampFreq = 40.113166485310122; break;
                case 50:
                    sampFreq = 40; freq = 9.967447917; break;
                case 10:
                    sampFreq = 40.0000000667; freq = 1.9986979167; break;
                case 1:
                    sampFreq = 40.0; break;
                default:
                    sampFreq = 40.0; break;
            }

            signalHisto = ReadData();

            //---- Start to Take FFTs----//
            AdcData[] adcData = new AdcData[4];
            for (int i = 0; i < 4; i++) adcData[i].outFreq = new string[5] { "", "", "", "", "" };

            // Start doing FFTs for each channel
            for (int isig = 0; isig < numchannels; isig++)
            {
                fourierHisto[isig] = DoFFT(signalHisto[isig]);
                adcData[isig] = DoQACalculations(fourierHisto[isig]);
                chartdata[isig] = adcData[isig].fourierdata;
            }   //End FFT

            Legend[] qalegend = CreateLegend(adcData);

            chartdisplay.Update(() =>
            {
                chartdisplay.fftchart.SetData(chartdata);
                chartdisplay.fftchart.Format(chipnum);
                chartdisplay.fftchart.AddLegends(qalegend);
                chartdisplay.fftchart.Save(filePath);
                chartdisplay.tabControl1.SelectTab("fftTab");
            });


            return adcData;
        }
        
        private Legend[] CreateLegend(AdcData[] adcData)
        {
            Legend[] qaInfo = new Legend[4];              
            // Add QA information to the chart area
            for (int isig = 0; isig < numchannels; isig++)
            {
                qaInfo[isig] = new Legend
                {
                    BackColor = Color.Transparent,
                    LegendStyle = LegendStyle.Column,
                    Name = "QA Legend " + (isig + 1)
                };
                for (int i = 0; i < 11; i++)
                    qaInfo[isig].CustomItems.Add(new LegendItem());

                qaInfo[isig].CustomItems[0].Cells.Add(new LegendCell(""));
                qaInfo[isig].CustomItems[1].Cells.Add(new LegendCell(String.Format("SFDR: {0:F2}", adcData[isig].sfdr)));
                qaInfo[isig].CustomItems[2].Cells.Add(new LegendCell(String.Format("SINAD: {0:F2}", adcData[isig].sinad)));
                qaInfo[isig].CustomItems[3].Cells.Add(new LegendCell(String.Format("SNR: {0:F2}", adcData[isig].sinadNoHarm)));
                qaInfo[isig].CustomItems[4].Cells.Add(new LegendCell(String.Format("ENOB: {0:F2}", adcData[isig].enob)));
                qaInfo[isig].CustomItems[5].Cells.Add(new LegendCell("Spur Freq. [MHz]:"));
                for (int i = 6; i < 11; i++)
                {
                    qaInfo[isig].CustomItems[i].Cells.Add(new LegendCell(String.Format("{0:F2}", adcData[isig].outFreq[i - 6])));
                }
                Font infoFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Italic);
                for (int i = 0; i < 11; i++)
                {
                    qaInfo[isig].CustomItems[i].Cells[0].Font = infoFont;
                    qaInfo[isig].CustomItems[i].Cells[0].Alignment = ContentAlignment.MiddleLeft;
                }
            }
            return qaInfo;
        }
        /*private void InitializeChart()
        {
            ResetChart(chart1);
            chart1.Titles.Add(DateTime.Now.ToString());

            for (int isig = 0; isig < numchannels; isig++)
            {
                chart1.ChartAreas.Add(new ChartArea());
                Title atlastitle = new Title
                {
                    Text = "ATLAS Upgrade",
                    Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold | FontStyle.Italic),
                    DockedToChartArea = chart1.ChartAreas[isig].Name,
                    Alignment = ContentAlignment.TopRight
                };
                var seriesFFT = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Color = Color.Red,
                    IsVisibleInLegend = false,
                    IsXValueIndexed = true,
                    MarkerStyle = MarkerStyle.Square,
                    MarkerColor = Color.Red,
                    MarkerBorderWidth = 0,
                    ChartArea = chart1.ChartAreas[isig].Name
                };
                chart1.Series.Add(seriesFFT);
                chart1.Titles.Add(atlastitle);
            }
        }*/

        private double[][] ReadData()
        {
            double[][] signalHisto = new double[4][];
            for (int isig = 0; isig < numchannels; isig++)
                signalHisto[isig] = new double[sampLength];

            //----Fill the histogram----//
            // We want to skip the first 20 entries incase something weird happens.
            // We want to fill the histrogram bins from the beginning
            // so we have an offset from the loop counter.
            for (int sampNum = 21; sampNum <= sampLength + 20; sampNum++)
                for (int isig = 0; isig < numchannels; isig++)
                    signalHisto[isig][sampNum - 20 - 1] = signals[isig][sampNum];

            return signalHisto;
        }
        
        private double[] DoFFT(double[] signalHisto)
        {

            double[] fourierHisto = new double[sampLength/2];

            //  --- Unmanaged FFT ---
            /*fftw_complexarray fourierComplex = new fftw_complexarray(sampLength);
            GCHandle fin = GCHandle.Alloc(signalHisto, GCHandleType.Pinned);
            IntPtr fout = fourierComplex.Handle;

            //inittime = timer.ElapsedTicks;
            
            
            IntPtr plan = fftw.dft_r2c(1, new int[] { sampLength }, fin.AddrOfPinnedObject(), fout, fftw_flags.Estimate);
            fftw.execute(plan);*/
            //  End Unmanaged FFT

            //  --- Managed FFT ---
            
            fftw_complexarray signalComplex = new fftw_complexarray(signalHisto);   // Make input complexarray from signalHisto
            fftw_complexarray fourierComplex = new fftw_complexarray(sampLength * 2);   // Make new output complexarray
            fftw_plan mplan = fftw_plan.dft_r2c_1d(sampLength, signalComplex, fourierComplex, fftw_flags.Estimate);
            mplan.Execute();
             
            //  End Managed FFT

            double[] fourierOut = fourierComplex.GetData_double();

            double re, im;
            for (int i = 0; i < sampLength / 2; i++)
            {
                re = fourierOut[2 * i];
                im = fourierOut[2 * i + 1];
                fourierHisto[i] = Math.Sqrt(re * re + im * im);
            }
            return fourierHisto;
        }

        public AdcData DoQACalculations(double[] fourierHisto)
        {
            int numPoints = 0;      //Keeping track of pts in FFT
            int ithData = 0;        //For help looping through FFT
            int numHarm = -1;       //Counting how many harmonics found

            Dictionary<double, double> theData = new Dictionary<double, double>();  //Keeping track of level of each frequency

            double binCont;         // Help with filling FFT
            double sum2 = 0.0;           // For Calculating SINAD
            double bin1, binMax = 0.0;    // For Manipulating output of FFT
            double[] aHarmFreq = new double[150];   //Aliased Harmonics
            double fourierFreq, tempFreq = 0;  //Help with looping through dictionary
            double noDistSum2 = 0;

            AdcData qadata = new AdcData();
            qadata.outFreq = new string[5] { "", "", "", "", "" };
            qadata.fourierdata = new double[sampLength / 2];


            //----Find the Second Largest Value----// 
            bin1 = fourierHisto[0];         // bin1 has the largest value, however
            fourierHisto[0] = 0;            // that isn't actually part of our data.
            binMax = fourierHisto.Max();        // The max value, our signal, is actually
            fourierHisto[0] = bin1;             // the second largest value, now in binMax.


            //chart1.Series[channel].Points.AddXY(0, 20 * Math.Log10(Math.Abs(fourierHisto[1]) / Math.Abs(binMax))); // Only added to make plot look nicer
            //----Normalize all the points to the maximum----//
            for (int i = 1; i < (sampLength / 2); i++)
            {
                binCont = Math.Abs(fourierHisto[i]) / Math.Abs(binMax); // Normalizing to the maximum
                theData[20 * Math.Log10(binCont)] = ((i) * sampFreq / sampLength);
                qadata.fourierdata[i-1] = 20 * Math.Log10(binCont);
                if (binCont != 1.0)
                {            // This is all points except the signal
                    numPoints++;
                    sum2 += binCont * binCont;
                }
            }


            noDistSum2 += sum2;     // This is so we can make parallel calculations subtracting harmonics

            //----Find Relevant Harmonics----//
            // Harmonics occur at |+/- (k*sampFreq) +/- (n*signalFreq)|
            // There will be overcounting if we let both k and n go +/-
            // Keeping k positive, and for k=0 keeping n positive,thedata there is no overcounting

            for (int k = 0; k <= 20; k++)
            {
                for (int ord = -55; ord <= 55; ord++)
                {
                    if (k > 0 || ord >= 0)
                        tempFreq = Math.Abs(k * sampFreq + ord * freq);
                    if (tempFreq != freq && tempFreq < sampFreq / 2 && tempFreq > 0)
                    {     // Limits range of interesting harmonics
                        numHarm++;
                        aHarmFreq[numHarm] = tempFreq;
                    }
                }
            }

            //----Printing the Highest Readings and Corresponding Freq----//

            var amps = theData.Keys.ToList();
            amps.Sort();

            foreach (double normAmp in amps)
            {
                fourierFreq = theData[normAmp];
                if ((ithData > (numPoints - 6)) && (ithData < numPoints))
                {
                    qadata.outFreq[numPoints - 1 - ithData] = String.Format("  {0:F2},   {1:F2}", fourierFreq, normAmp);
                }
                if (ithData == numPoints - 1)
                {
                    qadata.sfdr = normAmp;
                    // The SFDR is distance from signal (normalized to zero) to second largest
                    // value. This will be the normalized amplitude of the second last point
                    // since the last point is the signal (at 0 dB).
                }
                // This will remove the relevant harmonics so we can calculate SNR
                for (int i = 0; i <= numHarm; i++)
                {
                    if (Math.Abs(fourierFreq - aHarmFreq[i]) < 0.00001)
                    {
                        noDistSum2 -= Math.Pow(10, normAmp / 10);
                    }
                }
                ithData++;
            }

            //----Calculating Characteristic Variables----// 
            qadata.sinadNoHarm = 10 * Math.Log10(noDistSum2);
            qadata.sinad = 10 * Math.Log10(sum2);
            qadata.enob = (-qadata.sinad - 1.76) / 6.02;

            return qadata;
        }
    }
}
