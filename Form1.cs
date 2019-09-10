using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopifyScraper
{
    public partial class Form1 : Form
    {
        private List<Product> products;
        private int limit = 10;
        private int page = 1;
        private string urlinput;
        public static int MAX_PRODUCTS = 100000;
        public static string PRODUCTS_JSON = "/products.json?limit={0}&page={1}";
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        BackgroundWorker bw;
        private bool running = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                button1.Text = "Stop";
                running = true;
                InitData();
                textBox2.Text = "== Loading time ==\r\n";

                // what to do in the background thread
                bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    bool finished = false;
                    do
                    {
                        string url = urlinput + string.Format(PRODUCTS_JSON, limit, page);
                        finished = Read_JSON(url);
                        // report the progress in percent
                        b.ReportProgress(page * limit / (MAX_PRODUCTS * 100));
                        if (page++ >= MAX_PRODUCTS)
                            finished = true;

                        Thread.Sleep(100);
                    } while (!finished && running);

                });

                // what to do when progress changed (update the progress bar for example)
                bw.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    if (textBox2 != null)
                    {
                        textBox2.Text = string.Format("Imported {0} / {1} products...", products.Count, MAX_PRODUCTS);
                    }
                });

                // what to do when worker completes its task (notify the user)
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    string result = textBox2.Text;

                    if (products.Count > 0)
                    {
                        textBox2.Text = string.Format("== Finished importing {0} products ==\r\n", products.Count);
                        EnableExport(true);
                    }
                    else
                    {
                        textBox2.Text = string.Format("== Failed to load products ==\r\n", urlinput);
                        EnableExport(false);
                    }
                });

                bw.RunWorkerAsync();
            }
            else
            {
                if (button1.Text == "Resume")
                {
                    running = true;
                    if (!bw.IsBusy)
                        bw.RunWorkerAsync();
                    button1.Text = "Stop";
                    EnableExport(false);
                }
                else
                {
                    running = false;
                    button1.Text = "Resume";
                }
            }
        }

        public void EnableExport(bool enabled)
        {
            button2.Enabled = enabled;
        }

        public void InitData()
        {
            products = new List<Product>();
            textBox2.Text = string.Empty;
            button2.Enabled = false;

            bw = new BackgroundWorker();
            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            urlinput = textBox1.Text.Trim();
            MAX_PRODUCTS = Scraper.GET_MAX_ITEMS(urlinput);
        }

        public bool Read_JSON(string url)
        {
            //await semaphoreSlim.WaitAsync();
            bool isDone = false;
            string result = "Failed to load products!";

            try
            {
                var scraper = ShopifyScraper.Scraper._download_serialized_json_data<Scraper>(url);
                products.AddRange(scraper.products);
                result = "Downloading data of " + products.Count + " products ...";
                if (scraper.products == null || scraper.products.Length == 0)
                    isDone = true;
            }
            catch (Exception ex)
            {
                // silence is gold.
                isDone = true;
            }

            // textBox2.Text += result + "\r\n";
            //semaphoreSlim.Release();
            return isDone;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV File|*.csv|All Files|*.*";
            saveFileDialog1.Title = "Save products to CSV file";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                ExportCSV(saveFileDialog1.FileName);
            }
        }

        public async void ExportCSV(string path)
        {
            AbstractCsvExport exporter = new ShopifyExport();
            // Add to table
            try
            {
                bool x = await exporter.OpenFileStream(path);
                int count = products.Count;
                List<Task<bool>> writeTasks = new List<Task<bool>>(count);
                for (int i = 0; i < count; i++)
                {
                    List<string[]> result = exporter.ParseProduct(products[i]);
                    if (result != null)
                    {
                        writeTasks.Add(exporter.WriteLine(result));
                    }
                }
                Task writeResult = Task.WhenAll(writeTasks);
                try
                {
                    writeResult.Wait();
                }
                catch { }

                if (writeResult.Status == TaskStatus.RanToCompletion)
                {
                    textBox2.Text += "All products were exported succeeded.";
                    await exporter.Close();
                }
                else if (writeResult.Status == TaskStatus.Faulted)
                    textBox2.Text += "Export has failed";
            }
            catch(Exception ex)
            {
                textBox2.Text += "Export has failed";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (bw != null)
                bw.Dispose();
        }
    }
}
