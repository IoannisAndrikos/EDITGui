using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EDITgui
{
    public class Frame
    {
        public int frameIndex;
        public bladder Bladder;
        public thickness Thickness;
        public List<tumorItem> Tumors = new List<tumorItem>();
        public AlgorithmSettings frameSettings;


        public Frame(int frameIndex)
        {
            this.frameIndex = frameIndex;
            this.Bladder = new bladder() { points = new List<Point>(), area = 0, perimeter = 0 };
            this.Thickness = new thickness() { points = new List<Point>(), area = 0, perimeter = 0, meanThickness = 0 };
            this.frameSettings = new AlgorithmSettings();
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
        //public enum tumorType { tumor1, tumor2, tumor3, tumor4 };

        public List<Point> points { get; set; }
        public string group { get; set; }
        public double area { get; set; }
        public double perimeter { get; set; }
    } 

    //this values should be defined into a static file
    public class AlgorithmSettings
    {
        public AlgorithmSettings() { }

        public int repeats = 20;
        public int smoothing = 3;
        public double lamda1 = 1.0;
        public double lamda2 = 1.0;
        public int levelSize = 40;
        public bool filtering = false;
        public bool probeArtifactCorrection = false;
        public double minThickness = 0.3;
        public double maxThickness = 0.7;
        public bool majorThicknessExistence = false;
    }

}
