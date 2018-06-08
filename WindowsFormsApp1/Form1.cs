using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        private bool site = false;       //开始、停止状态
        private Thread tdWork = null;    //工作线程
        Process ps = null;               //工作进程

        private delegate void DShowTime(int value);    //用于夸线程操作控件，方法委托

        private int outCount = 4, show = 0;

        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);
        }

        #region //线程函数-即主工作方法
        private void AppRunMethod()
        {
            ProcessStartInfo psi = new ProcessStartInfo("Ping.exe"," www.baidu.com "+ " -t -l 1");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            ps = Process.Start(psi);

            string outStream = ""; //储存Ping.exe输出流字符串
            int time = 0;

            while (site)
            {
                outStream = ps.StandardOutput.ReadLine();
                time = GetTime(outStream);

                //通过委托来调用ShowTime方法
                //之所以要用委托是因为不能夸线程直接对控件进行操作
                //所以需要通过委托，调用方法进行操作
                Invoke(new DShowTime(ShowTime), time);
                Thread.Sleep(500);
                //强行执行垃圾回收，避免循环造成的垃圾堆积内存
                GC.Collect();
            }
        }
        #endregion

        #region //从返回流字符串中截取出延迟
        private int GetTime(string line)
        {
            int result = 0;
            if (line == null || string.Empty == line.Trim())
                return 0;
            Regex rg = new Regex(@".*\=(.*\d+)ms.*");
            int.TryParse(rg.Match(line).Groups[1].Value, out result);
            if (result == 0)
                ++outCount;
            else
                outCount = 0;
            return result;
        }
        #endregion

        #region //显示网速
        private void ShowTime(int value)
        {
            if (value > 0)
                show = value;
            if (outCount <= 3 && show != 0)
            {
                label2.Text = show.ToString() + " ms";

            }
            else
            {
                label2.Text = "未连接";

            }
          }
        #endregion

        //停止按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (site)
            {
                site = false;
                StopListent();


                //停止监听，重置相关属性及状态
                show = 0;
                label2.Text = "未连接";

            }
        }

        private void StopListent()
        {
            if (ps != null)
            {
                //如果进程存在就将其杀掉
                if (!ps.HasExited)
                    ps.Kill();
                //如果线程仍在运行，强制停止
                if (tdWork.IsAlive)
                    tdWork.Abort();               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!site)
            {
                //当site为false时，则创建一个新的线程，并在新线程中执行“线程函数”
                site = true;
                tdWork = new Thread(new ThreadStart(AppRunMethod));
                tdWork.Start();
                ChangeSite();  //将开始按钮置为不可用，地址框不可写
            }
        }

        public Form1()
        {
            InitializeComponent();
        }
    }
}