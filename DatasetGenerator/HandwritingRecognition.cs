using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DatasetGenerator
{
    public class HandwritingRecognition
    {
        public List<float> GetDatasetValues(Bitmap img, string backgroundColor)
        {
            var datasetValue = new List<float>();

            for (int i = 0; i < img.Height; i += 4)
            {
                for (int j = 0; j < img.Width; j += 4)
                {
                    var sum = 0;
                    for (int k = i; k < i + 4; k++)
                    {
                        for (int l = j; l < j + 4; l++)
                        {
                            sum += img.GetPixel(l, k).Name == backgroundColor ? 0 : 1;
                        }
                    }
                    datasetValue.Add(sum);
                }
            }
            return datasetValue;
        }
    }
}