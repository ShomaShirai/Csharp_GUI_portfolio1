using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;

namespace ss2409_pre
{
    public partial class Form1 : Form
    {
        private ComboBox comboBox2;
        private VideoCapture capture;
        private bool isRunning = false;
        private Task captureTask;
        private string _captureMode;

        public Form1()
        {
            InitializeComponent();
            button2.Click += new EventHandler(Button2_Click);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            this.Text = "Arucoマーカ GUI";
            this.Icon = new Icon("C:\\Users\\takos\\source\\repos\\ss2409_pre\\ss2409_pre\\favicon.ico");
        }

        private async void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isRunning)
            {
                await StopCamera();
                button2.Text = "開始";
            }
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                await StopCamera();
                button2.Text = "開始";
            }
            else
            {
                _captureMode = comboBox1.SelectedItem.ToString();

                // モードがDummyの場合は早期リターン
                if (_captureMode == "Dummy")
                {
                    MessageBox.Show("Dummyカメラは存在しません");
                    return;
                }

                if (await StartCamera())
                {
                    button2.Text = "停止";
                }
                else
                {
                    MessageBox.Show("カメラの起動に失敗しました。");
                }
            }
        }

        private async Task<bool> StartCamera()
        {
            try
            {
                if (_captureMode == "USBカメラ")
                {
                    capture = new VideoCapture(0);

                    if (capture == null || !capture.IsOpened())
                    {
                        MessageBox.Show("カメラが見つかりませんでした。");
                        return false;
                    }

                    isRunning = true;
                    captureTask = Task.Run(async () => await CaptureLoop());
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"キャプチャの起動中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }

        private async Task StopCamera()
        {
            isRunning = false;
            if (captureTask != null)
            {
                await captureTask;
            }
            capture?.Dispose();
            capture = null;

            if (pictureBox2.Image != null)
            {
                var temp = pictureBox2.Image;
                pictureBox2.Image = null;
                temp.Dispose();
            }
        }

        private async Task CaptureLoop()
        {
            using (var frame = new Mat())
            {
                while (isRunning)
                {
                    try
                    {
                        Bitmap bitmap = null;
                        if (_captureMode == "USBカメラ") // カメラモード
                        {
                            if (!capture.Read(frame) || frame.Empty())
                                continue;
                            bitmap = BitmapConverter.ToBitmap(frame);
                        }

                        if (bitmap != null)
                        {
                            if (pictureBox2.InvokeRequired)
                            {
                                pictureBox2.Invoke(new Action(() =>
                                {
                                    var oldImage = pictureBox2.Image;
                                    pictureBox2.Image = bitmap;
                                    oldImage?.Dispose();
                                }));
                            }
                            else
                            {
                                var oldImage = pictureBox2.Image;
                                pictureBox2.Image = bitmap;
                                oldImage?.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"フレーム処理中にエラーが発生: {ex.Message}");
                    }
                    await Task.Delay(30); // フレームレートの制御
                }
            }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            await StopCamera();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}