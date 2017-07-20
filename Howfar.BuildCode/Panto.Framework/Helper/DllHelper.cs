using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Panto.Framework
{
    /// <summary>
    /// 系统DLL助手
    /// </summary>
    public class DllHelper
    {
        #region Import DLL

        #region user32.dll

        /// <summary>
        /// 确定给定窗口是否是最小化
        /// </summary>
        /// <param name="hWnd">被测试窗口的句柄</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// 设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>
        /// 创建指定窗口的线程设置到前台，并且激活该窗口
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 闪烁显示指定窗口，让窗口在活动与非活动的状态之间切换
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="bInvert">true:程序窗口标题栏从活动切换到非活动状态,false:窗口标题栏还原为最初的状态</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        #endregion

        #region kernel32

        /// <summary>
        /// 写入配置文件
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filePath">INI文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="def">默认值</param>
        /// <param name="retVal">返回值</param>
        /// <param name="size">读取字节大小</param>
        /// <param name="filePath">INI文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

        #endregion
    }
}
