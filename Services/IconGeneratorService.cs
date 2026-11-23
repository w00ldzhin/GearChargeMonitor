using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace GearChargeMonitor.Services
{
    public class IconGeneratorService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public Icon CreateTextIcon(string text, Color textColor)
        {
            const int size = 32;

            using var bitmap = new Bitmap(size, size);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float fontSize = text.Length >= 3 ? 16f : 21f;
            using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(textColor);

            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var rect = new RectangleF(0, 0, size, size);
            graphics.DrawString(text, font, brush, rect, format);

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);

            return icon;
        }
    }
}