using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cv2
{
    internal class CvEngine
    {
        // Максимальное отношение длинны к ширине контура (применяется для отсечения аномальных контуров)
        private const double MaxContourRatio = 3f;

        // Максимальное расстояние по ширине для поиска ближайшего соседа
        private const double MaxDistanceByWidth = 1.9f;

        // Максимальное расстояние по высоте для поиска ближайшего соседа
        private const double MaxDistanceByHeight = 1.5f;

        // Отступ контура слова. 
        // Берем картинку немного с запасом, чтобы распозновалка получила не обрезанное слово
        private const int ContourMargin = 10;

        /// <summary>
        /// Получает возможные контуры слов
        /// </summary>
        /// <param name="contours"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static List<Rect> GetWordsFromContours(List<Rect> contours, Mat image)
        {
            if (contours == null || contours.Count == 0) return null;
            // Средняя площадь контура
            double avg = GetAverageContoursArea(contours);
            Console.WriteLine($"Avg. area: {avg}");

            // Создаем сортированный список по горизонтали (важно!!! без этого слово не соберется)
            var sortedList = contours.ToArray();
            for (int i = 0; i < sortedList.Length; i++)
            {
                for (int j = i; j < sortedList.Length; j++)
                {
                    if (sortedList[i].Left > sortedList[j].Left)
                    {
                        Swap(ref sortedList, i, j);
                    }
                }
            }

            // Определяем для каждого контура ближайший в пределах максимального расстояния
            int index = 0;
            var nodes = sortedList.Select(x => new Node { Id = index++, Rect = x }).ToList();
            foreach (var node in nodes)
            {
                node.Links = FindLink(node, nodes);

                // Рисуем линки (опционально для наглядности)
                foreach (var link in node.Links)
                {
                    image.Line(new Point(node.Rect.X, node.Rect.Y),
                        new Point(link.Rect.X, link.Rect.Y),
                        new Scalar(255, 255, 50), 2);
                }
            }

            // Объединяем линки в группы
            List<List<Node>> groups = new List<List<Node>>();
            List<Node> usedNodes = new List<Node>();
            foreach (var node in nodes.Where(x => x.Links.Count > 0))
            {
                var chain = FindChain(node, usedNodes);
                groups.Add(chain);
            }
            groups = groups.Where(x => x.Count > 0).ToList();

            // Пытаемся сформировать контуры цепей
            List<Rect> linkGroups = new List<Rect>();

            //определяем контуры групп
            foreach (var group in groups)
            {
                var top = group[0];
                var left = group[0];
                var right = group[0];
                var bottom = group[0];
                var width = group[0];
                var height = group[0];
                foreach (var node in group)
                {
                    if (top.Rect.Y > node.Rect.Y) top = node;
                    if (left.Rect.X > node.Rect.X) left = node;
                    if (right.Rect.X < node.Rect.X) right = node;
                    if (bottom.Rect.Y < node.Rect.Y) bottom = node;

                    if (width.Rect.Width < node.Rect.Width) width = node;
                    if (height.Rect.Height < node.Rect.Height) height = node;
                }

                linkGroups.Add(new Rect(left.Rect.X - ContourMargin, top.Rect.Y - ContourMargin,
                    right.Rect.X - left.Rect.X + width.Rect.Width + ContourMargin * 2,
                    bottom.Rect.Y - top.Rect.Y + height.Rect.Height + ContourMargin * 2)
                );
            }

            // Сортировка по вертикали
            var sortedLinkGroups = linkGroups.ToArray();
            for (int i = 0; i < sortedLinkGroups.Length; i++)
            {
                for (int j = i; j < sortedLinkGroups.Length; j++)
                {
                    if (sortedLinkGroups[i].Top > sortedLinkGroups[j].Top)
                    {
                        Swap(ref sortedLinkGroups, i, j);
                    }
                }
            }

            return sortedLinkGroups.ToList();
        }

        internal static List<Rect> GetReadableAreaContours(Rect singleFace)
        {
            int x = singleFace.X;
            int y = singleFace.Y;
            int w = singleFace.Width;
            int h = singleFace.Height;


            List<Rect> rects = new List<Rect>();
            // не прокатит!!!

            return rects;
        }

        /// <summary>
        /// Убираем лишние контуры (печати, узоры, подписи)
        /// </summary>
        /// <param name="possibleWords">Все возможные контуры слов</param>
        /// <param name="singleFace">Контур лица как отсчетная точка</param>
        /// <returns></returns>
        public static List<Rect> RemoveTrashContours(List<Rect> possibleWords, Rect singleFace)
        {
            // TODO: проверяем нашли ли лицо.
            // если да, то пытаемся от возможного лица убрать контуры,
            // которые находятся непосредственно рядом (печати, узоры, подписи)

            // предположим что 


            return possibleWords;
        }

        /// <summary>
        /// Получает возможные контуры букв
        /// </summary>
        /// <param name="contours"></param>
        /// <param name="minArea"></param>
        /// <param name="maxArea"></param>
        /// <returns></returns>
        public static List<Rect> GetPossibleLetters(Point[][] contours, int minArea, int maxArea)
        {
            var result = new List<Rect>(contours.Length);
            foreach (var con in contours)
            {
                var rect = GetRect(con);
                if (Area(rect) > minArea && Area(rect) < maxArea)
                {
                    result.Add(rect);
                }
            }
            return result;
        }

        /// <summary>
        /// Вычисляет среднюю площадь всех контуров
        /// </summary>
        /// <param name="contours"></param>
        /// <returns></returns>
        public static double GetAverageContoursArea(List<Rect> contours)
        {
            if (contours == null || contours.Count == 0) return 0;

            return contours.Average(Area);
        }

        /// <summary>
        /// Вычисляет площадь контура
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static double Area(Rect rect)
        {
            return (rect.Right - rect.Left) * (rect.Bottom - rect.Top);
        }

        /// <summary>
        /// Получает из массива точек прямоугольник
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static Rect GetRect(Point[] p)
        {
            int xMin = p[0].X;
            int xMax = p[0].X;
            int yMin = p[0].Y;
            int yMax = p[0].Y;
            for (int i = 0; i < p.Length; i++)
            {
                if (xMin > p[i].X) xMin = p[i].X;
                if (xMax < p[i].X) xMax = p[i].X;
                if (yMin > p[i].Y) yMin = p[i].Y;
                if (yMax < p[i].Y) yMax = p[i].Y;
            }
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        /// <summary>
        /// Рисует контуры
        /// </summary>
        /// <param name="contours"></param>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="drawText"></param>
        public static void DrawContours(List<Rect> contours, Mat image, Scalar color, int thickness = 1, bool drawText = false)
        {
            if (contours == null) return;
            int cnt = 0;
            foreach (var item in contours)
            {
                image.Rectangle(item, color, thickness);
                if (drawText)
                {
                    image.PutText($"{cnt}", new Point(item.X, item.Y), HersheyFonts.Italic, 1.2f, new Scalar(255, 0, 0));
                }

                cnt++;
            }
        }

        /// <summary>
        /// Находит цепочку соседей связанных друг с другом
        /// </summary>
        /// <param name="node"></param>
        /// <param name="usedNodes"></param>
        /// <returns></returns>
        private static List<Node> FindChain(Node node, List<Node> usedNodes)
        {
            var chains = new List<Node>();

            if (!usedNodes.Contains(node))
            {
                usedNodes.Add(node);
                chains.Add(node);

                foreach (var link in node.Links)
                {
                    if (!usedNodes.Contains(link))
                    {
                        var chain = FindChain(link, usedNodes);
                        chains.AddRange(chain);
                    }
                }
            }

            return chains;
        }

        /// <summary>
        /// Находит всех ближайших соседей для каждого контура
        /// </summary>
        /// <param name="a"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static List<Node> FindLink(Node a, List<Node> nodes)
        {
            List<Node> links = new List<Node>();

            foreach (var item in nodes)
            {
                if (Math.Abs(a.Rect.X - item.Rect.X) < a.Rect.Width * MaxDistanceByWidth &&
                    Math.Abs(a.Rect.Y - item.Rect.Y) < a.Rect.Height * MaxDistanceByHeight &&
                    a.Id != item.Id)
                {
                    links.Add(item);
                }
            }

            return links;
        }

        /// <summary>
        /// Вычисляет расстояние между точками
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double GetDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        /// <summary>
        /// Поменять местами элементы
        /// </summary>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private static void Swap(ref Rect[] list, int i, int j)
        {
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        /// <summary>
        /// Исключаем длинные контуры
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Rect> TruncateLongContours(List<Rect> list)
        {
            return list.Where(x => x.Width < x.Height * MaxContourRatio && x.Height < x.Width * MaxContourRatio).ToList();
        }
    }
}