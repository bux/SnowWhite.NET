using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AirStreamLib;

namespace AirStreamTVPlayer
{
    public partial class Form1 : Form
    {
        protected AirStreamServer server = new AirStreamServer(Environment.MachineName, 9001);
        protected double _rate = 0;
        protected double _initialPosition = 0;
        protected bool _isOpened = false;

        public Form1()
        {
            InitializeComponent();
            server.PlayControl += new EventHandler<PlayControlEventArgs>(server_PlayControl);
            server.PlayInfo += new EventHandler<PlayInfoEventArgs>(server_PlayInfo);
            axWindowsMediaPlayer1.OpenStateChange += new AxWMPLib._WMPOCXEvents_OpenStateChangeEventHandler(axWindowsMediaPlayer1_OpenStateChange);
            axWindowsMediaPlayer1.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(axWindowsMediaPlayer1_PlayStateChange);
            server.StartServer();
        }

        void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 0 || e.newState == 1 || e.newState == 2 || e.newState == 8 || e.newState == 10)
                _rate = 0;
            else
                _rate = 1;


            if (e.newState == 3)
            {
                server.SendStatus("playing");
            }
            else if (e.newState == 2)
            {
                server.SendStatus("paused");
            }
            else if (e.newState == 8)
            {
                server.SendStatus("stopped");
            }
        }

        void axWindowsMediaPlayer1_OpenStateChange(object sender, AxWMPLib._WMPOCXEvents_OpenStateChangeEvent e)
        {
            if (e.newState==13)
            {               
                var rate = (int)_rate;
                var startPos = axWindowsMediaPlayer1.currentMedia.duration * _initialPosition;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = startPos;
                if(rate>=1)
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                else
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                _isOpened = true;
            }
        }

        void server_PlayInfo(object sender, PlayInfoEventArgs e)
        {
            if (axWindowsMediaPlayer1.currentMedia != null)
                e.Duration = axWindowsMediaPlayer1.currentMedia.duration;
            e.Position = axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
            e.Rate = _rate;
        }

        void server_PlayControl(object sender, PlayControlEventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler<PlayControlEventArgs> handler = server_PlayControl;
                this.BeginInvoke(handler, new object[] { sender, e });
                return;
            }

            if(e is PlayControlLoadUrlEventArgs)
            {
                var ea=e as PlayControlLoadUrlEventArgs;
                axWindowsMediaPlayer1.Visible = true;
                pictureBox1.Visible = false;
                axWindowsMediaPlayer1.URL = ea.Url;
                _isOpened = false;
                _initialPosition = ea.StartPosition;
                System.Diagnostics.Debug.WriteLine("PlayControlLoadUrlEventArgs");
            }
            else if (e is PlayControlSetRateEventArgs)
            {
                var ea = e as PlayControlSetRateEventArgs;
                _rate = ea.NewRate;
                if (_isOpened)
                {
                    int rate = (int)ea.NewRate;
                    if (rate >= 1)
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    else
                        axWindowsMediaPlayer1.Ctlcontrols.pause();
                }
                System.Diagnostics.Debug.WriteLine("PlayControlSetRateEventArgs. Rate=" + ea.NewRate + " IsOpen: " + _isOpened);
            }
            else if (e is PlayControlStopEventArgs)
            {
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            }
            else if (e is PlayControlSeekEventArgs)
            {
                var ea = e as PlayControlSeekEventArgs;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = ea.NewPosition;
            }
            else if (e is PlayControlDisplayImageEventArgs)
            {
                var ea = e as PlayControlDisplayImageEventArgs;
                pictureBox1.Visible = true;
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                axWindowsMediaPlayer1.Visible = false;
                pictureBox1.Image = ea.Image;
                pictureBox1.Refresh();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            server.StopServer();
        }
    }
}
