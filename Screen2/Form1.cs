using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Screen2
{
    public partial class Form1 : Form
    {
        private System.Threading.Timer timer;
        private TcpClient tcpClient;
        private string serverIP = "192.168.0.1";
        private int serverPort = 8081;
        private Panel mainPanel1;
        private Image capturedImage;
        private PictureBox pictureBox1;

        public Form1()
        {
            InitializeComponent();
            InitializePictureBox();
            button2.Click += new EventHandler(button2_Click);
        }
        private Panel mainPanel;

        private void CaptureScreen(object state)
        {
            try
            {
                Rectangle rectangle = Screen.GetBounds(Point.Empty);
                using (Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, rectangle.Size);
                    }
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] buf = ms.ToArray();
                        tcpClient = new TcpClient();
                        tcpClient.Connect(serverIP, serverPort);
                        NetworkStream nStream = tcpClient.GetStream();
                        nStream.Write(buf, 0, buf.Length);
                        nStream.Close();
                        tcpClient.Close();

                        ms.Seek(0, SeekOrigin.Begin);
                        capturedImage = Image.FromStream(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void InitializePictureBox()
        {
            mainPanel1 = new Panel();
            mainPanel1.BackgroundImageLayout = ImageLayout.Stretch;
            mainPanel1.Dock = DockStyle.Fill;
            this.Controls.Add(mainPanel1);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            timer = new System.Threading.Timer(CaptureScreen, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Application.Idle += new EventHandler(Application_Idle);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            mainPanel1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Dispose();
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CaptureScreen(null);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (capturedImage != null)
            {
                mainPanel1.BackgroundImage = capturedImage;
            }
        }
    }
}
