using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShopifyScraper
{
    public class Scraper
    {
        public Product[] products { get; set; }
        public static Agent[] agents { get; set; }

        static Scraper()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var agentList = Scraper._download_serialized_json_data_file<AgentList>("agent_list.json");
            agents = agentList.agents;
        }

        public static int GET_MAX_ITEMS(string url)
        {
            int num = 0;
            string targetUrl = url.Contains("/collections/all")? url: url + "/collections/all";
            HtmlWeb htmlWeb = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8 
            };

            HtmlDocument document = htmlWeb.Load(targetUrl);
            var max_html = document.DocumentNode.SelectNodes("//span[@class='sp_result_html']").First();
            string input = max_html.ChildNodes[0].InnerHtml;
            // Split on one or more non-digit characters.
            string[] numbers = Regex.Split(input, @"\D+");
            if (numbers != null && numbers.Length > 0)
            {
                for (int i = numbers.Length - 1; i > 0; i--)
                {
                    if (numbers[i].Length > 0)
                    {
                        num = int.Parse(numbers[i]);
                        break;
                    }
                }
            }
            return num;
        }

        // Returns JSON string
        public static string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UseDefaultCredentials = true;

            if (agents != null)
            {
                Random rd = new Random();
                int x = rd.Next(1, agents.Length);
                request.UserAgent = agents[x].agent_string;
            }
            else
            {
                request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 70.0.3538.77 Safari / 537.36";
            }

            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        public static T _download_serialized_json_data<T>(string url) where T : new()
        {
            var json_data = string.Empty;
            // attempt to download JSON data as a string
            try
            {
                json_data = GET(url);
            }
            catch (Exception ex) { }
            // if string with JSON data is not empty, deserialize it to class and return its instance 
            return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
        }

        public static T _download_serialized_json_data_file<T>(string filePath) where T : new()
        {
            var json_data = string.Empty;
            // attempt to download JSON data as a string
            try
            {
                json_data = File.ReadAllText(filePath);
            }
            catch (Exception ex) { }
            // if string with JSON data is not empty, deserialize it to class and return its instance 
            return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
        }
    }

    public class AgentList
    {
        public Agent[] agents { get; set; }
    }

    public class Agent
    {
        public string agent_string { get; set; }
        public string agent_type { get; set; }
        public string agent_name { get; set; }
        public string os_type { get; set; }
        public string os_name { get; set; }
        public string device_type { get; set; }
    }

    public class Product
    {
        public long id { get; set; }
        public string title { get; set; }
        public string handle { get; set; }
        public string body_html { get; set; }
        public string published_at { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public string[] tags { get; set; }
        public Variant[] variants { get; set; }
        public Image[] images { get; set; }
        public Option[] options { get; set; }
    }

    public class Variant
    {
        public long id { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
        public string sku { get; set; }
        public bool requires_shipping { get; set; }
        public bool taxable { get; set; }
        public Featured_Image featured_image { get; set; }
        public bool available { get; set; }
        public string price { get; set; }
        public int grams { get; set; }
        public string compare_at_price { get; set; }
        public int position { get; set; }
        public long product_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }

    public class Featured_Image
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public int position { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public object alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public long[] variant_ids { get; set; }
    }

    public class Image
    {
        public long id { get; set; }
        public string created_at { get; set; }
        public int position { get; set; }
        public string updated_at { get; set; }
        public long product_id { get; set; }
        public long[] variant_ids { get; set; }
        public string src { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Option
    {
        public string name { get; set; }
        public int position { get; set; }
        public string[] values { get; set; }
    }
}
