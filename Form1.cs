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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopifyScraper
{
    public partial class Form1 : Form
    {
        private List<Product> products;
        private int limit = 10;
        private int page = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            products = new List<Product>();
            string url = textBox1.Text.Trim();
            string result = Read_JSON(url + "/products.json?limit=10&page=1");
            textBox2.Text = result;

            result += Read_JSON(url + "/products.json?limit=10&page=2");
            textBox2.Text = result;

            if (!result.Contains("Failed"))
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        public string Read_JSON(string url)
        {
            string result = "Failed to load products!";

            try
            {
                var scraper = ShopifyScraper.Sraper._download_serialized_json_data<Sraper>(url);
                products.AddRange(scraper.products);
                result = "Downloading data of " + products.Count + " products ...\n";
            }
            catch (Exception ex)
            {
                // silence is gold.
            }
            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExportCSV("products.csv");
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
                    textBox2.Text = "All products were exported succeeded.";
                    await exporter.Close();
                }
                else if (writeResult.Status == TaskStatus.Faulted)
                    textBox2.Text = "Export has failed";
            }
            catch(Exception ex)
            {
                textBox2.Text = "Export has failed";
            }
        }
    }
}
