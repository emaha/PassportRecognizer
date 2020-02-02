using cv2;
using OpenCvSharp;
using System.Collections.Generic;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace PassportRecognizer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var conv = new ConvolutionNet();
            //conv.RecognizeLetter(@"tiffs\A.png");

            // Каскад для определения лица
            CascadeClassifier cascadeClassifier = new CascadeClassifier(@"./casscade/haarcascade_frontalface_alt2.xml");

            // Пороги обработки исходного изображения
            int thresh0 = 140;
            int thresh1 = 160;
            int thresh2 = 180;

            // Порог выбора контура
            int minArea = 80;
            // Верхний попрог приходится делать большим, потому-что буквы в некоторых паспортах
            // написаны правтически слитно (одна буква перетекаетв другую без разрыва)
            // из-за этого получаются большие контуры, которые нужно учитывать,
            // но это даёт побочный эффект в виде объединения контуров далеко расположенных друг от друга
            int maxArea = 1000;

            var src = Cv2.ImRead(@"../../../../pas11.jpg");
            Cv2.Resize(src, src, new Size(1800, 2500));

            Mat img = new Mat();
            Cv2.CvtColor(src, img, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(img, img, new Size(3, 3), 1.5f);

            Mat dst0 = new Mat();
            Mat dst1 = new Mat();
            Mat dst2 = new Mat();
            Cv2.Threshold(img, dst0, thresh0, 255, ThresholdTypes.Binary);
            Cv2.Threshold(img, dst1, thresh1, 255, ThresholdTypes.Binary);
            Cv2.Threshold(img, dst2, thresh2, 255, ThresholdTypes.Binary);

            // Ищем все возможные контуры
            Cv2.FindContours(dst0, out Point[][] contours0, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            Cv2.FindContours(dst1, out Point[][] contours1, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            Cv2.FindContours(dst2, out Point[][] contours2, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            // Исключаем контуры не адекватные
            List<Rect> possibleContours0 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours0, minArea, maxArea));
            List<Rect> possibleContours1 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours1, minArea, maxArea));
            List<Rect> possibleContours2 = CvEngine.TruncateLongContours(CvEngine.GetPossibleLetters(contours2, minArea, maxArea));

            // Собираем все контуры вместе
            List<Rect> allContours = new List<Rect>();
            allContours.AddRange(possibleContours0);
            allContours.AddRange(possibleContours1);
            allContours.AddRange(possibleContours2);

            // Ищем лица
            var faces = cascadeClassifier.DetectMultiScale(src);
            Rect singleFace = new Rect();
            double maxFaceArea = 0;
            foreach (var face in faces)
            {
                var area = CvEngine.Area(face);
                if (maxFaceArea < area)
                {
                    maxFaceArea = area;
                    singleFace = face;
                }
                src.Rectangle(face, new Scalar(0, 128, 255), 3);
            }

            src.Rectangle(singleFace, new Scalar(0, 255, 255), 3);

            // Ищем в контурах слова
            List<Rect> possibleWords = CvEngine.GetWordsFromContours(allContours, src);
            src.PutText($"{possibleWords.Count}", new Point(80, 80), HersheyFonts.Italic, 2, new Scalar(0, 0, 0));

            // TODO: исключить контуры с мусорными данными
            possibleWords = CvEngine.RemoveTrashContours(possibleWords, singleFace);

            // Пробуем получить контуры читаемой части паспорта
            var readableAreaContours = CvEngine.GetReadableAreaContours(singleFace);


            // Рисуем контуры
            CvEngine.DrawContours(possibleContours0, src, new Scalar(0, 0, 255));
            CvEngine.DrawContours(possibleContours1, src, new Scalar(255, 0, 0));
            CvEngine.DrawContours(possibleContours2, src, new Scalar(0, 255, 0));
            CvEngine.DrawContours(possibleWords, src, new Scalar(255, 0, 255), 2, true);

            // Рисуем контуры читаемой части
            CvEngine.DrawContours(readableAreaContours, src, new Scalar(255, 150, 0), 4);

            // Распознаём
            TessEngine.Recognize(dst2, possibleWords);

            // Меняем размер для отображение на экране
            Cv2.Resize(src, src, new Size(600, 800));
            Cv2.Resize(dst0, dst0, new Size(300, 500));
            Cv2.Resize(dst1, dst1, new Size(300, 500));
            Cv2.Resize(dst2, dst2, new Size(300, 500));

            // Показываем
            Cv2.ImShow("src", src);
            Cv2.ImShow("src0", dst0);
            Cv2.ImShow("src1", dst1);
            Cv2.ImShow("src2", dst2);

            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
            //Console.ReadKey();
        }
    }
}