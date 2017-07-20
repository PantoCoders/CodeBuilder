using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    public class FileHelper
    {

        /// <summary>
        /// 获取文件的编码方式
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static Encoding GetEncoding(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            Encoding encoding1 = Encoding.Default;
            if (File.Exists(filePath))
            {
                using (FileStream stream1 = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (stream1.Length > 0)
                    {
                        using (StreamReader reader1 = new StreamReader(stream1, true))
                        {
                            char[] chArray1 = new char[1];
                            reader1.Read(chArray1, 0, 1);
                            encoding1 = reader1.CurrentEncoding;
                            reader1.BaseStream.Position = 0;
                            if (encoding1 == Encoding.UTF8)
                            {
                                byte[] buffer1 = encoding1.GetPreamble();
                                if (stream1.Length >= buffer1.Length)
                                {
                                    byte[] buffer2 = new byte[buffer1.Length];
                                    stream1.Read(buffer2, 0, buffer2.Length);
                                    for (int num1 = 0; num1 < buffer2.Length; num1++)
                                    {
                                        if (buffer2[num1] != buffer1[num1])
                                        {
                                            encoding1 = Encoding.Default;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    encoding1 = Encoding.Default;
                                }
                            }
                        }
                    }
                }
                if (encoding1 == null)
                {
                    encoding1 = Encoding.UTF8;
                }
            }
            return encoding1;
        }
        /// <summary>
        /// 计算字节的MD5
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        private static string GetByteMD5(byte[] data)
        {
            try {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(data);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString().ToUpper();

            }
            catch (Exception ex) {
                throw new Exception("GetMD5HashFromByte() fail,error:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取byte[] 数组的头尾 512KB 的内容MD5
        /// </summary>
        /// <param name="fileData">文件数据</param>
        /// <returns></returns>
        public static List<string> GetMD5File(byte[] fileData)
        {
            if (fileData == null) {
                throw new ArgumentNullException("fileData");
            }

            List<string> list = new List<string>();
            if (fileData.Length > 0) {
                long chunkSize = 1024 * 512;
                byte[] data = new byte[chunkSize > fileData.Length ? fileData.Length : chunkSize];
                Array.Copy(fileData, 0, data, 0, data.Length);
                list.Add(GetByteMD5(data));
                long readIndex = fileData.Length - chunkSize;
                if (readIndex > 0) {
                    data = new byte[fileData.Length - readIndex];
                    Array.Copy(fileData, readIndex, data, 0, data.Length);
                    list.Add(GetByteMD5(data));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取文件流 头尾 512KB 的内容MD5
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<string> GetMD5File(System.IO.Stream stream)
        {
            if (stream == null) {
                throw new ArgumentNullException("stream"); 
            }
            byte[] fileData = new byte[stream.Length];
            stream.Read(fileData, 0, fileData.Length);
            return GetMD5File(fileData);
        }
        /// <summary>
        /// 获取文件 头尾 512KB 的内容MD5
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<string> GetMD5File(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException("filePath");

            }
            if (!System.IO.File.Exists(filePath)) {
                throw new Panto.Framework.Exceptions.MyException("文件不存在:" + filePath);
            }
            return GetMD5File(System.IO.File.OpenRead(filePath));
        }
    }
}
