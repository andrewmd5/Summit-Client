using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace CodeUSAClient
{
    public class PreviewScreenshot : Form
    {
        private IContainer components;
        private ToolStripMenuItem discardToolStripMenuItem1;
        private MenuStrip menuStrip1;
        private PictureBox pictureBox1;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private UISystem.UIButton uploadButton;
        private ToolStripMenuItem uploadToolStripMenuItem;

        public PreviewScreenshot(Image image)
        {
            InitializeComponent();
            pictureBox1.Image = image;
            base.Width = image.Width;
            base.Height = image.Height + menuStrip1.Height;
            Text = Text + " | " + DateTime.Now;
            Graphics.FromImage(pictureBox1.Image)
                .DrawString(
                    "Screenshot taken by Summit Client - CodeUSA.net | Taken on " + DateTime.Now.ToLongDateString() +
                    " " + DateTime.Now.ToLongTimeString() + " " + TimeZone.CurrentTimeZone.StandardName,
                    new Font("Tahoma", 8f), Brushes.White, new PointF(0f, 0f));
        }

        private void discardToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void discardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var manager = new ComponentResourceManager(typeof (PreviewScreenshot));
            menuStrip1 = new MenuStrip();
            uploadToolStripMenuItem = new ToolStripMenuItem();
            discardToolStripMenuItem1 = new ToolStripMenuItem();
            pictureBox1 = new PictureBox();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar1 = new ToolStripProgressBar();
            menuStrip1.SuspendLayout();
            ((ISupportInitialize) pictureBox1).BeginInit();
            statusStrip1.SuspendLayout();
            base.SuspendLayout();
            menuStrip1.Items.AddRange(new ToolStripItem[] {uploadToolStripMenuItem, discardToolStripMenuItem1});
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(0x173, 0x18);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            uploadToolStripMenuItem.Name = "uploadToolStripMenuItem";
            uploadToolStripMenuItem.Size = new Size(0x39, 20);
            uploadToolStripMenuItem.Text = "Upload";
            uploadToolStripMenuItem.Click += uploadToolStripMenuItem_Click;
            discardToolStripMenuItem1.Name = "discardToolStripMenuItem1";
            discardToolStripMenuItem1.Size = new Size(0x3a, 20);
            discardToolStripMenuItem1.Text = "Discard";
            discardToolStripMenuItem1.Click += discardToolStripMenuItem1_Click;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0x18);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(0x173, 0xeb);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            statusStrip1.Items.AddRange(new ToolStripItem[] {toolStripProgressBar1});
            statusStrip1.Location = new Point(0, 0xed);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(0x173, 0x16);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 0x10);
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x173, 0x103);
            base.Controls.Add(statusStrip1);
            base.Controls.Add(pictureBox1);
            base.Controls.Add(menuStrip1);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.MainMenuStrip = menuStrip1;
            base.Name = "PreviewScreenshot";
            Text = "Screenshot Preview";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((ISupportInitialize) pictureBox1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public void PostToImgur(Image image, UISystem.UIButton button)
        {
            uploadButton = button;
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            var inArray = stream.GetBuffer();
            using (var client = new WebClient())
            {
                client.UploadProgressChanged += w_UploadProgressChanged;
                client.UploadValuesCompleted += w_UploadValuesCompleted;
                var values2 = new NameValueCollection();
                values2.Add("key", "eeb1018be96322f46df76ceb36576e08");
                values2.Add("image", Convert.ToBase64String(inArray));
                var data = values2;
                client.UploadValuesAsync(new Uri("http://imgur.com/api/upload.xml"), data);
            }
        }

        private void SetProgressBarMax(int value)
        {
            toolStripProgressBar1.Maximum = value;
        }

        private void SetProgressBarValue(int value)
        {
            if (value > 0)
            {
                toolStripProgressBar1.Value = value;
            }
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PostToImgur(pictureBox1.Image, null);
        }

        private void w_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            SetProgressBarValue(e.ProgressPercentage);
            if (uploadButton != null)
            {
                uploadButton.buttonText = "Uploading - " + (e.ProgressPercentage*2) + "%";
                uploadButton.Invalidate();
            }
        }

        private void w_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            var str = Encoding.ASCII.GetString(e.Result);
            var startIndex = str.IndexOf("<original_image>") + 0x10;
            var index = str.IndexOf("</original_image>");
            var text = str.Substring(startIndex, index - startIndex);
            if (uploadButton != null)
            {
                uploadButton.buttonText = "Take Screenshot";
                uploadButton.Invalidate();
            }
            Clipboard.SetText(text);
            MessageBox.Show("URL Copied to clipboard");
        }
    }
}