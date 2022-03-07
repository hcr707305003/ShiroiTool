using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace ShiroiTool
{
    public class HttpDownloader
    {
        public decimal fileSize = 0;

        public decimal fileBeDownLoadSize = 0;

        public int fileBeDownLoadPer = 0;

        public ProgressBar progressBar = null;

        public HttpDownloader(ProgressBar progressBar1 = null)
        {
            progressBar = progressBar1;
        }

        public bool Save(string url, string SavePath, string SaveName = "")
        {
            if (SaveName == "") {
                SaveName = System.IO.Path.GetFileName(url);
            }
            bool Value = false;
            WebResponse response = null;
            Stream stream = null;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("text/")) {
                    Value = SaveBinaryFile(response, SavePath + SaveName);
                }
            } catch (Exception err) {
                string aa = err.ToString();
            }
            return Value;
        }


        private bool SaveBinaryFile(WebResponse response, string FileName)
        {
            bool Value = true;
            byte[] buffer = new byte[1024];
            try {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                Stream outStream = System.IO.File.Create(FileName);
                Stream inStream = response.GetResponseStream();
                fileSize = int.Parse(response.Headers.Get("Content-Length"));
                int l;
                do {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    fileBeDownLoadSize += l;
                    fileBeDownLoadPer = (int)((fileBeDownLoadSize / fileSize) * 100);
                    if (progressBar != null) {
                        new MethodInvoker(() => {
                            progressBar.Value = Convert.ToInt32(fileBeDownLoadPer);
                        })();
                    }
                    //Console.WriteLine("文件大小:{0},已下载:{1},未下载:{2},已下载的百分比:{3}",fileSize, fileBeDownLoadSize, fileSize - fileBeDownLoadSize, fileBeDownLoadPer);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                } while (l > 0);
                outStream.Close();
                inStream.Close();
            } catch {
                Value = false;
            }
            return Value;
        }
    }
}
