using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace CodeUSAClient
{
    public class Options : Form
    {
        private Dictionary<string, FieldInfo> UIThemeSettings;
        private IContainer components;
        private UISystem.UILabelBox formTitleBar;
        private List<Settings.Setting> settings;
        private Rectangle visibleRegion;

        public Options()
        {
            Action onClickAction = null;
            InitializeComponent();
            settings = Settings.GetSettings();
            var settingsPanel = new UISystem.UISimplePanel(90, 30, base.Width - 100, base.Height - 40);
            base.Controls.Add(settingsPanel);
            var themePanel = new UISystem.UISimplePanel(90, 30, base.Width - 100, base.Height - 40)
            {
                Visible = false
            };
            base.Controls.Add(themePanel);
            if (onClickAction == null)
            {
                onClickAction = () => base.Close();
            }
            var button = new UISystem.UIButton(base.Width - 0x21, 5, 0x19, 15, "X", onClickAction)
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            base.Controls.Add(button);
            base.Controls.Add(new UISystem.UIButton(3, 0x1b, 70, 0x16, "Save", () => Settings.Save()));
            base.Controls.Add(new UISystem.UIButton(3, 0x34, 70, 0x16, "Load", () =>
            {
                Settings.Load();
                foreach (Control control in settingsPanel.Controls)
                {
                    control.Invalidate();
                }
            }));
            base.Controls.Add(new UISystem.UIButton(3, 0x4d, 70, 0x16, "Theme", element =>
            {
                if (element.buttonText == "Theme")
                {
                    element.buttonText = "Settings";
                    settingsPanel.Visible = false;
                    themePanel.Visible = true;
                }
                else
                {
                    element.buttonText = "Theme";
                    settingsPanel.Visible = true;
                    themePanel.Visible = false;
                }
            }));
            var box = new UISystem.UILabelBox(0, 0, base.Width, 0x19, Text, 0x11)
            {
                Anchor = AnchorStyles.Left
            };
            formTitleBar = box;
            formTitleBar.MouseMove += formTitleBar_MouseMove;
            base.Controls.Add(formTitleBar);
            visibleRegion = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
            var x = 3;
            var y = 3;
            foreach (var str in Settings.GetHeads())
            {
                foreach (var setting in Settings.GetSettingsForHead(str))
                {
                    UISystem.UIElement element = null;
                    if (setting.type == typeof (bool))
                    {
                        settingsPanel.Controls.Add(element = new UISystem.UICheckBox(x, y, "Bool", setting));
                    }
                    else if ((setting.type == typeof (int)) || (setting.type == typeof (string)))
                    {
                        settingsPanel.Controls.Add(element = new UISystem.UIValueEditor(x, y, 70, setting));
                    }
                    else if (setting.type == typeof (Color))
                    {
                        themePanel.Controls.Add(
                            element = new UISystem.UIButton(x, y, 100, 0x19, setting.name, () => { }));
                    }
                    else if (setting.type == typeof (SolidBrush))
                    {
                        themePanel.Controls.Add(
                            element = new UISystem.UIButton(x, y, 100, 0x19, setting.name, () => { }));
                    }
                    if (element != null)
                    {
                        if ((y + element.Height) > settingsPanel.Height)
                        {
                            y = 3;
                            x += 200;
                            element.Location = new Point(x, y);
                            y += element.Height + 5;
                        }
                        else
                        {
                            y += element.Height + 5;
                        }
                    }
                }
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

        private void formTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GameClient.ReleaseCapture();
                GameClient.SendMessage(base.Handle, 0xa1, 2, 0);
            }
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            base.ClientSize = new Size(0x28e, 0x184);
            base.FormBorderStyle = FormBorderStyle.None;
            base.Name = "Options";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            Text = "Client Options";
            base.TopMost = true;
            base.Paint += Options_Paint;
            base.ResumeLayout(false);
        }

        private void Options_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(UISystem.UITheme.traceColour, visibleRegion);
        }
    }
}