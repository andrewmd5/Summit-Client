using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CodeUSAClient.Properties;

namespace CodeUSAClient
{
    public class UISystem
    {
        public static PrivateFontCollection fontCollection = new PrivateFontCollection();

        #region Nested type: UIBrowser

        public class UIBrowser : UIElement
        {
            private WebBrowser browserControl;

            public UIBrowser(int x, int y, int width, int height, string defaultUrl) : base(x, y, width, height)
            {
                var browser = new WebBrowser
                {
                    Location = new Point(1, 1),
                    Size = new Size(width - 2, height - 2),
                    Url = new Uri(defaultUrl),
                    IsWebBrowserContextMenuEnabled = false,
                    Dock = DockStyle.Fill,
                    MinimumSize = new Size(20, 20),
                    Name = "browserControl",
                    ScriptErrorsSuppressed = true,
                    ScrollBarsEnabled = false,
                    TabIndex = 0,
                    Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top
                };
                browserControl = browser;
                browserControl.DocumentCompleted += browserControl_DocumentCompleted;
                base.Controls.Add(browserControl);
                AdjustFormScrollbars(false);
            }

            private void browserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                browserControl.Document.Body.Style = "overflow:hidden";
            }

            private void browserControl_Resize(object sender, EventArgs e)
            {
            }

            public void NavigateTo(string url)
            {
                browserControl.Navigate(url);
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics graphics)
            {
            }
        }

        #endregion

        #region Nested type: UIButton

        public class UIButton : UIElement
        {
            private static Thread backgroundThread;
            private static List<UIButton> buttonList;
            private bool animating;
            private Thread animationThread;
            public Image buttonImage;
            public string buttonText;
            public bool init;
            public int[] linePos;
            private Action onClickDelegate;
            private Action<UIButton> onClickDelegateWithParameter;
            public Color textColour;

            static UIButton()
            {
                uint count = 0;
                buttonList = new List<UIButton>();
                backgroundThread = new Thread(() =>
                {
                    Label_0000:
                    while (buttonList.Count == 0)
                    {
                        Thread.Sleep(500);
                    }
                    try
                    {
                        foreach (var button in buttonList)
                        {
                            var index = 0;
                            while (index < button.linePos.Length)
                            {
                                var random = new Random(index + ((int) count++));
                                button.linePos[index] += random.Next(3, 5);
                                if (button.linePos[index] > button.Width)
                                {
                                    button.linePos[index] = 0;
                                }
                                button.Invalidate();
                                index++;
                                Thread.Sleep(5);
                            }
                        }
                        goto Label_0000;
                    }
                    catch (InvalidOperationException)
                    {
                        goto Label_0000;
                    }
                });
            }

            public UIButton(int x, int y, int width, int height, string text, Action onClickAction)
                : base(x, y, width, height)
            {
                buttonText = text;
                onClickDelegate = onClickAction;
                textColour = UITheme.textColour.Color;
                linePos = new int[new Random(((x + y) + width) + height).Next(1, 4)];
            }

            public UIButton(int x, int y, int width, int height, string text, Action<UIButton> onClickAction)
                : base(x, y, width, height)
            {
                buttonText = text;
                onClickDelegateWithParameter = onClickAction;
                textColour = UITheme.textColour.Color;
                linePos = new int[new Random(((x + y) + width) + height).Next(1, 4)];
            }

            public override void OnClick()
            {
                ParameterizedThreadStart start = null;
                if (!animating)
                {
                    animating = true;
                    if (start == null)
                    {
                        start = delegate(object element)
                        {
                            Action method = null;
                            var button1 = (UIButton) element;
                            var green = 0;
                            while (green <= 0xff)
                            {
                                if (base.IsDisposed)
                                {
                                    return;
                                }
                                textColour = Color.FromArgb(0xff, green, green);
                                try
                                {
                                    if (method == null)
                                    {
                                        method = () => base.Invalidate();
                                    }
                                    base.Invoke(method);
                                }
                                catch (InvalidOperationException)
                                {
                                }
                                green++;
                                Thread.Sleep(5);
                            }
                            textColour = UITheme.textColour.Color;
                            animating = false;
                            animationThread.Abort();
                        };
                    }
                    (animationThread = new Thread(start)).Start(this);
                }
                if (onClickDelegateWithParameter != null)
                {
                    onClickDelegateWithParameter(this);
                }
                else
                {
                    onClickDelegate();
                }
            }

            public override void RenderElement(Graphics g)
            {
                if (!init)
                {
                    buttonList.Add(this);
                    init = true;
                }
                g.FillRectangle(
                    new LinearGradientBrush(base.elementArea, base.Enabled ? UITheme.buttonTop : UITheme.buttonBottom,
                        UITheme.buttonBottom, LinearGradientMode.Vertical), base.elementArea);
                var ef = g.MeasureString(buttonText, UITheme.defaultFont);
                var point = new PointF((elementRegion.Width/2) - (((int) ef.Width)/2),
                    (elementRegion.Height/2) - (((int) ef.Height)/2));
                if (buttonImage != null)
                {
                    g.DrawImage(buttonImage, base.elementArea);
                }
                g.DrawString(buttonText,
                    (base.isMouseOver && base.Enabled) ? UITheme.underlinedFont : UITheme.defaultFont,
                    new SolidBrush(textColour), point);
                for (var i = 0; i < linePos.Length; i++)
                {
                    g.FillRectangle(Brushes.White, new Rectangle(linePos[i], ((i%2) == 0) ? 0 : (base.Height - 3), 1, 3));
                }
            }
        }

        #endregion

        #region Nested type: UICheckBox

        public class UICheckBox : UIElement
        {
            private Rectangle box;
            private Settings.Setting setting;
            private bool sized;
            private string _buttonText;

            public UICheckBox(int x, int y, string buttonText, Settings.Setting targetSetting) : base(x, y, 70, 20)
            {
                _buttonText = buttonText;
                box = new Rectangle(3, 3, 13, 13);
                setting = targetSetting;
            }

            public override void OnClick()
            {
                setting.value = !((bool) setting.value);
            }

            public override void RenderElement(Graphics graphics)
            {
                if (!sized)
                {
                    var ef = graphics.MeasureString(setting.name, UITheme.defaultFont);
                    base.SetSize(((int) ef.Width) + 20, 20);
                    sized = true;
                }
                graphics.FillRectangle(Brushes.Gray, box);
                graphics.DrawRectangle(Pens.White, box);
                if ((bool) setting.value)
                {
                    graphics.FillRectangle(Brushes.Lime, new Rectangle(5, 5, 10, 10));
                }
                graphics.DrawString(_buttonText, UITheme.defaultFont, Brushes.White, new PointF(17f, 2f));
            }
        }

        #endregion

        #region Nested type: UIColourPicker

        public class UIColourPicker : Form
        {
            private UISimplePanel baseElement;

            public UIColourPicker(int hexColour)
            {
                base.Size = new Size(300, 600);
                base.FormBorderStyle = FormBorderStyle.FixedSingle;
                baseElement = new UISimplePanel(0, 0, 300, 600);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var argb = 0;
                var x = 3;
                var y = 3;
                while (argb != 0xffffff)
                {
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(argb)),
                        new Rectangle(new Point(x, y), new Size(1, 1)));
                    y++;
                    if ((y + 1) > (base.Height - 4))
                    {
                        x++;
                        y = 3;
                    }
                }
            }
        }

        #endregion

        #region Nested type: UIDragBox

        public class UIDragBox : UIElement
        {
            private static int height = 10;
            private static int width = 10;
            private Form parentForm;

            public UIDragBox(Form parent) : base(parent.Right - width, parent.Bottom - height, width, height)
            {
                base.MouseMove += UIDragBox_MouseMove;
                base.MouseUp += UIDragBox_MouseUp;
                parentForm = parent;
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics graphics)
            {
                graphics.FillRectangle(Brushes.Red, base.ClientRectangle);
            }

            private void UIDragBox_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    var width = Cursor.Position.X - parentForm.Left;
                    var height = Cursor.Position.Y - parentForm.Top;
                    parentForm.SetBounds(parentForm.Left, parentForm.Top, width, height);
                }
            }

            private void UIDragBox_MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    parentForm.Invalidate();
                }
            }
        }

        #endregion

        #region Nested type: UIElement

        public abstract class UIElement : Panel
        {
            public Rectangle elementArea;
            public Rectangle elementRegion;
            public bool isMouseOver;

            public UIElement(int x, int y, int width, int height)
            {
                base.Location = new Point(x, y);
                base.Size = new Size(width, height);
                elementRegion = new Rectangle(x, y, width - 1, height - 1);
                elementArea = new Rectangle(0, 0, width - 1, height - 1);
                base.SetStyle(
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint,
                    true);
            }

            public abstract void OnClick();

            protected override void OnClick(EventArgs e)
            {
                OnClick();
                base.Invalidate();
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                isMouseOver = true;
                base.Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                isMouseOver = false;
                base.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.FillRectangle(UITheme.backgroundColour, elementArea);
                RenderElement(e.Graphics);
                e.Graphics.DrawRectangle(UITheme.traceColour, elementArea);
            }

            public abstract void RenderElement(Graphics graphics);

            public void Resize(int width, int height)
            {
                base.Size = new Size(base.Size.Width + width, base.Size.Height + height);
                elementRegion = new Rectangle(elementRegion.X, elementRegion.Y, base.Size.Width - 1,
                    base.Size.Height - 1);
                elementArea = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                base.Invalidate();
            }

            public void SetSize(int width, int height)
            {
                base.Size = new Size(width, height);
                elementRegion = new Rectangle(elementRegion.X, elementRegion.Y, base.Size.Width - 1,
                    base.Size.Height - 1);
                elementArea = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                base.Invalidate();
            }
        }

        #endregion

        #region Nested type: UILabelBox

        public class UILabelBox : UIElement
        {
            #region Position enum

            public enum Position
            {
                Bottom = 8,
                Center = 0x10,
                Left = 1,
                Right = 2,
                Top = 4
            }

            #endregion

            public Font labelFont;
            public string labelText;
            private int textPosition;

            public UILabelBox(int x, int y, int width, int height, string text, int textPos) : base(x, y, width, height)
            {
                labelText = text;
                textPosition = textPos;
                BackColor = Color.Transparent;
                labelFont = UITheme.bigFont;
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics g)
            {
                g.FillRectangle(
                    new LinearGradientBrush(base.elementArea, Color.FromArgb(0x5d, 0x5d, 0x5d),
                        Color.FromArgb(30, 30, 30), LinearGradientMode.Vertical), base.elementArea);
                var ef = g.MeasureString(labelText, labelFont);
                var empty = PointF.Empty;
                if ((textPosition & 0x10) == 1)
                {
                    empty.X = (elementRegion.Width/2) - (((int) ef.Width)/2);
                    empty.Y = (elementRegion.Height/2) - (((int) ef.Height)/2);
                }
                if ((textPosition & 2) == 1)
                {
                    empty.X = base.Width - ef.Width;
                }
                if ((textPosition & 8) == 1)
                {
                    empty.Y = base.Height - ef.Height;
                }
                if (textPosition == 6)
                {
                    empty.Y = (elementRegion.Height/2) - (((int) ef.Height)/2);
                    empty.X = 0f;
                }
                g.DrawString(labelText, labelFont, UITheme.textColour, empty);
            }

            public void SetText(string text)
            {
                labelText = text;
                base.Invalidate();
            }
        }

        #endregion

        #region Nested type: UINewsBox

        public class UINewsBox : UIElement
        {
            private int itemOverIndex;
            private Point mousePos;
            private List<NewsItem> newsItems;

            public UINewsBox(int x, int y, int width, int height, string newsAddress) : base(x, y, width, height)
            {
                newsItems = new List<NewsItem>();
                var item3 = new NewsItem
                {
                    title = "Latest News & Patches:"
                };
                newsItems.Add(item3);
                using (var client = new WebClient())
                {
                    var document = new XmlDocument();
                    try
                    {
                        document.LoadXml(client.DownloadString(newsAddress));
                        var element = document["source"];
                        for (var i = 1; i < element.ChildNodes.Count; i++)
                        {
                            var node = element.ChildNodes[i];
                            var item = new NewsItem
                            {
                                title = node.ChildNodes[0].InnerText,
                                author = node.ChildNodes[1].InnerText,
                                date = node.ChildNodes[2].InnerText,
                                threadId = node.Attributes[0].Value
                            };
                            newsItems.Add(item);
                        }
                    }
                    catch (WebException)
                    {
                        var item2 = new NewsItem
                        {
                            title = "Unable to connect to server!"
                        };
                        newsItems.Add(item2);
                    }
                }
                mousePos = new Point();
            }

            public override void OnClick()
            {
                if ((itemOverIndex < newsItems.Count) && (itemOverIndex != 0))
                {
                    Process.Start("http://codeusa.net/forums/showthread.php?t=" + newsItems[itemOverIndex].threadId);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                itemOverIndex = (e.Y - 3)/15;
                mousePos = e.Location;
                base.Invalidate();
            }

            public override void RenderElement(Graphics g)
            {
                g.DrawLine(UITheme.traceColour, 0, 0x12, base.Width, 0x12);
                var num = 3;
                for (var i = 0; i < newsItems.Count; i++)
                {
                    var item = newsItems[i];
                    g.MeasureString(item.title, UITheme.boldFont);
                    var empty = PointF.Empty;
                    empty.X = 3f;
                    empty.Y = num;
                    var title = item.title;
                    if (title.Length > 40)
                    {
                        title = title.Substring(0, 40) + "...";
                    }
                    g.DrawString(title, UITheme.boldFont, UITheme.textColour, empty);
                    num += 15;
                }
                if ((base.isMouseOver && (itemOverIndex < newsItems.Count)) && (itemOverIndex != 0))
                {
                    var text = newsItems[itemOverIndex].title + "\nPosted by " + newsItems[itemOverIndex].author +
                               " on " + newsItems[itemOverIndex].date;
                    var ef = g.MeasureString(text, UITheme.boldFont);
                    mousePos.Offset(0, (int) -ef.Height);
                    g.FillRectangle(UITheme.backgroundHoverColour,
                        new Rectangle(mousePos, new Size((int) ef.Width, (int) ef.Height)));
                    g.DrawString(text, UITheme.defaultFont, UITheme.textColour, new PointF(mousePos.X, mousePos.Y));
                }
            }

            #region Nested type: NewsItem

            [StructLayout(LayoutKind.Sequential)]
            private struct NewsItem
            {
                public string title;
                public string author;
                public string date;
                public string threadId;
            }

            #endregion
        }

        #endregion

        #region Nested type: UIPictureBox

        public class UIPictureBox : PictureBox
        {
            public UIPictureBox(int x, int y, int width, int height)
            {
                base.Location = new Point(x, y);
                base.Size = new Size(width, height);
                base.SizeMode = PictureBoxSizeMode.CenterImage;
                BackColor = Color.Transparent;
            }

            public void SetImage(string path)
            {
                base.Image = Image.FromFile(path);
            }
        }

        #endregion

        #region Nested type: UIProgressBar

        public class UIProgressBar : UIElement
        {
            private bool displayText;
            private int maxValue;
            private string textValue;
            private int value;

            public UIProgressBar(int x, int y, int width, int height, int max) : base(x, y, width, height)
            {
                displayText = true;
                textValue = "N/A";
                maxValue = max;
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics g)
            {
                var num = value/((float) maxValue);
                var text = displayText ? textValue : (((int) (num*100f)) + "%");
                var ef = g.MeasureString(text, UITheme.defaultFont);
                var rect = new Rectangle(1, 1, (int) (num*(elementArea.Width - 2)), elementArea.Height - 1);
                if (rect.Width > 0)
                {
                    g.FillRectangle(
                        new LinearGradientBrush(rect, UITheme.progressBarTop, UITheme.progressBarBottom,
                            LinearGradientMode.Vertical), rect);
                }
                g.DrawString(text, UITheme.defaultFont, UITheme.textColour,
                    new PointF((elementArea.Width/2) - (ef.Width/2f), (elementArea.Height/2) - (ef.Height/2f)));
            }

            public void SetText(string text, int value)
            {
                Action method = delegate
                {
                    textValue = text;
                    this.value = value;
                    displayText = true;
                    Invalidate();
                };
                if (base.InvokeRequired)
                {
                    base.Invoke(method);
                }
                else
                {
                    method();
                }
            }

            public void SetValue(int value)
            {
                Action method = delegate
                {
                    this.value = value;
                    displayText = false;
                    Invalidate();
                };
                if (base.InvokeRequired)
                {
                    base.Invoke(method);
                }
                else
                {
                    method();
                }
            }
        }

        #endregion

        #region Nested type: UISimplePanel

        public class UISimplePanel : UIElement
        {
            public UISimplePanel(int x, int y, int width, int height) : base(x, y, width, height)
            {
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics graphics)
            {
            }
        }

        #endregion

        #region Nested type: UITextBox

        public class UITextBox : TextBox
        {
            public UITextBox(int x, int y, int width, int height)
            {
                BackColor = UITheme.buttonBottom;
                ForeColor = Color.White;
                base.Location = new Point(x, y);
                base.Size = new Size(width, height);
                base.BorderStyle = BorderStyle.FixedSingle;
            }

            public UITextBox(int x, int y, int width, int height, List<string> autoCompleteValues)
            {
                BackColor = UITheme.buttonBottom;
                ForeColor = Color.White;
                base.Location = new Point(x, y);
                base.Size = new Size(width, height);
                base.BorderStyle = BorderStyle.FixedSingle;
                base.AutoCompleteMode = AutoCompleteMode.Suggest;
                base.AutoCompleteSource = AutoCompleteSource.CustomSource;
                var strings = new AutoCompleteStringCollection();
                strings.AddRange(autoCompleteValues.ToArray());
                base.AutoCompleteCustomSource = strings;
            }
        }

        #endregion

        #region Nested type: UITheme

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct UITheme
        {
            public static SolidBrush backgroundColour;
            public static SolidBrush backgroundHoverColour;
            public static SolidBrush textColour;
            public static SolidBrush buttonOutline;
            public static Pen traceColour;
            public static Color progressBarTop;
            public static Color progressBarBottom;
            public static Color buttonTop;
            public static Color buttonBottom;
            public static Font defaultFont;
            public static Font boldFont;
            public static Font underlinedFont;
            public static Font bigFont;
            public static Font normalFont;

            static unsafe UITheme()
            {
                backgroundColour = new SolidBrush(Color.FromArgb(30, 30, 30));
                backgroundHoverColour = new SolidBrush(Color.FromArgb(50, 50, 50));
                textColour = new SolidBrush(Color.FromArgb(0xff, 0xff, 0xff));
                buttonOutline = new SolidBrush(Color.FromArgb(30, 30, 0x5d));
                traceColour = new Pen(Color.FromArgb(0x7d, 0x7d, 0x7d));
                progressBarTop = Color.FromArgb(50, 50, 50);
                progressBarBottom = Color.FromArgb(30, 30, 30);
                buttonTop = Color.FromArgb(0x73, 0x73, 0x73);
                buttonBottom = Color.FromArgb(30, 30, 30);
                defaultFont = new Font("Tahoma", 9f);
                boldFont = new Font(defaultFont, FontStyle.Bold);
                underlinedFont = new Font(defaultFont, FontStyle.Underline);
                bigFont = null;
                normalFont = null;
                var font = Resources.font;
                fixed (byte* numRef = font)
                {
                    fontCollection.AddMemoryFont((IntPtr) numRef, font.Length);
                }
                bigFont = new Font(fontCollection.Families[0], 15f);
                normalFont = new Font(fontCollection.Families[0], 13f);
            }
        }

        #endregion

        #region Nested type: UIValueEditor

        public class UIValueEditor : UIElement
        {
            private Settings.Setting setting;
            private bool sized;
            private TextBox textBox;

            public UIValueEditor(int x, int y, int width, Settings.Setting targetSetting) : base(x, y, width, 40)
            {
                setting = targetSetting;
                var box = new TextBox
                {
                    Location = new Point(0, 20),
                    BackColor = UITheme.buttonBottom,
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                textBox = box;
                textBox.TextChanged += textBox_TextChanged;
                textBox.Text = targetSetting.value.ToString();
                base.Controls.Add(textBox);
            }

            public override void OnClick()
            {
            }

            public override void RenderElement(Graphics graphics)
            {
                if (!sized)
                {
                    var ef = graphics.MeasureString(setting.name, UITheme.defaultFont);
                    base.SetSize(((int) ef.Width) + 5, 40);
                    textBox.Width = base.Width;
                    sized = true;
                }
                graphics.DrawString(setting.name, UITheme.defaultFont, Brushes.White, new PointF(1f, 1f));
            }

            private void textBox_TextChanged(object sender, EventArgs e)
            {
                if (textBox.TextLength > 0)
                {
                    setting.value = Convert.ChangeType(textBox.Text, setting.type);
                }
            }
        }

        #endregion
    }
}