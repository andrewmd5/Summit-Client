using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CodeUSAClient
{
    public class GameClient : Form
    {
        public const int HT_CAPTION = 2;
        public const int WM_NCLBUTTONDOWN = 0xa1;
        public static string forumUsername = "";

        public static string[] skills =
        {
            "attack", "defence", "strength", "constitution", "range", "prayer", "magic", "cooking", "woodcutting",
            "fletching", "fishing", "firemaking", "crafting", "smithing", "mining", "herblore",
            "agility", "thieving", "slayer", "farming", "runecrafting", "hunter", "construction", "summoning",
            "dungeoneering"
        };

        private UISystem.UIBrowser clientBrowser;
        private IContainer components;
        private bool expanded;
        private UISystem.UIButton findButton;
        public UISystem.UILabelBox formTitleBar;
        private UISystem.UIButton hiscoresButton;
        private UISystem.UISimplePanel hiscoresPanel;
        private UISystem.UILabelBox infoBox;
        private bool minimized;
        private Options optionsWindow;
        private UISystem.UIButton panelButton;
        private UISystem.UITextBox playerTextbox;
        private UISystem.UIButton screenshotButton;
        private UISystem.UISimplePanel screenshotPanel;

        private List<PictureBox> skillsImage;
        private UISystem.UISimplePanel skillsPanel;
        private List<Label> skillsText;
        private UISystem.UIButton takeScreenshotButton;
        private UISystem.UISimplePanel toolPanel;
        private int toolPanelWidth;
        private ToolTip toolTip1;
        private Rectangle viewportRectangle;
        private Rectangle visibleRegion;

        public GameClient()
        {
            Action onClickAction = null;
            Action action2 = null;
            Action action3 = null;
            Action action4 = null;
            Action action5 = null;
            Action action6 = null;
            Action action7 = null;
            Action action8 = null;
            expanded = true;
            InitializeComponent();
            Settings.Load();
            base.Size = new Size(Settings.GetValue<int>("Resolution.Width"), Settings.GetValue<int>("Resolution.Height"));
            skillsImage = new List<PictureBox>();
            skillsText = new List<Label>();
            Text = Text + " | Logged in as " + forumUsername;
            MinimumSize = new Size(800, 600);
            var browser = new UISystem.UIBrowser(5, 30, base.Width - 270, base.Height - 50,
                "http://codeusa.net/play/web.html")
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top
            };
            base.Controls.Add(clientBrowser = browser);
            var panel = new UISystem.UISimplePanel(clientBrowser.Right + 5, 30, (base.Width - clientBrowser.Right) - 10,
                ((int) (base.Height*0.75f)) - 50)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            base.Controls.Add(toolPanel = panel);
            onClickAction = () =>
            {
                clientBrowser.Dispose();
                browser = new UISystem.UIBrowser(5, 30, base.Width - 270, base.Height - 50,
                    "http://codeusa.net/play/web.html")
                {
                    Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top
                };
                base.Controls.Add(clientBrowser = browser);
            };
            var button = new UISystem.UIButton(toolPanel.Left, toolPanel.Bottom + 5, toolPanel.Width, 0x19,
                "Reload Applet", onClickAction)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            base.Controls.Add(button);
            if (action2 == null)
            {
                action2 = () =>
                {
                    if (!hiscoresPanel.Visible)
                    {
                        hiscoresPanel.Visible = true;
                        screenshotPanel.Visible = false;
                        hiscoresButton.Resize(0, 4);
                        screenshotButton.Resize(0, -4);
                    }
                };
            }
            var button2 = new UISystem.UIButton(2, 2, 0x4c, 0x1a, "Hiscores", action2)
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            toolPanel.Controls.Add(hiscoresButton = button2);
            var panel2 = new UISystem.UISimplePanel(2, 30, toolPanel.Width - 4, toolPanel.Height - 0x20)
            {
                Visible = true
            };
            toolPanel.Controls.Add(hiscoresPanel = panel2);
            if (action3 == null)
            {
                action3 = () =>
                {
                    if (!screenshotPanel.Visible)
                    {
                        hiscoresPanel.Visible = false;
                        screenshotPanel.Visible = true;
                        hiscoresButton.Resize(0, -4);
                        screenshotButton.Resize(0, 4);
                    }
                };
            }
            var button3 = new UISystem.UIButton(80, 2, 0x4c, 0x16, "Screenshots", action3)
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            toolPanel.Controls.Add(screenshotButton = button3);
            var panel3 = new UISystem.UISimplePanel(2, 30, toolPanel.Width - 4, toolPanel.Height - 0x20)
            {
                Visible = false
            };
            toolPanel.Controls.Add(screenshotPanel = panel3);
            var autoCompleteValues = new List<string>();
            if (Settings.GetValue<bool>("Hiscores.AutoComplete"))
            {
                using (var client = new WebClient())
                {
                    autoCompleteValues.AddRange(
                        client.DownloadString("http://codeusa.net/play/list.php").Split(new[] {'\n'}));
                }
            }
            hiscoresPanel.Controls.Add(playerTextbox = new UISystem.UITextBox(2, 3, 0x9d, 20, autoCompleteValues));
            playerTextbox.KeyPress += playerTextbox_KeyPress;
            hiscoresPanel.Controls.Add(skillsPanel = new UISystem.UISimplePanel(0, 0x1a, hiscoresPanel.Width, 350));
            var x = 2;
            var y = 5;
            var index = 0;
            while (index < skills.Length)
            {
                if ((x + 0x37) >= hiscoresPanel.Width)
                {
                    x = 2;
                    y += 0x24;
                }
                Image image = null;
                if (File.Exists("./data/skill/" + skills[index] + ".png"))
                {
                    image = Image.FromFile("./data/skill/" + skills[index] + ".png");
                }
                else
                {
                    Debug.PrintLine("There was an error downloading the skill image for: " + skills[index]);
                }
                var item = new PictureBox
                {
                    Location = new Point(x, y),
                    Size = new Size(0x20, 0x20),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Image = image
                };
                var label = new Label
                {
                    Visible = true,
                    AutoSize = true,
                    Location = new Point(item.Location.X + 0x20, item.Location.Y + 8),
                    Text = "0",
                    ForeColor = Color.White
                };
                skillsImage.Add(item);
                skillsText.Add(label);
                skillsPanel.Controls.AddRange(new Control[] {item, label});
                index++;
                x += 0x37;
            }
            var box4 = new UISystem.UILabelBox(2, skillsPanel.Height - 0x5e, 180, 0x5c, "No User", 1)
            {
                labelFont = UISystem.UITheme.normalFont
            };
            skillsPanel.Controls.Add(infoBox = box4);
            if (action4 == null)
            {
                action4 = delegate
                {
                    Thread t = null;
                    new Thread(() =>
                    {
                        if ((playerTextbox.Text = playerTextbox.Text.Trim()).Length == 0)
                        {
                            infoBox.SetText("No player name entered!");
                        }
                        else
                        {
                            infoBox.SetText("Looking up " + playerTextbox.Text + "...");
                            using (var client = new WebClient())
                            {
                                var strArray =
                                    client.DownloadString("http://codeusa.net/hiscores/lookup.php?user=" +
                                                          playerTextbox.Text)
                                        .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                                if (strArray[0] == "false")
                                {
                                    MessageBox.Show(playerTextbox.Text + " is not in our hiscores!",
                                        "Sorry, we could not find that player!", MessageBoxButtons.OK,
                                        MessageBoxIcon.Asterisk);
                                    infoBox.SetText("Could not find " + playerTextbox.Text);
                                    return;
                                }
                                var totalLevel = 0;
                                uint totalXp = 0;
                                for (var i = 1; i < strArray.Length; i++)
                                {
                                    var info = strArray[i].Split(new[] {'#'});
                                    totalLevel += int.Parse(info[1]);
                                    totalXp += (uint) int.Parse(info[2].Replace(",", ""));
                                    var toolText =
                                        string.Concat(new object[]
                                        {
                                            char.ToUpper(info[0][0]), info[0].Substring(1), "\nRank: ",
                                            client.DownloadString(
                                                "http://codeusa.net/hiscores/lookup.php?do=rank&query=" + info[0] +
                                                "xp&user=" + playerTextbox.Text),
                                            "\nExperience: ", info[2]
                                        });
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        skillsText[i - 1].Text = info[1];
                                        toolTip1.SetToolTip(skillsText[i - 1], toolText);
                                        toolTip1.SetToolTip(skillsImage[i - 1], toolText);
                                    }));
                                }
                                var overallRank =
                                    client.DownloadString("http://codeusa.net/hiscores/lookup.php?do=rank&user=" +
                                                          playerTextbox.Text);
                                Invoke(
                                    new MethodInvoker(
                                        () =>
                                            infoBox.SetText(
                                                string.Concat(new object[]
                                                {
                                                    "Username: ", playerTextbox.Text, "\nRank: ", overallRank,
                                                    "\nTotal Level: ", totalLevel, "\nTotal XP: ",
                                                    totalXp.ToString("0,0", CultureInfo.InvariantCulture)
                                                }))));
                            }
                            Thread.CurrentThread.Abort();
                        }
                    }).Start();
                };
            }
            hiscoresPanel.Controls.Add(findButton = new UISystem.UIButton(160, 2, 40, 0x16, "Find", action4));
            if (action5 == null)
            {
                action5 = () =>
                {
                    Image image = new Bitmap(clientBrowser.Width, clientBrowser.Height);
                    var graphics = Graphics.FromImage(image);
                    var upperLeftSource = new Point(base.Location.X + clientBrowser.Left,
                        base.Location.Y + clientBrowser.Top);
                    graphics.CopyFromScreen(upperLeftSource, new Point(0, 0), clientBrowser.Size);
                    var filename = "./" + Settings.GetValue<string>("Screenshot.ImageDirectory") + "/" +
                                   DateTime.Now.ToString().Replace("/", ".").Replace(":", "-") + ".png";
                    if (!Settings.GetValue<bool>("Screenshot.DontSave"))
                    {
                        image.Save(filename, ImageFormat.Png);
                        if (!File.Exists(filename))
                        {
                            MessageBox.Show("There was an error saving the screenshot to:\n" + filename, "Oops!",
                                MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                    }
                    if (!Settings.GetValue<bool>("Screenshot.PreviewBeforeUpload"))
                    {
                        new PreviewScreenshot(image).PostToImgur(image, takeScreenshotButton);
                    }
                    else
                    {
                        new PreviewScreenshot(image).Show();
                    }
                };
            }
            screenshotPanel.Controls.Add(
                takeScreenshotButton =
                    new UISystem.UIButton(2, 2, screenshotPanel.Width - 4, 0x18, "Take Screenshot", action5));
            if (action6 == null)
            {
                action6 = delegate
                {
                    if ((optionsWindow == null) || optionsWindow.IsDisposed)
                    {
                        optionsWindow = new Options();
                    }
                    optionsWindow.Show();
                    optionsWindow.Location =
                        new Point((Screen.PrimaryScreen.WorkingArea.Width/2) - (optionsWindow.Width/2),
                            (Screen.PrimaryScreen.WorkingArea.Height/2) - (optionsWindow.Height/2));
                };
            }
            var button4 = new UISystem.UIButton(0x9e, 2, 0x4c, 0x16, "Options", action6)
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            toolPanel.Controls.Add(button4);
            var button5 = new UISystem.UIButton(base.Width - 0x21, 5, 0x19, 15, "",
                () => Process.GetCurrentProcess().Kill())
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                buttonImage = Image.FromFile("./data/close.png")
            };
            base.Controls.Add(button5);
            if (action7 == null)
            {
                action7 = delegate
                {
                    clientBrowser.Anchor = AnchorStyles.None;
                    base.WindowState = FormWindowState.Minimized;
                    minimized = true;
                };
            }
            var button6 = new UISystem.UIButton(base.Width - 60, 5, 0x19, 15, "", action7)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                buttonImage = Image.FromFile("./data/minimize.png")
            };
            base.Controls.Add(button6);
            if (action8 == null)
            {
                action8 = delegate
                {
                    expanded = !expanded;
                    if (panelButton.buttonText == "<")
                    {
                        base.Width = clientBrowser.Right + 5;
                        panelButton.buttonText = ">";
                        toolPanelWidth = toolPanel.Width;
                        clientBrowser.SetSize(base.Width - 10, clientBrowser.Height);
                        toolPanel.Width = 0;
                    }
                    else
                    {
                        base.Width += toolPanelWidth + 5;
                        toolPanel.Width = toolPanelWidth;
                        panelButton.buttonText = "<";
                    }
                    base.Invalidate();
                };
            }
            var button7 = new UISystem.UIButton(base.Width - 0x60, 5, 0x19, 15, "<", action8)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            base.Controls.Add(panelButton = button7);
            var box5 = new UISystem.UILabelBox(0, 0, base.Width, 0x19, Text, 0x11)
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            formTitleBar = box5;
            formTitleBar.MouseMove += GameClient_MouseMove;
            base.Controls.Add(formTitleBar);
            var box3 = new UISystem.UIDragBox(this)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            base.Controls.Add(box3);
            visibleRegion = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
            var di = new DirectoryInfo("./" + Settings.GetValue<string>("Screenshot.ImageDirectory"));
            if (!di.Exists)
            {
                di.Create();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GameClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void GameClient_KeyPress(object sender, KeyPressEventArgs e)
        {
            MessageBox.Show(e.KeyChar.ToString());
        }

        private void GameClient_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(base.Handle, 0xa1, 2, 0);
            }
        }

        private void GameClient_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(UISystem.UITheme.traceColour, visibleRegion);
        }

        private void GameClient_Resize(object sender, EventArgs e)
        {
            if (minimized)
            {
                clientBrowser.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                minimized = false;
            }
            visibleRegion = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
            if (formTitleBar != null)
            {
                formTitleBar.SetSize(base.Width, 0x19);
            }
            if (clientBrowser != null)
            {
                if (expanded)
                {
                    clientBrowser.SetSize(toolPanel.Left - 10, clientBrowser.Height);
                }
                else
                {
                    clientBrowser.SetSize(base.Width - 10, clientBrowser.Height);
                }
            }
        }

        private void InitializeComponent()
        {
            components = new Container();
            var manager = new ComponentResourceManager(typeof (GameClient));
            toolTip1 = new ToolTip(components);
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            base.ClientSize = new Size(0x4a6, 600);
            base.FormBorderStyle = FormBorderStyle.None;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.Name = "GameClient";
            base.StartPosition = FormStartPosition.CenterScreen;
            Text = "Summit Client by CodeUSA";
            base.FormClosing += GameClient_FormClosing;
            base.Paint += GameClient_Paint;
            base.MouseMove += GameClient_MouseMove;
            base.Resize += GameClient_Resize;
            base.ResumeLayout(false);
        }

        private void playerTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                findButton.OnClick();
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}