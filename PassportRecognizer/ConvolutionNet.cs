using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DatasetGenerator;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PassportRecognizerML.Model;
using Point = OpenCvSharp.Point;

namespace PassportRecognizer
{
    /// <summary>
    /// Свёрточная сетка
    /// </summary>
    internal class ConvolutionNet
    {
        private HandwritingRecognition handWriting = new HandwritingRecognition();
        private int cnt = 0;

        public void RecognizeLetter(string filename)
        {
            string file = @"C:\Temp\tiffs\mat27.png";

            if (!File.Exists(file)) return;

            Bitmap bit = new Bitmap(file);
            Bitmap resized = new Bitmap(bit, 32, 32);

            var hand = new HandwritingRecognition();
            var data = hand.GetDatasetValues(resized, "ffffffff");

            var input = new ModelInput();
            input.PixelValues = data.ToArray();

            ModelOutput result = ConsumeModel.Predict(input);

            Console.WriteLine($"Prediction: {Convert.ToChar((int)result.Prediction + '0')} \tScore: {result.Score.Max()}");
        }

        // Вообще не вариант. Очень много времени берет
        public void GetPossibleLetters(Mat mat)
        {
            int width = mat.Width;
            int height = mat.Height;

            List<Point> points = new List<Point>();

            for (int h = 0; h < height - 32; h += 4)
            {
                for (int w = 0; w < width - 32; w += 4)
                {
                    Mat image = new Mat(mat, new Rect(w, h, 32, 32));

                    var model = RecognizeImage(image.ToBitmap());
                    if (model.Score.Max() > 20)
                    {
                        points.Add(new Point(w, h));
                    }
                }
                Console.WriteLine($"ROW {h}\t points: {points.Count}");
            }

            Console.WriteLine($"Count: {points.Count}");
        }

        public ModelOutput RecognizeImage(Bitmap image)
        {
            image.Save($@"c:/Temp/{cnt++}.png");

            var data = handWriting.GetDatasetValues(image, "ffffffff");

            var input = new ModelInput
            {
                PixelValues = data.ToArray()
            };

            return ConsumeModel.Predict(input);
        }
    }
}