using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Collections;
using System.IO;

namespace ShiroiTool
{
    internal class HttpRequest
    {
        public SortedList Headers = new SortedList();//设置头信息

        #region Delete方式
        public string Delete(string data, string url)
        {
            return CommonHttpRequest(data, url, "DELETE");
        }

        public string Delete(string url)
        {
            //Web访问对象64
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            //设置头信息
            foreach (var item in this.Headers.Keys) {
                SetHeaderValue(myRequest.Headers, (string)item, (string)this.Headers[item]);
            }
            myRequest.Method = "DELETE";
            // 获得接口返回值68
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string ReturnXml = HttpUtility.UrlDecode(reader.ReadToEnd());
            string ReturnXml = reader.ReadToEnd();
            reader.Close();
            myResponse.Close();
            return ReturnXml;
        }
        #endregion

        #region Put方式
        public string Put(string data, string url)
        {
            return CommonHttpRequest(data, url, "PUT");
        }
        #endregion

        #region POST方式实现

        public string Post(string data, string url)
        {
            return CommonHttpRequest(data, url, "POST");
        }

        public string CommonHttpRequest(string data, string url, string type)
        {
            //构造http请求的对象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            //转成网络流
            byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);
            //设置
            myRequest.Method = type;
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = "application/json";
            myRequest.MaximumAutomaticRedirections = 1;
            myRequest.AllowAutoRedirect = true;
            //设置头信息
            foreach (var item in this.Headers.Keys) {
                SetHeaderValue(myRequest.Headers, (string)item, (string)this.Headers[item]);
            }
            // 发送请求
            Stream newStream = myRequest.GetRequestStream();
            newStream.Write(buf, 0, buf.Length);
            newStream.Close();
            // 获得接口返回值
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            string ReturnXml = reader.ReadToEnd();
            reader.Close();
            myResponse.Close();
            return ReturnXml;
        }
        #endregion

        #region GET方式实现
        public string Get(string url)
        {
            //构造一个Web请求的对象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            //设置头信息
            foreach (var item in this.Headers.Keys) {
                SetHeaderValue(myRequest.Headers, (string)item, (string)this.Headers[item]);
            }
            // 获得接口返回值68
            //获取web请求的响应的内容
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

            //通过响应流构造一个StreamReader
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string ReturnXml = HttpUtility.UrlDecode(reader.ReadToEnd());
            string ReturnXml = reader.ReadToEnd();
            reader.Close();
            myResponse.Close();
            return ReturnXml;
        }
        #endregion

        #region 头信息添加
        public HttpRequest SetHearder(SortedList h)
        {
            this.Headers = h;
            return this;
        }


        public void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null) {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }
        #endregion
    }
}
