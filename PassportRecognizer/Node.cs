using System.Collections.Generic;
using OpenCvSharp;

namespace cv2
{
    public class Node
    {
        public int Id { get; set; }
        public List<Node> Links { get; set; }
        public Rect Rect { get; set; }
    }
}