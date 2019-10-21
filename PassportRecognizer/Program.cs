using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace cv2
{
    internal class Program
    {
        // TODO: попробовать еще с этими обработками
        // cv::dilate(MCRregion, MCRregion, 24);
        // cv::erode(MCRregion, MCRregion, 24);
        // cv::bitwise_not(MCRregion, MCRregion);

        private static void Main(string[] args)
        {
            // Каскад для определения лица
            CascadeClassifier cascadeClassifier = new CascadeClassifier(@"./casscade/haarcascade_frontalface_alt2.xml");

            // Пороги обработки исходного изображения
            int thresh1 = 100;
            int thresh2 = 110;
            int thresh3 = 120;

            // Порог выбора контура
            int minArea = 100;
            int maxArea = 4000;

            var src = Cv2.ImRead(@"C:\Temp\pas1.jpg");
            Cv2.Resize(src, src, new Size(1800, 2500));

            Mat img = new Mat();
            Cv2.CvtColor(src, img, ColorConversionCodes.BGR2GRAY);

            Mat dst1 = new Mat();
            Mat dst2 = new Mat();
            Mat dst3 = new Mat();
            Cv2.Threshold(img, dst1, thresh1, 255, ThresholdTypes.Binary);
            Cv2.Threshold(img, dst2, thresh2, 255, ThresholdTypes.Binary);
            Cv2.Threshold(img, dst3, thresh3, 255, ThresholdTypes.Binary);

            // Ищем все возможные контуры
            Cv2.FindContours(dst1, out Point[][] contours1, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            Cv2.FindContours(dst2, out Point[][] contours2, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            Cv2.FindContours(dst3, out Point[][] contours3, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            // Исключаем контуры не адекватные
            List<Rect> possibleContours1 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours1, minArea, maxArea));
            List<Rect> possibleContours2 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours2, minArea, maxArea));
            List<Rect> possibleContours3 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours3, minArea, maxArea));

            // Собираем все контуры вместе
            List<Rect> allContours = new List<Rect>();
            allContours.AddRange(possibleContours1);
            allContours.AddRange(possibleContours2);
            allContours.AddRange(possibleContours3);

            // Ищем лица
            var faces = cascadeClassifier.DetectMultiScale(src);
            foreach (var face in faces)
            {
                src.Rectangle(face, new Scalar(0, 255, 255), 3);
            }

            // Ищем в контурах слова
            List<Rect> possibleWords = CvEngine.GetWordsFromContours(allContours, src);
            src.PutText($"{possibleWords.Count}", new Point(40, 40), HersheyFonts.Italic, 1, new Scalar(0, 0, 0));

            // Рисуем контуры
            CvEngine.DrawContours(possibleContours1, src, new Scalar(0, 0, 255));
            CvEngine.DrawContours(possibleContours2, src, new Scalar(0, 255, 0));
            CvEngine.DrawContours(possibleContours3, src, new Scalar(255, 0, 0));
            CvEngine.DrawContours(possibleWords, src, new Scalar(255, 0, 255), 2, true);

            // Распознаём
            TessEngine.Recognize(dst2, possibleWords);

            // Меняем размер для отображение на экране
            Cv2.Resize(src, src, new Size(800, 1000));
            Cv2.Resize(dst1, dst1, new Size(300, 500));
            Cv2.Resize(dst2, dst2, new Size(300, 500));
            Cv2.Resize(dst3, dst3, new Size(300, 500));

            // Показываем
            Cv2.ImShow("src", src);
            Cv2.ImShow("src1", dst1);
            Cv2.ImShow("src2", dst2);
            Cv2.ImShow("src3", dst3);

            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
            //Console.ReadKey();
        }
    }
}