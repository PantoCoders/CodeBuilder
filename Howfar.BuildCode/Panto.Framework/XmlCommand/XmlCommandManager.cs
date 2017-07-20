using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// Xml命令助手
    /// </summary>
    public class XmlCommandManager
    {
        /// <summary>
        /// 加载命令
        /// </summary>
        /// <param name="path">XmlCommand目录</param>
        public static void LoadCommands(string path)
        {
            ClownFish.XmlCommandManager.LoadCommnads(path);
        }
    }
}
