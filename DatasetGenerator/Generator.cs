using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DatasetGenerator
{
    public class Generator
    {
        public void Generate()
        {
            var fontsList = new List<string> { "Arial", "Arial Narrow", "Calibri", "Times New Roman", "Calibri Light" };
            var fontStyles = new List<FontStyle> { FontStyle.Bold };
            var pathDataset = @"dataset.csv";
            var drawString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var backgroundColor = "ffffffff";
            var saveFiles = true;
            var filesCatalogName = "images";

            var tw = new StreamWriter(pathDataset);
            tw.WriteLine("PixelValues,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Number");

            if (saveFiles)
            {
                var dn = new DirectoryInfo(filesCatalogName);
                if (!dn.Exists)
                {
                    dn.Create();
                }
            }

            foreach (var c in drawString)
            {
                foreach (var fontName in fontsList)
                {
                    for (int size = 10; size < 26; size++)
                    {
                        foreach (var fontStyle in fontStyles)
                        {
                            for (int angle = -13; angle < 13; angle++)
                            {
                                var res = new Bitmap(32, 32);

                                // Create font and brush.
                                Font drawFont = new Font(fontName, size, fontStyle);
                                var drawBrush = new SolidBrush(Color.Black);

                                // Create point for upper-left corner of drawing.
                                float x = 1;
                                float y = 1;

                                // Set format of string.
                                StringFormat drawFormat = new StringFormat();

                                using (var g = Graphics.FromImage(res))
                                {
                                    g.Clear(Color.White);
                                    g.RotateTransform(angle); // set up rotate
                                    g.DrawString(c.ToString(), drawFont, drawBrush, x, y, drawFormat);
                                }
                                if (saveFiles)
                                {
                                    var path = $"{filesCatalogName}\\{c}_{Guid.NewGuid()}.png";
                                    res.Save(path);
                                }

                                var handwritingRecognition = new HandwritingRecognition();

                                var datasetValue = handwritingRecognition.GetDatasetValues(res, backgroundColor);

                                tw.WriteLine($"{string.Join(",", datasetValue)},{c - 'A'}");
                            }
                        }
                    }
                }
            }
            tw.Close();
        }
    }
}