using System.Text;
using SkiaSharp;

namespace WebApplication1.VerificationCode;

public static class VerificationCodeHelper
{
    public static string GetRandomCode(int length)
    {
        StringBuilder code = new();
        string text = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        Random random = new();
        for (int i = 0; i < length; i++)
        {
            code.Append(text[random.Next(0, text.Length)]);
        }
        return code.ToString();
    }

    public static byte[] GetVerificationCode(string text)
    {
        int width = 128;
        int height = 45;
        Random random = new();

        using SKBitmap image = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas canvas = new(image);
        canvas.DrawColor(SKColors.White);

        // 画干扰线
        // 干扰线占图片面积的比例
        decimal distractorLineRate = 0.0005M;
        for (int i = 0; i < (width * height * distractorLineRate); i++)
        {
            using SKPaint _drawStyle = new();
            _drawStyle.Color = new(Convert.ToUInt32(random.Next(int.MaxValue)));
            canvas.DrawLine(random.Next(0, width), random.Next(0, height), random.Next(0, width), random.Next(0, height), _drawStyle);
        }

        // 画文本
        using SKPaint drawStyle = new();
        drawStyle.Color = SKColors.Red;
        drawStyle.TextSize = height;
        drawStyle.IsAntialias = true; // 抗锯齿

        // 文本居中显示
        float textWidth = drawStyle.MeasureText(text);
        float x = (width - textWidth) / 2;
        float y = (height + drawStyle.TextSize) / 2; // 调整Y轴位置以居中

        // 绘制每个字符
        foreach (char c in text)
        {
            string character = c.ToString();
            float charWidth = drawStyle.MeasureText(character);
            float charRotation = random.Next(-15, 15); // 随机旋转角度

            canvas.Save();
            canvas.RotateDegrees(charRotation, x + charWidth / 2, y - drawStyle.TextSize / 2);
            canvas.DrawText(character, x, y, drawStyle);
            canvas.Restore();

            x += charWidth;
        }

        // 绘制噪点
        for (int i = 0; i < (width * height * 0.6); i++)
        {
            image.SetPixel(random.Next(0, width), random.Next(0, height), new SKColor(Convert.ToUInt32(random.Next(int.MaxValue))));
        }

        using var img = SKImage.FromBitmap(image);
        using SKData p = img.Encode(SKEncodedImageFormat.Png, 100);
        return p.ToArray();
    }
}
