using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CodeUSAClient.Properties;

namespace CodeUSAClient
{
    public class StartupWindow : Form
    {
        public const int HT_CAPTION = 2;
        public const int WM_NCLBUTTONDOWN = 0xa1;
        public static GameClient clientInstance;
        public static StartupWindow instance;
        private static float version = 11f;
        private IContainer components;
        private bool hasUpdated;
        private SoundPlayer simpleSound;
        private UISystem.UIProgressBar updateBar;
        private Thread updaterThread;
        private Rectangle visibleRegion;

        public StartupWindow()
        {
            Action onClickAction = null;
            Action action2 = null;
            simpleSound = new SoundPlayer("./data/title.wav");
            ThreadStart start = null;
            if (File.Exists(Application.StartupPath + @"\Relaunch.bat"))
            {
                File.Delete(Application.StartupPath + @"\Relaunch.bat");
            }
            foreach (var process in Process.GetProcesses())
            {
                if ((process.ProcessName == "CodeUSAClient") && (process.Id != Process.GetCurrentProcess().Id))
                {
                    MessageBox.Show("Sorry only one instance of Summit can be run at a time!", "Duplicate Process",
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Process.GetCurrentProcess().Kill();
                }
            }
            InitializeComponent();
            base.SetStyle(
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            Settings.Load();
            if (onClickAction == null)
            {
                onClickAction = delegate
                {
                    updaterThread = new Thread(() =>
                    {
                        updateBar.SetText("Downloading Update", 0);
                        var response =
                            (HttpWebResponse)
                                WebRequest.Create("http://codeusa.net/play/CodeUSAClient.exe").GetResponse();
                        using (var reader = new BinaryReader(response.GetResponseStream()))
                        {
                            var count = 0;
                            var num2 = 0;
                            using (
                                var stream = new FileStream(Application.StartupPath + @"\client.dat",
                                    FileMode.OpenOrCreate))
                            {
                                byte[] buffer;
                                Label_0052:
                                buffer = new byte[0x400];
                                num2 += count = reader.Read(buffer, 0, buffer.Length);
                                if (count >= 1)
                                {
                                    stream.Write(buffer, 0, count);
                                    var num3 = num2/((float) response.ContentLength);
                                    var num4 = (int) (num3*100f);
                                    updateBar.SetText("Downloading Update... " + num4 + "%", num4);
                                    goto Label_0052;
                                }
                                updateBar.SetText("Downloaded Update!", 100);
                                stream.Close();
                                File.WriteAllText(Application.StartupPath + @"\Relaunch.bat",
                                    "@echo off\ndel CodeUSAClient.exe\nren client.dat CodeUSAClient.exe\nstart CodeUSAClient.exe");
                                Process.Start(Application.StartupPath + @"\Relaunch.bat");
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                    });
                    updaterThread.Start();
                };
            }
            var updateButton = new UISystem.UIButton(base.Width - 400, base.Height - 40, 170, 20, "Update Game Client",
                onClickAction)
            {
                Enabled = false
            };
            base.Controls.Add(updateButton);

            var playButton = new UISystem.UIButton(base.Width - 200, base.Height - 40, 170, 20, "Launch Game Client",
                () =>
                {
                    stopSimpleSound();
                    clientInstance = new GameClient();
                    clientInstance.Show();
                    base.Hide();
                })
            {
                Enabled = false
            };
            base.Controls.Add(playButton);
            var loginBox = new UISystem.UILabelBox(updateButton.Left, base.Height - 0xc3,
                playButton.Right - updateButton.Left, 150, "Please Login to Play\n\nUsername:\n\nPassword:", 0)
            {
                labelFont = UISystem.UITheme.bigFont
            };
            var valueTrusted = "";
            var str2 = "";
            if (Settings.GetValue<bool>("Login.RememberLoginDetails"))
            {
                valueTrusted = Settings.GetValueTrusted<string>("Login.Username");
                str2 = Settings.GetValueTrusted<string>("Login.Password");
            }
            var usernameBox = new UISystem.UITextBox(80, 0x38, loginBox.Width/2, 0x19)
            {
                MaxLength = 12,
                Text = valueTrusted
            };
            loginBox.Controls.Add(usernameBox);
            var passwordBox = new UISystem.UITextBox(80, 0x66, loginBox.Width/2, 0x19)
            {
                MaxLength = 20,
                Text = str2,
                UseSystemPasswordChar = true
            };
            loginBox.Controls.Add(passwordBox);
            loginBox.Controls.Add(new UISystem.UIButton(loginBox.Width - 0x4b, loginBox.Height - 0x39, 0x47, 0x19,
                "Register", () => Process.Start("http://codeusa.net/forums/register.php")));
            loginBox.Controls.Add(new UISystem.UIButton(loginBox.Width - 0x4b, loginBox.Height - 0x1d, 0x47, 0x19,
                "Login", delegate(UISystem.UIButton element)
                {
                    if (usernameBox.Text.Trim().Length < 3)
                    {
                        MessageBox.Show("Your username must be greater than 3 characters", "Oops!", MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk);
                    }
                    else if (passwordBox.Text.Trim().Length == 0)
                    {
                        MessageBox.Show("You did not enter a password", "Oops!", MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        var client = new WebClient();
                        try
                        {
                            var str =
                                client.DownloadString("http://codeusa.net/play/login.php?u=" + usernameBox.Text.Trim() +
                                                      "&p=" + passwordBox.Text.Trim());
                            if (str != "false")
                            {
                                element.buttonText = "Logged In";
                                element.Enabled = false;
                                playButton.Enabled = true;
                                GameClient.forumUsername = str;
                                if (Settings.GetValue<bool>("Login.RememberLoginDetails"))
                                {
                                    Settings.SetValue("Login.Username", usernameBox.Text);
                                    Settings.SetValue("Login.Password", passwordBox.Text);
                                    Settings.Save();
                                }
                                else
                                {
                                    Settings.SetValue("Login.Username", "");
                                    Settings.SetValue("Login.Password", "");
                                    Settings.Save();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid Username/Password combination", "Oops!", MessageBoxButtons.OK,
                                    MessageBoxIcon.Asterisk);
                            }
                        }
                        catch (WebException)
                        {
                            MessageBox.Show("Client could not connect to the server!", "Oops!", MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);
                        }
                        finally
                        {
                            if (client != null)
                            {
                                client.Dispose();
                            }
                        }
                    }
                }));
            loginBox.Controls.Add(new UISystem.UICheckBox(passwordBox.Left, passwordBox.Bottom + 2, "Remember Login",
                Settings.GetSetting("Login.RememberLoginDetails")));
            base.Controls.Add(loginBox);
            base.Controls.Add(new UISystem.UINewsBox(20, base.Height - 120, 0x174, 100,
                "http://codeusa.net/forums/external.php?forumids=2&type=xml"));
            base.Controls.Add(updateBar = new UISystem.UIProgressBar(20, base.Height - 0xaf, 0x174, 50, 100));
            if (Settings.GetValue<bool>("Client.AutoUpdate"))
            {
                updaterThread = new Thread(() =>
                {
                    Action method = null;
                    updateBar.SetText("Checking for Loader Update...", 0);
                    try
                    {
                        if (
                            float.Parse(
                                Encoding.ASCII.GetString(StreamFile("http://codeusa.net/play/version", "Version"))) >
                            version)
                        {
                            Invoke(new MethodInvoker(() => updateButton.Enabled = true));
                        }
                        else
                        {
                            var strArray =
                                Encoding.ASCII.GetString(StreamFile("http://codeusa.net/play/config.php?do=list",
                                    "Resource List")).Split(new[] {'\n'});
                            var num = 0;
                            foreach (var str in strArray)
                            {
                                if (str != "")
                                {
                                    var strArray2 = str.Split(new[] {'#'});
                                    string[] strArray3 =
                                    {
                                        strArray2[0].Substring(0, strArray2[0].LastIndexOf("/")),
                                        strArray2[0].Substring(strArray2[0].LastIndexOf("/") + 1)
                                    };
                                    var info = new FileInfo("./" + strArray2[0]);
                                    if (!info.Exists || (info.Length != long.Parse(strArray2[1])))
                                    {
                                        Debug.PrintLine("Downloading resource: " + str + " | " + info.FullName);
                                        var num2 = (int) ((num++/((float) strArray.Length))*100f);
                                        updateBar.SetText("Updating..." + info.Name, num2);
                                        var bytes = StreamFile("http://www.codeusa.net/play/" + strArray2[0],
                                            "Downloading " + info.Name);
                                        Directory.CreateDirectory(strArray3[0]);
                                        File.WriteAllBytes("./" + strArray2[0], bytes);
                                    }
                                }
                            }
                            if (File.Exists("./data/title.wav"))
                            {
                                playSimpleSound();
                            }
                            updateBar.SetText("Client is Up-to-date!", 100);
                            Invoke(new MethodInvoker(() =>
                            {
                                loginBox.Enabled = true;
                                ContinueInit();
                                Settings.Load();
                            }));
                        }
                    }
                    catch (FormatException)
                    {
                    }
                    updaterThread.Abort();
                });
                updaterThread.Start();
            }
            else
            {
                updateBar.SetText("Auto Update is disabled!", 100);
                loginBox.Enabled = true;
            }
            visibleRegion = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
        }

        private void ContinueInit()
        {
            var button = new UISystem.UIButton(base.Width - 0x21, 5, 0x19, 15, "", () =>
            {
                stopSimpleSound();
                Application.Exit();
                Process.GetCurrentProcess().Kill();
                Application.ExitThread();
            })
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                buttonImage = Image.FromFile("./data/close.png")
            };
            base.Controls.Add(button);
            var button2 = new UISystem.UIButton(base.Width - 60, 5, 0x19, 15, "",
                () => base.WindowState = FormWindowState.Minimized)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                buttonImage = Image.FromFile("./data/minimize.png")
            };
            base.Controls.Add(button2);
            var box = new UISystem.UILabelBox(0, 0, base.Width, 0x19, Text, 0x11);
            box.MouseMove += MainWindow_MouseMove;
            base.Controls.Add(box);
            var box2 = new UISystem.UIPictureBox((base.Width/2) - 0xba, 30, 0x174, 0x60);
            box2.SetImage("./data/logo.png");
            base.Controls.Add(box2);
            var button3 = new UISystem.UIButton(10, 30, 0x20, 0x20, "",
                () => Process.Start("https://www.facebook.com/CodeUSASoftware"))
            {
                buttonImage = Image.FromFile("./data/facebook.png")
            };
            base.Controls.Add(button3);
            var button4 = new UISystem.UIButton(0x2c, 30, 0x20, 0x20, "",
                () => Process.Start("http://www.youtube.com/user/CodeusaSoftware"))
            {
                buttonImage = Image.FromFile("./data/you-tube.png")
            };
            base.Controls.Add(button4);
            var button5 = new UISystem.UIButton(0x4e, 30, 0x20, 0x20, "",
                () => Process.Start("http://www.twitter.com/#!/CodeusaSoftware"))
            {
                buttonImage = Image.FromFile("./data/twitter.png")
            };
            base.Controls.Add(button5);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DownloadFile(string address, string localPath, string displayText)
        {
            updateBar.SetText("Downloading File...", 0);
            var response = (HttpWebResponse) WebRequest.Create(address).GetResponse();
            using (var reader = new BinaryReader(response.GetResponseStream()))
            {
                var count = 0;
                var num2 = 0;
                using (
                    var stream = new FileStream(Application.StartupPath + @"\" + localPath.Replace("/", @"\"),
                        FileMode.OpenOrCreate))
                {
                    byte[] buffer;
                    Label_005E:
                    buffer = new byte[0x400];
                    num2 += count = reader.Read(buffer, 0, buffer.Length);
                    if (count >= 1)
                    {
                        stream.Write(buffer, 0, count);
                        var num3 = num2/((float) response.ContentLength);
                        var num4 = (int) (num3*100f);
                        updateBar.SetText(string.Concat(new object[] {displayText, " - ", num4, "%"}), num4);
                        goto Label_005E;
                    }
                    updateBar.SetText("Downloaded " + displayText, 100);
                    stream.Close();
                }
            }
        }

        public void ExecuteCommandSync(object command)
        {
            try
            {
                var info = new ProcessStartInfo("cmd", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var process = new Process
                {
                    StartInfo = info
                };
                process.Start();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }
            catch (Exception)
            {
            }
        }

        private void InitializeComponent()
        {
            var manager = new ComponentResourceManager(typeof (StartupWindow));
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            BackgroundImage = Resources.bg1;
            BackgroundImageLayout = ImageLayout.Stretch;
            base.ClientSize = new Size(0x345, 0x202);
            base.FormBorderStyle = FormBorderStyle.None;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.Name = "StartupWindow";
            base.StartPosition = FormStartPosition.CenterScreen;
            Text = "                                                                      CodeUSA - Summit Loader";
            base.Load += StartupWindow_Load;
            base.Paint += MainWindow_Paint;
            base.MouseMove += MainWindow_MouseMove;
            base.ResumeLayout(false);
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(base.Handle, 0xa1, 2, 0);
            }
        }

        private void MainWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(UISystem.UITheme.traceColour, visibleRegion);
        }

        private void playSimpleSound()
        {
            simpleSound.PlayLooping();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void StartupWindow_Load(object sender, EventArgs e)
        {
        }

        private void stopSimpleSound()
        {
            simpleSound.Stop();
            simpleSound.Dispose();
        }

        private byte[] StreamFile(string address, string displayText)
        {
            var list = new List<byte>();
            try
            {
                var response = (HttpWebResponse) WebRequest.Create(address).GetResponse();
                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    var num = 0;
                    var num2 = 0;
                    while (true)
                    {
                        var buffer = new byte[0x400];
                        num2 += num = reader.Read(buffer, 0, buffer.Length);
                        if (num < 1)
                        {
                            goto Label_009E;
                        }
                        for (var i = 0; i < num; i++)
                        {
                            list.Add(buffer[i]);
                        }
                        var num4 = num2/((float) response.ContentLength);
                    }
                }
            }
            catch (WebException)
            {
                updateBar.SetText("Unable to connect to Server", 100);
            }
            Label_009E:
            return list.ToArray();
        }
    }
}