using OpenCvSharp;
using System.Collections.Generic;

namespace cv2
{
    public class Node
    {
        public int Id { get; set; }
        public List<Node> Links { get; set; }
        public Rect Rect { get; set; }
    }
}