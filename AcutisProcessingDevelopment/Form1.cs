using RASW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcutisProcessingDevelopment
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string processedTargetFolder = @"C:\ACData\AC_Synchronised_Export_Data";
        string originalFilesFolder = @"C:\ACData";           // appSet.getValue("DataFolderPath");
        string configurationFolder = @"C:\ACData\Configuration";
        int ProcessCounter = 0;

        Dictionary<string, DateTime> lastWriteTimes = new Dictionary<string, DateTime>();

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckFolderStructure();
        }

        async void ProcessFiles()
        {
            try
            {
                AppSettings appSet = new AppSettings(Path.Combine(configurationFolder, "Acutis.xml"));
                string bk = appSet.getValue("BK");
                string ak = appSet.getValue("AK");
                bool uploadToIS = false;


                if (bk.Length != 0)
                    uploadToIS = true;

                Dictionary<string, string> dataParams = new Dictionary<string, string>();

                try
                {
                    if (Directory.Exists(originalFilesFolder))
                    {
                        CheckFolderStructure();
                        string dateNow = DateTime.Now.ToString();                               // get the date time now so that each file has the same date time
                        string[] files = Directory.GetFiles(originalFilesFolder, "*.csv");      // get the list of "csv" file only that are in the folder

                        if (UpdateManager.CheckForNewFileWrites(lastWriteTimes, files) > 0)     // check for any update for AC, if so process files
                        {
                            foreach (var file in files)                                         // full paths :)
                            {
                                string lastLine = File.ReadLines(file).Last();                  // gets the last line from file.
                                string[] data = lastLine.Split(';');                            // data[0] = "2015-11-05 15:20:17.072"   data[1] = "20.9364"

                                using (StreamWriter sw = File.AppendText(Path.Combine(processedTargetFolder, Path.GetFileName(file))))
                                {
                                    string formattedValue = String.Format("{0:0.00}", Convert.ToDouble(data[1]));       // format to 2 decimal places (does round up 20.94)
                                    sw.WriteLine(dateNow + "|" + formattedValue);

                                    if (uploadToIS)
                                        dataParams.Add(Path.GetFileNameWithoutExtension(file), formattedValue);         // if upload to ISD is enabled then async upload the data
                                }
                            }

                            if (uploadToIS)
                            {
                                await SendMessageToIS(dataParams,ak,bk);
                            }

                            ProcessCounter++;
                            lblProcessCount.Text = "Process Count: " + ProcessCounter.ToString();
                        }
                        else
                        {
                            dataParams.Clear();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;  // log error to event log
                }

                //dataParams.Clear(); // clear the dictionary
            }
            catch (Exception)
            {
                throw; // log the error in service log
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private async Task SendMessageToIS(Dictionary<string, string> dataParams,string AK, string BK)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(dataParams);
                //var response = await client.PostAsync("https://groker.initialstate.com/api/events?accessKey=V4iVTL0C9Mh5m5gMPP7XLwwF4y3mouYI&bucketKey=JWRCPRMBGJRW", content);  // DEV Bucket
                var response = await client.PostAsync("https://groker.initialstate.com/api/events?accessKey=" + AK + "&bucketKey=" + BK, content);
                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        private void CheckFolderStructure()
        {
            if(!Directory.Exists(processedTargetFolder))
            {
                Directory.CreateDirectory(processedTargetFolder);
            }

            if (!Directory.Exists(configurationFolder))     // create hidden folder if not already created
            {
                DirectoryInfo di = Directory.CreateDirectory(configurationFolder);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProcessFiles();
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
           ProcessFiles();
        }

        private async void btnTestUploadToIS_Click(object sender, EventArgs e)
        {
           // 2016-11-29T09:22:06.669650Z  format   /// DateTime.UtcNow.ToString("o")

            Random r = new Random();
            double number = r.NextDouble();

            Dictionary<string, string> dataParams = new Dictionary<string, string>();

            string formattedValue = String.Format("{0:0.00}", Convert.ToDouble(number * 100));       // format to 2 decimal places (does round up 20.94)

            dataParams.Add(Path.GetFileNameWithoutExtension("TestParamName"), formattedValue);         // if upload to ISD is enabled then async upload the data
            await SendMessageToIS(dataParams, "", "");

            MessageBox.Show(DateTime.UtcNow.ToString("o"));

        }
    }
}
