using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calameo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //foreach (var item in richTextBox1.Text)
            //{

            //}
            string ts = richTextBox1.Text;
            while (ts.Contains("calameo"))
            {
                string link = (ts.Substring(ts.IndexOf("https://www.calameo.com"))).Split('"')[0];
                //richTextBox2.AppendText(link+Environment.NewLine);
                //"noopener">
                string name = ts.Substring(ts.IndexOf("\"noopener\">")).Split('<')[0].Split('>')[1];
                richTextBox2.AppendText(link + ";" + name + Environment.NewLine);
                //ts = ts.Replace(link, "");
                ts = ts.Substring(ts.IndexOf(name));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";

        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (string item in richTextBox1.Lines)
            {
                if (item.Contains("href="))
                {
                    string link = item.Substring(item.IndexOf("href=")).Split('"')[1];
                    if (item.Contains("noopener"))
                    {
                        string name = item.Substring(item.IndexOf("\"noopener\">")).Split('<')[0].Split('>')[1];
                        richTextBox2.AppendText(link + ";" + name + Environment.NewLine);
                    }
                    else
                    {
                        richTextBox2.AppendText(link + Environment.NewLine);
                    }
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (var item in richTextBox1.Lines)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                string url = item.Split(';')[0];
                string ts = getRowUrl(url);
                string f1stlink = "";
                if (ts.Contains("image_src"))
                {
                    f1stlink = ts.Substring(ts.IndexOf("image_src")).Split('"')[2];
                }
                string len = ts.Substring(ts.IndexOf("Length:")).Split(' ')[1];
                richTextBox2.AppendText(f1stlink + ';' + len + Environment.NewLine);
            }
        }
        public string getRowUrl(string url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                ServicePointManager.ServerCertificateValidationCallback = ((object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
                ServicePointManager.Expect100Continue = false;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                httpWebRequest.Timeout = 10000;
                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
                httpWebRequest.KeepAlive = false;
                WebResponse response = httpWebRequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string text = streamReader.ReadToEnd();
                responseStream.Close();
                streamReader.Close();
                response.Close();
                return text;
            }
            catch (WebException webExcp)
            {
                if (webExcp.Status != WebExceptionStatus.Timeout)
                {
                    try
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                        if (httpResponse != null)
                        {
                            return webExcp.Status.ToString() + "-" + (int)httpResponse.StatusCode + "-" + httpResponse.StatusDescription; //webExcp.ToString();
                        }
                        else
                        {
                            return webExcp.Status.ToString();
                        }
                    }
                    catch
                    {
                        return "BadProxy";
                    }
                }
                else
                {
                    return "BadProxy";
                }
            }
            catch
            {
                return "BadProxy";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (var item in richTextBox1.Lines)
            {
                string dirName = item.Split('|')[1];
                string url = item.Split('|')[2];
                int col = int.Parse(item.Split('|')[3]);
                Directory.CreateDirectory(dirName);
                for (int i = 1; i <= col; i++)
                {

                    string cururl = url.Replace("p1.", "p" + i.ToString() + ".");
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(cururl, Application.StartupPath + "\\" + dirName + "\\" + i.ToString() + "." + url.Split('/')[4].Split('.')[1]);
                    }
                }
                richTextBox2.AppendText(item + " fin" + Environment.NewLine);
            }
            richTextBox2.AppendText("Fin" + Environment.NewLine);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string[] subDirs = Directory.GetDirectories(Environment.CurrentDirectory);
            foreach (string subDir in subDirs)
            {
                richTextBox2.AppendText(subDir);
                DirectoryInfo dsubDir = new DirectoryInfo(subDir + Environment.NewLine);
                FileInfo[] Files = dsubDir.GetFiles("*.jpg");
                foreach (FileInfo file in Files)
                {
                    createPdf(subDir + "\\" + file.Name);
                }
                richTextBox2.AppendText("Fin" + Environment.NewLine);
                //return;
                //for
            }
            richTextBox2.AppendText("Fin" + Environment.NewLine);
        }
        public void createPdf(string fileName)
        {
            iTextSharp.text.Rectangle pageSize = null;

            using (var srcImage = new Bitmap(fileName))
            {
                pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
            }
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
                document.Open();
                var image = iTextSharp.text.Image.GetInstance(fileName);
                document.Add(image);
                document.Close();
                string dstFilename = fileName.Replace(".jpg", ".pdf");
                File.WriteAllBytes(dstFilename, ms.ToArray());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string[] subDirs = Directory.GetDirectories(Environment.CurrentDirectory);
            foreach (string subDir in subDirs)
            {
                //richTextBox2.AppendText(subDir);
                //richTextBox2.AppendText(subDir.Split('\\')[subDir.Split('\\').Length - 1] + ".pdf");
                string outname=subDir.Split('\\')[subDir.Split('\\').Length - 1] + ".pdf";
                //return;
                DirectoryInfo dsubDir = new DirectoryInfo(subDir + Environment.NewLine);
                FileInfo[] Files = dsubDir.GetFiles("*.pdf");
                using (PdfDocument outPdf = new PdfDocument())
                {
                    for (int i = 1; i <= Files.Length; i++)
                    {
                        using (PdfDocument one = PdfReader.Open(subDir + "\\" + i.ToString() + ".pdf", PdfDocumentOpenMode.Import))
                            CopyPages(one, outPdf);
                        //CopyPages(two, outPdf);
                    }
                    outPdf.Save(outname);
                }

                richTextBox2.AppendText("Fin" + Environment.NewLine);
                //return;
                //for
            }
            richTextBox2.AppendText("Fin" + Environment.NewLine);
        }
        public void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }
    }
}
