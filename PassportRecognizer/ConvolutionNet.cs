using System.Drawing;
using System.IO;
using DatasetGenerator;
using PassportRecognizerML.Model;

namespace PassportRecognizer
{
    /// <summary>
    /// Свёрточная сетка
    /// </summary>
    internal class ConvolutionNet
    {
        public void RecognizeLetter(string filename)
        {
            if (!File.Exists(filename)) return;

            Bitmap bit = new Bitmap(filename);
            Bitmap resized = new Bitmap(bit, 32, 32);

            var hand = new HandwritingRecognition();
            var data = hand.GetDatasetValues(resized, "ffffffff");

            var input = new ModelInput();
            input.PixelValues = data.ToArray();

            ModelOutput result = ConsumeModel.Predict(input);
        }

        public void RecognizeImage(Bitmap image)
        {
        }
    }
}