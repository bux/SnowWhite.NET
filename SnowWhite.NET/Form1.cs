using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SnowWhite.NET
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private AirPlayHandler aph;
        private void button1_Click(object sender, EventArgs e)
        {

            aph = new AirPlayHandler();

            aph.StartBonjour();

            aph.StartServers();

            m_btnStart.BackColor = Color.Green;
            m_btnStart.Enabled = false;

            m_btnStop.Enabled = true;
            m_btnStop.BackColor = Color.Gainsboro;


        }

        private void m_btnStop_Click(object sender, EventArgs e)
        {
            aph.StopEverything();
            aph = null;

            m_btnStop.BackColor = Color.Green;
            m_btnStop.Enabled = false;

            m_btnStart.Enabled = true;
            m_btnStart.BackColor = Color.Gainsboro;
        }
    }
}
