using System;
using System.Data;
using System.Configuration;
using System.Web.SessionState;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Alert 的摘要说明
//ASP.NET中的弹出信息对话框类
/// </summary>
namespace KellCommons
{
    public class Alert
    {
        /// <summary>
        /// ShowMsgBox 显示消息对话框
        /// </summary>
        /// <param name="message">将要显示的消息</param>
        public static void ShowMsgBox(string message)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");</script>");
        }
        public static void thisTransfer(string url)
        {
            HttpContext.Current.Response.Write("<script>window.location='" + url + "';</script>");
        }

        public static void ShowMsgBox(string message, string url)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");location.href='" + url + "';</script>");
        }
        public static void ShowMsgBox(string message, string url, string FrmName)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");window.top." + FrmName + ".location='" + url + "';</script>");
        }

        public static void ShowMsgBoxClose(string message)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");opener.location.reload(true);window.close();</script>");
        }
        public static void ShowMsgBoxFreshNoClose(string message)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");opener.location.reload(true);</script>");
        }
        public static void ShowMsgBoxAndOpen(string message, string dirURL, string openURL, int openWidth, int openHeight)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");window.open('" + openURL + "','popupnav',   'width=" + openWidth + ",height=" + openHeight + ",resizable=1,scrollbars=no');window.location.href='" + dirURL + "';</script>");

        }
        /// <summary>
        /// 关闭本窗口，并且刷新父窗口中的IFRAME
        /// </summary>
        public static void ShowMsgBoxCloseIframeFresh(string message, string fatherIframe)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");window.top." + fatherIframe + ".location.href=window.top." + fatherIframe + ".location.href;window.top.hidePopWin();</script>");
        }
        public static void SearchWin(string url)
        {
            HttpContext.Current.Response.Write("<script>window.top.frmMain.location='" + url + "';window.top.hidePopWin();</script>");
        }

        public static void ShowMsgBoxCloseIframeFresh(string fatherIframe)
        {
            HttpContext.Current.Response.Write("<script>window.top." + fatherIframe + ".location.href=window.top." + fatherIframe + ".location.href;window.top.hidePopWin();</script>");
        }
        public static void frmTransfer(string url, string FrmName)
        {
            HttpContext.Current.Response.Write("<script>window." + FrmName + ".location='" + url + "';</script>");
        }
        public static void AlertSession(int dirType, string direc)
        {
            if (dirType == 1)//转跳到主页
            {
                HttpContext.Current.Response.Write("<script>alert('您还没登陆或在该页面停留时间过长，请重新登陆！');window.top.location='" + direc + "Default.aspx';</script>");
            }
            else//关闭
            {
                HttpContext.Current.Response.Write("<script>alert('您还没登陆或在该页面停留时间过长，请重新打开网站！');window.close();</script>");
            }
        }


        /**/
        /// <summary>
        /// 弹出窗口并返回到前一页面
        /// </summary>
        /// <param name="m">提示信息内容</param>
        public static void ShowMsgBoxBack(string message)
        {
            HttpContext.Current.Response.Write("<script>alert(\"" + message + "\");history.back(-1);</script>");
        }
        /// <summary>
        /// 显示一个提示
        /// </summary>
        public static void ShowMsgTip(string message, Label infoLab)
        {
            string tipMsg = "";
            tipMsg += "<table width='96%' border='0' align='center' cellpadding='0' cellspacing='0' bgcolor='#F3F3F3' style='border:1px dotted red'>";
            tipMsg += "<tr>";
            tipMsg += "    <td bgcolor='#FDF3D5' height='20px'>&nbsp;&nbsp;提示:" + message + " ";
            tipMsg += "     </td>";
            tipMsg += "  </tr>";
            tipMsg += "</table>";
            infoLab.Text = tipMsg;
        }

        private string alert;
        /**/
        /// <summary>
        /// 弹出窗口并返回到前一页面
        /// </summary>
        /// <param name="m">提示信息内容</param>
        public Alert(string m)
        {
            this.alert = "<script>alert('" + m + "');histroy.back(-1);</script>";
        }

        /// <summary>
        /// Back 返回前级操作
        /// </summary>
        public static void Back()
        {
            HttpContext.Current.Response.Write("<script>history.back();</script>");
        }

        /// <summary>
        /// 关闭当前窗口，并刷新父窗口
        /// </summary>
        public static void winClose()
        {
            HttpContext.Current.Response.Write("<script>javascript:opener.location.reload(true);window.close();</script>");
        }
        /// <summary>
        /// innerHTML 返回前级操作
        /// </summary>
        public static void innerHTML(string HtmlCont, string showMsg)
        {
            HttpContext.Current.Response.Write("<script>document.getElementById('" + HtmlCont + "').innerHTML = \"" + showMsg + "\";</script>");
        }


    }
}