
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Panto.Framework;
namespace Panto.Framework
{
    /// <summary>
    /// http访问帮助库
    /// </summary>
    public class HttpClientHelper
    {

        private static HttpClientHelper clien;

        private static readonly object objLock = new object();

        /// <summary>
        /// 客户端
        /// </summary>
        public static HttpClientHelper Client
        {
            get
            {
                if (clien == null)
                {
                    lock (objLock)
                    {
                        if (clien == null)
                        {
                            clien = new HttpClientHelper();
                        }
                    }
                }
                return clien;
            }
        }


        Encoding encoding = Encoding.Default;
        public string respHtml = "";
        WebProxy proxy;
        CookieContainer cc;
        WebHeaderCollection requestHeaders;
        WebHeaderCollection responseHeaders;
        int bufferSize = 1024 * 200;
        public event EventHandler<UploadEventArgs> UploadProgressChanged;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        /// <summary>
        /// 是否加载本地cookie
        /// </summary>
        public bool LoadLocalCookie = false;

        /// <summary>
        /// 创建WebClient的实例  
        /// </summary>
        /// <param name="_LoadLocalCookie">是否加载本地cookie</param>
        public HttpClientHelper(bool _LoadLocalCookie = false)
        {

            requestHeaders = new WebHeaderCollection();
            responseHeaders = new WebHeaderCollection();
            cc = new CookieContainer();

            if (LoadLocalCookie)
            {
                LoadCookiesFromDisk();
            }
        }
        /// <summary>    
        /// 设置发送和接收的数据缓冲大小    
        /// </summary>    
        public int BufferSize
        {
            get { return bufferSize; }
            set { bufferSize = value; }
        }
        /// <summary>    
        /// 获取响应头集合    
        /// </summary>    
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }
        /// <summary>    
        /// 获取请求头集合    
        /// </summary>    
        public WebHeaderCollection RequestHeaders
        {
            get { return requestHeaders; }
            set { requestHeaders = value; }
        }
        /// <summary>    
        /// 获取或设置代理    
        /// </summary>    
        public WebProxy Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }
        /// <summary>    
        /// 获取或设置请求与响应的文本编码方式 （默认Encoding.Default） 
        /// </summary>    
        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <summary>    
        /// 获取或设置与请求关联的Cookie容器    
        /// </summary>    
        public CookieContainer CookieContainer
        {
            get { return cc; }
            set { cc = value; }
        }

        public CookieCollection Cookies = null;
        private int _TimeOut = 0;

        /// <summary>
        /// 获取或设置访问超时时间（默认无限）
        /// </summary>
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }

        private int _StateCode = 404;
        /// <summary>
        /// http状态码
        /// </summary>
        public int StateCode { get { return _StateCode; } }

        public bool _IsAbort = false;
        /// <summary>
        /// 是否取消访问
        /// </summary>
        public bool IsAbort { get { return _IsAbort; } set { _IsAbort = value; } }

        /// <summary>
        /// 起源页
        /// </summary>
        public string Origin { get; set; }
        /// <summary>
        /// 上一个页
        /// </summary>
        public string Referer { get; set; }



        private bool _AllowAutoRedirect = true;
        /// <summary>
        /// 是否自动重定向（默认是）
        /// </summary>
        public bool AllowAutoRedirect { get { return _AllowAutoRedirect; } set { _AllowAutoRedirect = value; } }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LogingUserName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string LogingUserPwd { get; set; }

        public string Accept { get; set; }
        /// <summary>    
        ///  获取网页源代码    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <returns></returns>    
        public string GetHtml(string url)
        {
            HttpWebRequest request = CreateRequest(url, "GET");

            byte[] data = GetData(request);

            respHtml = ToHtml(request, data);

            return respHtml;
        }

        /// <summary>
        /// 内容格式
        /// </summary>
        public string ContentType { get; set; }

        private string CharacterSet = string.Empty;

        public string LoginUrl { get; set; }

        private static CredentialCache myCredCache = null;

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <param name="domain">域</param>
        /// <param name="loginUrl">登录地址</param>
        /// <param name="IsLogin">不使用缓存，重新登录</param>
        public void LogLan(string userName, string passWord, string domain, string loginUrl = "", bool IsLogin = false)
        {
            if (string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = LoginUrl;
            }
            else
            {
                LoginUrl = loginUrl;
            }

            HttpWebRequest WRequest;
            HttpWebResponse WResponse;
            if (IsLogin)
            {
                myCredCache = null;
            }
            if (myCredCache == null)
            {
                myCredCache = new CredentialCache();
            }
            if (myCredCache.GetCredential(new Uri(LoginUrl), "NTLM") == null)
            {
                myCredCache.Add(new Uri(LoginUrl), "NTLM", new NetworkCredential(userName, passWord, domain));
                // Pre-authenticate the request.
                WRequest = (HttpWebRequest)HttpWebRequest.Create(LoginUrl);
                // Set the username and the password.
                WRequest.Credentials = myCredCache;
                // This property must be set to true for Kerberos authentication.

                // Keep the connection alive.

                WRequest.UserAgent = "Upload Test";
                WRequest.Method = "HEAD";
                WRequest.Timeout = 10000;
                WResponse = (HttpWebResponse)WRequest.GetResponse();
                WResponse.Close();
            }

        }

        /// <summary>    
        /// 下载文件    
        /// </summary>    
        /// <param name="url">文件URL地址</param>    
        /// <param name="filename">文件保存完整路径</param>    
        public void DownloadFile(string url, string filename)
        {
            FileStream fs = null;
            try
            {
                HttpWebRequest request = CreateRequest(url, "GET");
                byte[] data = GetData(request);
                if (StateCode != 200)
                {
                    respHtml = ToHtml(request, data);
                }
                else
                {
                    if (data.Length > 0)
                    {
                        fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                        fs.Write(data, 0, data.Length);
                    }
                }
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        /// <summary>    
        /// 从指定URL下载数据    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <returns></returns>    
        public byte[] GetData(string url)
        {
            HttpWebRequest request = CreateRequest(url, "GET");
            byte[] data = GetData(request);

            return data;
        }
        /// <summary>    
        /// 向指定URL发送文本数据    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <param name="postData">urlencode编码的文本数据</param>    
        /// <returns></returns>    
        public string Post(string url, string postData)
        {
            byte[] data = encoding.GetBytes(postData);
            return Post(url, data);
        }
        /// <summary>    
        /// 向指定URL发送字节数据    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <param name="postData">发送的字节数组</param>    
        /// <returns></returns>    
        public string Post(string url, byte[] postData)
        {
            HttpWebRequest request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = postData.Length;

            PostData(request, postData);

            byte[] data = GetData(request);

            respHtml = ToHtml(request, data);

            return respHtml;
        }
        /// <summary>    
        /// 向指定网址发送mulitpart编码的数据    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <param name="mulitpartForm">mulitpart form data</param>    
        /// <returns></returns>    
        public string Post(string url, MultipartForm mulitpartForm)
        {
            HttpWebRequest request = CreateRequest(url, "POST");
            request.ContentType = mulitpartForm.ContentType;
            request.ContentLength = mulitpartForm.FormData.Length;

            PostData(request, mulitpartForm.FormData);
            respHtml = ToHtml(request, GetData(request));


            return respHtml;
        }

        ///// <summary>    
        ///// 向指定网址发送mulitpart编码的数据    
        ///// </summary>    
        ///// <param name="url">网址</param>    
        ///// <param name="mulitpartForm">mulitpart form data</param>    
        ///// <returns></returns>    
        //public FileInfos Post(string url, System.Web.HttpPostedFile postFile, User user, List<string> md5Info = null)
        //{
        //    MultipartForm mf = new MultipartForm();
        //    if (postFile == null) {
        //        throw new ArgumentNullException("postFile");
        //    }
        //    byte [] fileData = new byte[postFile.ContentLength];
        //    postFile.InputStream.Read(fileData,0,fileData.Length);
        //    return this.Post(url, fileData,postFile.FileName, user, md5Info);


        //}

        //public FileInfos Post(string url, string filePath, string fileName, User user, List<string> md5Info = null)
        //{
        //    MultipartForm mf = new MultipartForm();
        //    if (string.IsNullOrEmpty(filePath)) {
        //        throw new ArgumentNullException("filePath");
        //    }
        //    if (string.IsNullOrEmpty(fileName)) {
        //        throw new ArgumentNullException("fileName");
        //    }
        //    if(!File.Exists(filePath)){
        //       throw new Panto.Framework.Exceptions.MyException("文件不存在:"+filePath); 
        //    }
        //    byte[] fileData = null;
        //    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        //        fileData = new byte[fs.Length];
        //        fs.Read(fileData, 0, fileData.Length);
        //    }
        //    return this.Post(url, fileData, fileName, user, md5Info);
        //}

        //public FileInfos Post(string url, byte[] fileData, string fileName, User user,List<string> md5Info = null)
        //{
        //    MultipartForm mf = new MultipartForm();
        //    if (fileData==null) {
        //        throw new ArgumentNullException("filePath");
        //    }
        //    if (string.IsNullOrEmpty(fileName)) {
        //        throw new ArgumentNullException("fileName");
        //    }


        //    mf.AddFlie("file", fileName, fileData);
        //    mf.AddString("chunks", "1");
        //    mf.AddString("chunk", "1");
        //    mf.AddString("lastModifiedDate", DateTime.Now);
        //    mf.AddString("name", fileName);
        //    mf.AddString("size", fileData.Length);
        //    if (user != null) {
        //        mf.AddString("userID", user.UserGUID);
        //        mf.AddString("userName_Chn", user.UserNameChn);
        //    }
        //    if (md5Info == null) {
        //        md5Info = FileHelper.GetMD5File(fileData);
        //        if (md5Info != null && md5Info.Count > 0) {
        //            mf.AddString("FileCompleteSatrtMD5", md5Info[0]);
        //            if (md5Info.Count > 1) {
        //                mf.AddString("FileCompleteEndMD5", md5Info[1]);
        //            }
        //        }
        //    }
        //    else {
        //        if (md5Info != null && md5Info.Count > 0) {
        //            mf.AddString("FileCompleteSatrtMD5", md5Info[0]);
        //            if (md5Info.Count > 1) {
        //                mf.AddString("FileCompleteEndMD5", md5Info[1]);
        //            }
        //        }
        //    }

        //    mf.AddString("IsUnPack", "false");
        //    HttpWebRequest request = CreateRequest(url, "POST");
        //    request.ContentType = mf.ContentType;
        //    request.ContentLength = mf.FormData.Length;
        //    PostData(request, mf.FormData);
        //    respHtml = ToHtml(request, GetData(request));
        //    if (_StateCode == 200) {
        //        return respHtml.FromJson<FileInfos>();
        //    }
        //    else {
        //        throw new Panto.Framework.Exceptions.MyException(respHtml.StripHTML());
        //    }
        //}
        
        private string ToHtml(HttpWebRequest request, byte[] data)
        {
            Match meta = Regex.Match(Encoding.Default.GetString(data), "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
            string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value.ToLower() : string.Empty;
            charter = charter.Replace("\"", "").Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk");

            if (charter.Length > 2)
                encoding = Encoding.GetEncoding(charter.Trim());
            else
            {
                if (string.IsNullOrEmpty(CharacterSet))
                    encoding = Encoding.UTF8;
                else
                    try
                    {
                        encoding = Encoding.GetEncoding(CharacterSet);
                    }
                    catch
                    {
                        encoding = Encoding.UTF8;
                    }
            }

            return encoding.GetString(data);
        }

        private void SaveCookies(string cookieStr)
        {
            string[] cookies = cookieStr.Split(',');
            foreach (string cookeiValue in cookies)
            {
                string[] values = cookeiValue.Split(';');

                Cookie cookie = new Cookie();

                foreach (string keyValue in values)
                {
                    if (keyValue.IndexOf("=") > -1)
                    {
                        string key = keyValue.Substring(0, keyValue.IndexOf("=")).Trim();
                        string value = keyValue.Replace(key + "=", "");

                        switch (key.ToLower())
                        {
                            case "domain": cookie.Domain = value.Trim(); break;
                            case "path": cookie.Path = value.Trim(); break;
                            case "comment": cookie.Comment = value; break;
                            case "commenturi": cookie.CommentUri = new Uri(value); break;
                            case "discard": cookie.Discard = Convert.ToBoolean(value); break;
                            case "expired": cookie.Expired = Convert.ToBoolean(value); break;
                            case "port": cookie.Port = value; break;
                            case "secure": cookie.Secure = Convert.ToBoolean(value); break;
                            case "version": cookie.Version = Convert.ToInt32(value); break;
                            case "expires": cookie.Expires = DateTime.Now.AddDays(7); break;
                            default: cookie.Value = value; cookie.Name = key.Trim(); break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(cookie.Name) && !string.IsNullOrEmpty(cookie.Domain))
                {
                    cc.Add(cookie);
                }
            }
        }

        /// <summary>    
        /// 读取请求返回的数据    
        /// </summary>    
        /// <param name="request">请求对象</param>    
        /// <returns></returns>    
        private byte[] GetData(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                _StateCode = (int)response.StatusCode;
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                if (response == null)
                {
                    _StateCode = 404;
                    return new byte[0];
                }
                else
                {
                    _StateCode = (int)response.StatusCode;
                }
            }
            Stream stream = response.GetResponseStream();

            responseHeaders = response.Headers;
            Cookies = response.Cookies;
            //if (response.Headers["Set-Cookie"] != null) {
            //    SaveCookies(response.Headers["Set-Cookie"].ToString());
            //}


            if (LoadLocalCookie)
            {
                SaveCookiesToDisk();
            }
            CharacterSet = response.CharacterSet;

            //this.Referer = request.RequestUri.ToString();

            DownloadEventArgs args = new DownloadEventArgs();
            if (responseHeaders[HttpResponseHeader.ContentLength] != null)
                args.TotalBytes = Convert.ToInt32(responseHeaders[HttpResponseHeader.ContentLength]);

            MemoryStream msTemp = new MemoryStream();

            int count = 0;
            byte[] buf = new byte[bufferSize];
            Stream ya = null;
            if (ResponseHeaders[HttpResponseHeader.ContentEncoding] != null)
            {
                switch (ResponseHeaders[HttpResponseHeader.ContentEncoding].ToLower())
                {
                    case "gzip":
                        ya = new GZipStream(stream, CompressionMode.Decompress); break;
                    case "deflate":
                        ya = new DeflateStream(stream, CompressionMode.Decompress); break;
                    default: ya = stream;
                        break;
                }
            }
            else
            {
                ya = stream;
            }
            while ((count = ya.Read(buf, 0, buf.Length)) > 0)
            {
                msTemp.Write(buf, 0, count);
                if (this.DownloadProgressChanged != null)
                {
                    args.BytesReceived += count;
                    args.ReceivedData = new byte[count];
                    Array.Copy(buf, args.ReceivedData, count);
                    this.DownloadProgressChanged(this, args);
                }
                System.Threading.Thread.Sleep(1);
            }
            ya.Close();
            stream.Close();
            return msTemp.ToArray();

        }
        /// <summary>    
        /// 发送请求数据    
        /// </summary>    
        /// <param name="request">请求对象</param>    
        /// <param name="postData">请求发送的字节数组</param>    
        private void PostData(HttpWebRequest request, byte[] postData)
        {
            int offset = 0;
            int sendBufferSize = bufferSize;
            int remainBytes = 0;
            Stream stream = request.GetRequestStream();
            UploadEventArgs args = new UploadEventArgs();
            args.TotalBytes = postData.Length;
            while ((remainBytes = postData.Length - offset) > 0)
            {
                if (sendBufferSize > remainBytes) sendBufferSize = remainBytes;
                stream.Write(postData, offset, sendBufferSize);
                offset += sendBufferSize;
                if (this.UploadProgressChanged != null)
                {
                    args.BytesSent = offset;
                    this.UploadProgressChanged(this, args);
                }
                System.Threading.Thread.Sleep(1);
            }
            stream.Close();
        }
        /// <summary>    
        /// 创建HTTP请求    
        /// </summary>    
        /// <param name="url">URL地址</param>    
        /// <returns></returns>    
        private HttpWebRequest CreateRequest(string url, string method)
        {
            Uri uri = new Uri(url);

            if (uri.Scheme == "https")
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(this.CheckValidationResult);

            // Set a default policy level for the "http:" and "https" schemes.    
            //HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            //HttpWebRequest.DefaultCachePolicy = policy;
            //
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = this.AllowAutoRedirect;



            request.Method = method;
            request.ServicePoint.ConnectionLimit = 63000;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.Expect100Continue = false;

            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            if (Accept != null)
            {
                request.Accept = Accept;
            }
            else
            {
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            }
            request.KeepAlive = !IsAbort;
            if (!string.IsNullOrEmpty(this.Referer))
            {
                request.Referer = this.Referer;
            }
            if (!string.IsNullOrEmpty(this.Origin))
            {
                if (request.Headers["Origin"] == null)
                {
                    request.Headers.Add("Origin", this.Origin);
                }
                else
                {
                    request.Headers.Set("Origin", this.Origin);
                }
            }
            request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");

            //request.Timeout = _TimeOut > 0 ? _TimeOut : 5000;

            if (proxy != null)
            {
                request.Proxy = new WebProxy(proxy.Address, false);

            }
            if (!string.IsNullOrEmpty(ContentType))
            {
                request.ContentType = ContentType;
            }
            else
            {
                request.ContentType = "application/x-www-form-urlencoded";
            }

            request.CookieContainer = cc;

            //request.PreAuthenticate = false;
            //request.UnsafeAuthenticatedConnectionSharing = true;
            //request.AllowWriteStreamBuffering = false;
            //request.UseDefaultCredentials = true;

            if (!string.IsNullOrEmpty(this.LogingUserName) && !string.IsNullOrEmpty(this.LogingUserPwd))
            {
                request.Credentials = new NetworkCredential(LogingUserName, LogingUserPwd);
            }
            else
            {
                if (myCredCache != null)
                {
                    request.Credentials = myCredCache;
                }
            }



            foreach (string key in requestHeaders.Keys)
            {
                request.Headers.Add(key, requestHeaders[key]);
            }
            requestHeaders.Clear();
            return request;
        }
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        /// <summary>    
        /// 将Cookie保存到磁盘    
        /// </summary>    
        private void SaveCookiesToDisk()
        {

            string cookieFile = Environment.CurrentDirectory + "\\HttpClientHelper.cookie";
            FileStream fs = null;
            try
            {
                if (!File.Exists(cookieFile))
                {
                    File.Exists(cookieFile);
                }
                fs = new FileStream(cookieFile, FileMode.Create);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formater.Serialize(fs, cc);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        /// <summary>    
        /// 从磁盘加载Cookie    
        /// </summary>    
        private void LoadCookiesFromDisk()
        {
            cc = new CookieContainer();
            string cookieFile = Environment.CurrentDirectory + "\\HttpClientHelper.cookie";
            if (!File.Exists(cookieFile))
                return;
            FileStream fs = null;
            try
            {
                fs = new FileStream(cookieFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length > 0)
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    cc = (CookieContainer)formater.Deserialize(fs);
                }

            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        /// <summary>
        /// 判断服务是否可以访问
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool IsServiceAccess(string url, string param, string method)
        {
            if ("POST" == method)
            {
                byte[] postData = encoding.GetBytes(param);
                HttpWebRequest request = CreateRequest(url, "POST");
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = postData.Length;

                PostData(request, postData);

                return IsAccessSuccess(request);
            }
            else
            {
                HttpWebRequest request = CreateRequest(url, "GET");

                return IsAccessSuccess(request);
            } 
        }
        
        /// <summary>
        /// 判断服务是否请求成功
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool IsAccessSuccess(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                int stateCode = (int)response.StatusCode;
                if (200 == stateCode)
                {
                    return true;
                }
                return false;
            }
            catch (WebException ex)
            {
                //response = (HttpWebResponse)ex.Response;
                //if (response == null)
                //{
                //    _StateCode = 404;
                //}
                //else
                //{
                //    _StateCode = (int)response.StatusCode;
                //}
                return false;
            }
        }
    }



    /// <summary>    
    /// 上传数据参数    
    /// </summary>    
    public class UploadEventArgs : EventArgs
    {
        int bytesSent;
        int totalBytes;
        /// <summary>    
        /// 已发送的字节数    
        /// </summary>    
        public int BytesSent
        {
            get { return bytesSent; }
            set { bytesSent = value; }
        }
        /// <summary>    
        /// 总字节数    
        /// </summary>    
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
    }
    /// <summary>    
    /// 下载数据参数    
    /// </summary>    
    public class DownloadEventArgs : EventArgs
    {
        int bytesReceived;
        int totalBytes;
        byte[] receivedData;
        /// <summary>    
        /// 已接收的字节数    
        /// </summary>    
        public int BytesReceived
        {
            get { return bytesReceived; }
            set { bytesReceived = value; }
        }
        /// <summary>    
        /// 总字节数    
        /// </summary>    
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
        /// <summary>    
        /// 当前缓冲区接收的数据    
        /// </summary>    
        public byte[] ReceivedData
        {
            get { return receivedData; }
            set { receivedData = value; }
        }
    }





    /// <summary>    
    /// 对文件和文本数据进行Multipart形式的编码    
    /// </summary>    
    public class MultipartForm
    {
        private Encoding encoding;
        private MemoryStream ms;
        private string boundary;
        private byte[] formData;
        /// <summary>    
        /// 获取编码后的字节数组    
        /// </summary>    
        public byte[] FormData
        {
            get
            {
                if (formData == null)
                {
                    byte[] buffer = encoding.GetBytes("--" + this.boundary + "--\r\n");
                    ms.Write(buffer, 0, buffer.Length);
                    formData = ms.ToArray();
                }
                return formData;
            }
        }
        /// <summary>    
        /// 获取此编码内容的类型    
        /// </summary>    
        public string ContentType
        {
            get { return string.Format("multipart/form-data; boundary={0}", this.boundary); }
        }
        /// <summary>    
        /// 获取或设置对字符串采用的编码类型    
        /// </summary>    
        public Encoding StringEncoding
        {
            set { encoding = value; }
            get { return encoding; }
        }
        /// <summary>    
        /// 实例化    
        /// </summary>    
        public MultipartForm()
        {
            boundary = string.Format("----{0}", "WebKitFormBoundaryLuDumdjAfSwKOlF9");
            ms = new MemoryStream();
            encoding = Encoding.UTF8;
        }
        /// <summary>    
        /// 添加一个文件    
        /// </summary>    
        /// <param name="name">文件域名称</param>    
        /// <param name="fileName">文件的完整路径</param>   
        /// <param name="stream">文件流</param>    
        public void AddFlie(string name, string fileName, System.IO.Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            FileStream fs = null;
            byte[] fileData = new byte[stream.Length];
            try
            {
                stream.Read(fileData, 0, fileData.Length);

                this.AddFlie(name, fileName, fileData, fileData.Length);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        /// <summary>    
        /// 添加一个文件    
        /// </summary>    
        /// <param name="name">文件域名称</param>    
        /// <param name="filename">文件的完整路径</param>    
        public void AddFlie(string name, string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("尝试添加不存在的文件。", filename);
            FileStream fs = null;
            byte[] fileData = { };
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
                this.AddFlie(name, Path.GetFileName(filename), fileData, fileData.Length);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
        /// <summary>    
        /// 添加一个文件    
        /// </summary>    
        /// <param name="name">文件域名称</param>    
        /// <param name="filename">文件名</param>    
        /// <param name="fileData">文件二进制数据</param>    
        /// <param name="dataLength">二进制数据大小</param>    
        public void AddFlie(string name, string filename, byte[] fileData, int dataLength = 0)
        {
            if (dataLength <= 0 || dataLength > fileData.Length)
            {
                dataLength = fileData.Length;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("--{0}\r\n", this.boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\n", name, filename);
            sb.AppendFormat("Content-Type: {0}\r\n", this.GetContentType(filename));
            sb.Append("\r\n");
            byte[] buf = encoding.GetBytes(sb.ToString());
            ms.Write(buf, 0, buf.Length);
            ms.Write(fileData, 0, dataLength);
            byte[] crlf = encoding.GetBytes("\r\n");
            ms.Write(crlf, 0, crlf.Length);
        }




        /// <summary>    
        /// 添加字符串    
        /// </summary>    
        /// <param name="name">文本域名称</param>    
        /// <param name="value">文本值</param>    
        public void AddString(string name, object value)
        {
            if (value != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("--{0}\r\n", this.boundary);
                sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n", name);
                sb.Append("\r\n");
                sb.AppendFormat("{0}\r\n", value.ToString());
                byte[] buf = encoding.GetBytes(sb.ToString());
                ms.Write(buf, 0, buf.Length);
            }

        }



        /// <summary>    
        /// 从注册表获取文件类型    
        /// </summary>    
        /// <param name="filename">包含扩展名的文件名</param>    
        /// <returns>如：application/stream</returns>    
        private string GetContentType(string filename)
        {
            Microsoft.Win32.RegistryKey fileExtKey = null; ;
            string contentType = "application/stream";
            try
            {
                fileExtKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(Path.GetExtension(filename));
                if (fileExtKey == null)
                {
                    contentType = "application/stream";
                }
                else
                {
                    contentType = fileExtKey.GetValue("Content Type", contentType).ToString();
                }

            }
            finally
            {
                if (fileExtKey != null) fileExtKey.Close();
            }
            return contentType;
        }
    }




    /// <summary>
    /// FileInfo 的实体类
    /// </summary>
    [Serializable]
    public class FileInfos
    {

        /// <summary>
        /// 
        /// </summary>
        public Guid FileID { get; set; }
        /// <summary>
        /// 源文件名
        /// </summary>
        public string FileSourceName { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// 文件MD5值
        /// </summary>
        public string FileMD { get; set; }
        /// <summary>
        /// 文件最后修改时间
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        /// <summary>
        /// 文件绝对地址（以虚拟目录根目录开始）
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 下载次数
        /// </summary>
        public int? DownloadTick { get; set; }
        /// <summary>
        /// 文件物理地址
        /// </summary>
        public string FilePhysical { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 新文件名
        /// </summary>
        public string FileNewName { get; set; }
        /// <summary>
        /// 文件服务器编号
        /// </summary>
        public Guid? FileSerID { get; set; }
        /// <summary>
        /// 开始头部分MD5
        /// </summary>
        public string FileSatrtMD5 { get; set; }
        /// <summary>
        /// 结束部分MD5
        /// </summary>
        public string FileEndMD5 { get; set; }


        /// <summary>
        /// 文件附加信息
        /// </summary>


        public string FileData { get; set; }

        /// <summary>
        /// 文件预览地址
        /// </summary>
        public string PreviewAddress { get; set; }


        /// <summary>
        /// 转换状态
        /// </summary>
        public ConvertStateE ConvertState { get; set; }


        /// <summary>
        /// 文件是否已存在
        /// </summary>
        public bool exist { get; set; }



        /// <summary>
        /// 文件与业务表关系数据主键
        /// </summary>
        public Guid? BusinessFileID { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 文件下载地址
        /// </summary>
        public string DowsFileAddress
        {
            get;
            set;
        }
    }

    public enum ConvertStateE
    {
        /// <summary>
        /// 等待转换
        /// </summary>
        Wait = 0,
        /// <summary>
        /// 完成转换
        /// </summary>
        Complete = 1,
        /// <summary>
        /// 不支持
        /// </summary>
        NoSupport = 2,
        /// <summary>
        /// 异常
        /// </summary>
        Error = 3
    }
}
