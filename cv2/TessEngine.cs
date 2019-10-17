using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenCvSharp;
using Tesseract;

namespace cv2
{
    internal class TessEngine
    {
        // Распознать
        public static void Recognize(Mat image, List<OpenCvSharp.Rect> possibleWords)
        {
            using (var ocr = new TesseractEngine(@"./tessdata/", "rus", EngineMode.Default))
            {
                int cnt = 0;
                foreach (var word in possibleWords)
                {
                    Mat mat = new Mat(image, word);

                    // TODO: выделить серию и номер паспорта как-то получше
                    // Серия и номер паспорта. Поворачиваем на 90 градусов
                    if (word.X > image.Width * 0.75f)
                    {
                        var max = new[] { mat.Width, mat.Height }.Max();

                        // Поворот на 90 градусов
                        //
                        Mat m = new Mat(max, max, MatType.CV_8U);
                        Cv2.Transpose(mat, mat);
                        Cv2.Flip(mat, mat, FlipMode.X);
                    }

                    Bitmap bit = new Bitmap(mat.ToMemoryStream());
                    Pix pix = PixConverter.Bitmap2Pix(bit);
                    pix.Save(@"C:\Temp\tiffs\mat" + cnt + ".png");

                    // Распознавалка
                    var page = ocr.Process(pix);
                    var text = page.GetText().Trim();

                    Console.WriteLine($"Rect {cnt}: {text}");

                    page.Dispose();

                    cnt++;
                }
            }
        }
    }
}