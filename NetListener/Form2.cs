using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WHU.Commons;
using WindowsTest.Business;

namespace NetListener
{
    public partial class Form2 : Form
    {

        private delegate void ThreadWork(int quality);

        string ServerIP = "";
        int StardardBandwidth = 1024;
        Thread NetworkChecker = null;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            NetworkChecker = new Thread(new ThreadStart(SystemMonitoring));

            NetworkChecker.IsBackground = true;
            NetworkChecker.Start();
            AppConfig ac = new AppConfig();
            ServerIP = ac.AppConfigGet("ServerIP");
            StardardBandwidth = int.Parse(ac.AppConfigGet("StardardBandwidth"));
            int quality = NetworkCheck();

            label1.Text = quality.ToString();

        }

        private void SystemMonitoring()
        {
            Thread.Sleep(1000);
            while (true)
            {

                //关键，返回网络质量数据
                int quality = NetworkCheck();

                UpdateMonitor(quality);

                Thread.Sleep(5000);
            }
        }
        private int NetworkCheck()
            {
                int result = 0;
                float nw = Ping.Test(ServerIP);
                result = (int)(nw / StardardBandwidth * 100);//以作为标准带宽，后期可调。
                result %= 100;
                return result;

            }
        //更新显示器
        private void UpdateMonitor(int quality)
        {
            if (this.label1.InvokeRequired)//等待异步 
            { 
            
                ThreadWork fc = new ThreadWork(UpdateMonitor);
                // this.Invoke(fc);//通过代理调用刷新方法 
                this.Invoke(fc, new object[1] { quality });
            }
            else
            {
                label1.Text = quality.ToString();
                if (quality > 80)
                {
                    label1.ForeColor = Color.Green;
                }
                else if (quality > 60)
                {
                    label1.ForeColor = Color.Orange;
                }
                else if (quality > 40)
                {
                    label1.ForeColor = Color.Yellow;
                }
                else
                {
                    label1.ForeColor = Color.Red;
                }
            }
        }

    }
}
