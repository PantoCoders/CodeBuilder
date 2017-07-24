using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;


namespace Panto.Map.Extensions.Log
{
	/// <summary>
	/// 提供记录异常日志到文件的方法。
	/// </summary>
	internal static class FileLoger
	{
		private static readonly string s_separateLine = "<!--##############f2781505-f286-4c9d-b73d-fa78eae22723$$$$$$$$$$$$$-->";

		/// <summary>
		/// 将一段文本追加到指定的文件结尾，并添加分隔行。
		/// </summary>
		/// <param name="text">要写入的文本</param>
		/// <param name="filePath">要写入的文件路径</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void SaveToFile(string text, string filePath)
		{
			string dir = Path.GetDirectoryName(filePath);
			if( Directory.Exists(dir) == false )
				Directory.CreateDirectory(dir);


			string contents = text + "\r\n\r\n" + s_separateLine + "\r\n\r\n";
			File.AppendAllText(filePath, contents, Encoding.UTF8);
		}


		/// <summary>
		/// 根据文件或者目录，按特定的分隔符解析所有文件，返回多次调用SaveToFile得到的输入文本列表。
		/// </summary>
		/// <param name="fileOrDirectories">文件或者目录</param>
		/// <returns>返回多次调用SaveToFile得到的输入文本列表。</returns>
		public static List<string> ParseLines(params string[] fileOrDirectories)
		{
			if( fileOrDirectories == null || fileOrDirectories.Length == 0 )
				return null;

			// 先找出所有文件名
			List<string> files = new List<string>();

			foreach( string fileOrDirectory in fileOrDirectories ) {
				if( File.Exists(fileOrDirectory) )
					files.Add(fileOrDirectory);
				else {
					string[] filenames = Directory.GetFiles(fileOrDirectory, "*.log", SearchOption.AllDirectories);
					foreach( string file in filenames )
						files.Add(file);
				}
			}

			// 解析所有文件
			List<string> lines = new List<string>();

			string line = null;
			StringBuilder sb = new StringBuilder();

			foreach( string file in files ) {
				using( StreamReader reader = new StreamReader(file, Encoding.UTF8) ) {
					while( (line = reader.ReadLine()) != null ) {
						// 遇到分隔符
						if( line == s_separateLine ) {
							if( sb.Length > 0 ) {
								lines.Add(sb.ToString());
								sb.Remove(0, sb.Length);
							}
						}
						else {
							// 没有分隔符就累加
							if( line.Length > 0 )
								sb.AppendLine(line);
						}
					}
				}
			}

			return lines;
		}


	}
}
