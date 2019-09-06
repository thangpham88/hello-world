using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyScraper
{
    public abstract class AbstractCsvExport
    {
        public abstract string Name { get; }
        public abstract string[] Headers { get; }
        public abstract List<string[]> ParseProduct(Product product);

        private FileStream fs;
        private StreamWriter wr;

        public async Task<bool> OpenFileStream(string path)
        {
            if (fs != null)
            {
                await Close();
            }

            try
            {
                bool isExist = File.Exists(path);
                fs = new FileStream(path, !isExist ? FileMode.Create : FileMode.Append, FileAccess.Write);
                wr = new StreamWriter(fs);
                if (!isExist)
                {
                    wr.WriteLine(string.Join(",", Headers.Select(h => MakeupText(h))));
                    wr.Flush();
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<bool> WriteLine(List<string[]> lines)
        {
            await semaphoreSlim.WaitAsync();

            foreach (var line in lines)
            {
                wr.WriteLine(string.Join(",", line.Select(l => MakeupText(l))));
                wr.Flush();
            }

            semaphoreSlim.Release();

            return true;
        }

        public async Task Close()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (wr != null)
                {
                    wr.Flush();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            catch { }
            semaphoreSlim.Release();

            fs = null;
            wr = null;
        }

        private static string MakeupText(string src)
        {
            string dts = (src == null ? "" : src);
            if (dts.Contains("\"") || dts.Contains(","))
            {
                dts = dts.Replace("\"", "\"\"");
                dts = string.Format("\"{0}\"", dts);
            }

            return dts;
        }
    }
}
