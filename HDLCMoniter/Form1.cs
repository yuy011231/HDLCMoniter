using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HDLCMoniter
{
    public partial class Form1 : Form
    {
        private Timer timer = new Timer();
        private HDLC _hdlc = new HDLC(); 
        public Form1()
        {
            InitializeComponent();

            _hdlc.OpenPort("FBIHDLC49");

            FileIO.Write("↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓\r\n");
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            List<byte> data = new List<byte>();
            if (_hdlc.Receive(ref data))
            {
                string dataStr = ToStringByteList(data);
                Moniter.Text = dataStr;
                FileIO.Write(dataStr + "\r\n");
            }
            else
            {
                Moniter.Text = "No Receive";
                FileIO.Write("No Receivec\r\n");
            }
        }

        private string ToStringByteList(List<byte> data)
        {
            string ret = string.Empty;
            foreach(byte val in data)
            {
                ret += " " + val.ToString("X2");
            }
            return ret;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer.Stop();
            _hdlc.ClosePort();
        }
    }
}
