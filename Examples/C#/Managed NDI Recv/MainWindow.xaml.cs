using NewTek;
using NewTek.NDI;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using Accord.Video.FFMPEG;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

using DotNetPusher.Pushers;
using System.Runtime.InteropServices;

namespace Managed_NDI_Receive
{

    
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hdc);

        public int recStart = 0;
        public int serverStarted = 0;
        public WebSocketServer wssv = new WebSocketServer("ws://localhost:8050");
  
        public string byteToString(byte[] buffer)
        {
            string message;
            int endIndex;
            message = Encoding.ASCII.GetString(buffer);
            endIndex = message.IndexOf('\0');
            if (endIndex > 0) message = message.Substring(0, endIndex);
            return message;
        }
        public DotNetPusher.Encoders.Encoder encoder = new DotNetPusher.Encoders.Encoder(1920,1080,25, 1024*800);
        public MainWindow()
        {
           
            InitializeComponent();
          

            // Not required, but "correct". (see the SDK documentation)
            if (!NDIlib.initialize())
            {
                // Cannot run NDI. Most likely because the CPU is not sufficient (see SDK documentation).
                // you can check this directly with a call to NDIlib.is_supported_CPU()
                if (!NDIlib.is_supported_CPU())
                {
                    MessageBox.Show("CPU unsupported.");
                }
                else
                {
                    // not sure why, but it's not going to run
                    MessageBox.Show("Cannot run NDI.");
                }

                // we can't go on
                Close();
            }
            
            
        }

        // properly dispose of the unmanaged objects
        protected override void OnClosed(EventArgs e)
        {
            if (ReceiveViewer != null)
                ReceiveViewer.Dispose();

            if (_findInstance != null)
                _findInstance.Dispose();

            base.OnClosed(e);
        }
        private WriteableBitmap VideoBitmap;
        // This will find NDI sources on the network.
        // Continually updated as new sources arrive.
        // Note that this example does see local sources (new Finder(true))
        // This is for ease of testing, but normally is not needed in released products.
        public Finder FindInstance
        {
            get { return _findInstance; }
        }
        private Finder _findInstance = new Finder(true);

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            NewTek.NDI.WPF.ReceiveView.recStart = 1;

            
        }

        private void SourcesSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Server_Click(object sender, RoutedEventArgs e)
        {

            if (serverStarted == 0)
            {
                wssv.AddWebSocketService<Echo>("/Echo");
                wssv.Start();
                serverStarted = 1;
            }
            else
            {
                wssv.Stop();
                serverStarted = 0;
            }

           

        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {

            NewTek.NDI.WPF.ReceiveView.recStart = 2;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

            NewTek.NDI.WPF.ReceiveView.recStart = 0;
        }
    }
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "RECORD") NewTek.NDI.WPF.ReceiveView.recStart = 1;
            else if ( e.Data.Contains("FILENAME") )
            {                NewTek.NDI.WPF.ReceiveView.fileName = e.Data.Split('|')[1];
            }
            else if (e.Data == "STOP") NewTek.NDI.WPF.ReceiveView.recStart = 0;
            else if(e.Data=="PAUSE") NewTek.NDI.WPF.ReceiveView.recStart = 2;

        }
    }
}
