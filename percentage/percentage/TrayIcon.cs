using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const string iconFont = "Segoe UI";
        private const int iconFontSize = 14;

        private string batteryPercentage;
        private bool batteryCharging;
        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            // initialize menuItem
            menuItem.Index = 0;
            menuItem.Text = "E&xit";
            menuItem.Click += new System.EventHandler(menuItem_Click);

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = "?";

            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;

            batteryCharging = powerStatus.PowerLineStatus == PowerLineStatus.Online;
            batteryPercentage = (powerStatus.BatteryLifePercent * 100).ToString();

            Color backColor = GetBackColor(batteryPercentage);
            Color textColor = GetFontColor(backColor);

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize), textColor, backColor)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        notifyIcon.Text = (batteryCharging ? "Charging - " : "") + batteryPercentage + "%";
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            var textSize = GetImageSize(text, font);
            Image image = new Bitmap((int) textSize.Width, (int) textSize.Height);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // paint the background
                graphics.Clear(backColor);

                // create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }

        private static SizeF GetImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }

        private Color GetFontColor(Color backColor)
        {
            var l = 0.2126 * backColor.R + 0.7152 * backColor.G + 0.0722 * backColor.B;

            return l < 0.5 ? Color.White : Color.Black;
        }


        /// <summary>
        /// Applica la scala dal verde al rosso in base alla percentuale [2022-12-18]
        /// </summary>
        /// <param name="batteryPercentage"></param>
        /// <returns></returns>
        private Color GetBackColor(string batteryPercentage)
        {
            // https://stackoverflow.com/a/6394340
            int percentage = Convert.ToInt32(batteryPercentage);

            var red = (percentage > 50 ? 1 - 2 * (percentage - 50) / 100.0 : 1.0) * 255;
            var green = (percentage > 50 ? 1.0 : 2 * percentage / 100.0) * 255;
            var blue = 0.0;
            Color result = Color.FromArgb((int)red, (int)green, (int)blue);

            return result;

            //Color color = Color.Black;
            //var step = 255 / 50;

            //if (batteryValue >= 100) // verde
            //{
            //    color = Color.FromArgb(0, 255, 0);
            //}
            //else if (batteryValue < 100 && batteryValue > 50) // scala dal verde al giallo per i valori tra 100 e 50
            //{
            //    var value = Math.Abs((100 / 2) - batteryValue);
            //    byte R = Convert.ToByte(255 - (value * step));
            //    color = Color.FromArgb(R, 255, 0);
            //}
            //else if (batteryValue == 50) // giallo
            //{
            //    color = Color.FromArgb(255, 255, 0);
            //}
            //else if (batteryValue < 50 && batteryValue > 0) // scala dal giallo al rosso per i valori tra 50 e 0
            //{
            //    var value = Math.Abs((100 / 2) - batteryValue);
            //    byte G = Convert.ToByte(255 - (value * step));
            //    color = Color.FromArgb(255, G, 0);
            //}
            //else if (batteryValue == 0) // rosso
            //{
            //    color = Color.FromArgb(255, 0, 0);
            //}
            //return color;
        }
    }
}
