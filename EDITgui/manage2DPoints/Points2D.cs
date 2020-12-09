using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    public class Points2D
    {
        Context context;

        public enum oject2DType { bladder, thickness, tumor}

        //ultrasound data
        public List<List<Point>> bladder = new List<List<Point>>();
        public List<double> bladderArea = new List<double>();
        public List<double> bladderPerimeter = new List<double>();

        //photoAcoustic data
        public List<List<Point>> thickness = new List<List<Point>>();
        public List<double> meanThickness = new List<double>();
        public List<double> thicknessArea = new List<double>();
        public List<double> thicknessPerimeter = new List<double>();

        //tumors
        public List<List<tumorItem>> tumors = new List<List<tumorItem>>();

        public Points2D(Context context)
        {
            this.context = context;
        }


        List<Point> emptyList = new List<Point>();
        public void initializationOfPoints2D(int fileCount)
        {
            bladder.Clear();
            bladderArea.Clear();
            bladderPerimeter.Clear();
            thickness.Clear();
            meanThickness.Clear();
            thicknessArea.Clear();
            thicknessPerimeter.Clear();

            for(int i=0; i<fileCount; i++)
            {
                bladder.Add(emptyList);
                bladderArea.Add(0);
                bladderPerimeter.Add(0);
                thickness.Add(emptyList);
                meanThickness.Add(0);
                thicknessArea.Add(0);
                thicknessPerimeter.Add(0);
            }
        }


        public List<Point> getObjectPoints2D(oject2DType type, int selected)
        {
            switch (type)
            {
                case oject2DType.bladder:
                    return this.bladder[context.getUltrasoundPart().slider_value];
                    break;
                case oject2DType.thickness:
                    return this.thickness[context.getUltrasoundPart().slider_value];
                    break;
                case oject2DType.tumor:
                    return this.tumors[context.getUltrasoundPart().slider_value][selected].points;
                    break;
            }

            return new List<Point>();
        }

    }
}
