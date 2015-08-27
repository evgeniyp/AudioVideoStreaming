using AForge.Video.DirectShow;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsExample
{
    public partial class Form1 : Form
    {
        VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = eventArgs.Frame.Clone(new Rectangle(0, 0, eventArgs.Frame.Width, eventArgs.Frame.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            });

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoSource.SignalToStop();
        }
    }
}
