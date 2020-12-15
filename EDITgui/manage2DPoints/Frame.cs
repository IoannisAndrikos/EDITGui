using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    public class Frame
    {
        public int frameIndex;
        public bladder Bladder;
        public thickness Thickness;
        public List<tumorItem> Tumors = new List<tumorItem>();

        public Frame(int frameIndex)
        {
            this.frameIndex = frameIndex;
            this.Bladder = new bladder() { points = new List<Point>(), area = 0, perimeter = 0 };
            this.Thickness = new thickness() { points = new List<Point>(), area = 0, perimeter = 0, meanThickness = 0 };
        }
    }

    public class bladder
    {
        public List<Point> points { get; set; }
        public double area { get; set; }
        public double perimeter { get; set; }
    }

    public class thickness
    {
        public List<Point> points { get; set; }
        public double meanThickness { get; set; }
        public double area { get; set; }
        public double perimeter { get; set; }
    }

    public class tumorItem
    {
        public List<Point> points { get; set; }
        public double area { get; set; }
        public double perimeter { get; set; }
    }
}
