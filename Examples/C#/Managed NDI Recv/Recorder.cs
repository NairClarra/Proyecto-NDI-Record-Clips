using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Accord.Video.FFMPEG;
using System.IO;
using System.Drawing.Imaging;

namespace Managed_NDI_Receive
{
    internal class Recorder
    {
        //video variables
        private Rectangle bounds;
        private string outputPath = "";
        private string tempPath = "";
        private int fileCount = 1;
        private List<string> inputImageSequence = new List<string>();

        //file variables
        private string audioName = "mic.wav";
        private string videoName = "video.wav";
        private string finalNAme = "finalVideo.mp4";


        //time variable
        Stopwatch watch = new Stopwatch();

        //audio variables
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        }

        public void ScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenshots");

            bounds = b;
            outputPath = outPath;
            

        }
        private void CreateTempFolder(string name) {
            if (Directory.Exists("D://"))
            {
                string pathName = $"D://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
            else
            {
                string pathName = $"C://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
        }
        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            foreach (string dir in dirs)
            {
                DeletePath(dir);
            }
            Directory.Delete(targetDir, false);
            
        }
        private void DeleteFilesExcept(string targetFile, string excFile)
        {
            string [] files= Directory.GetFiles(targetFile); 
            foreach(string file in files)
            {
                if (file != excFile)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }
        public void cleanup()
        {
            if (Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }
        public string getElapsed()
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);

        }
        public void RecordVideo()
        {
            watch.Start();

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap)){
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

                }
                string name = tempPath = "//screenshot-" + fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;

                bitmap.Dispose();

            }
            
        }
    }   

}
