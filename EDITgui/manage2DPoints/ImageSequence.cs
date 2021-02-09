using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    public class ImageSequence
    {
        Context context;
        int numOfFrames;

        List<Frame> frames = new List<Frame>();

        public ImageSequence(Context context)
        {
            this.context = context;
        }

        public void InitiallizeFrames(int numOfFrames)
        {
            this.numOfFrames = numOfFrames;
            frames.Clear();
            for (int i = 0; i < this.numOfFrames; i++)
            {
                this.frames.Add(new Frame(i));
            }
        }

        //---------------------------------- GETTERS ---------------------------------------
        //----------------------------------------------------------------------------------
        public List<Point> getBladderPoints()
        {
            return frames[getSliderValue()].Bladder.points;
        }

        public List<Point> getBladderPoints(int index)
        {
            return frames[index].Bladder.points;
        }

        public List<Point> getThicknessPoints()
        {
            return frames[getSliderValue()].Thickness.points;
        }

        public List<Point> getTumorPoints(int index)
        {
            return frames[getSliderValue()].Tumors[index].points;
        }

        public List<List<List<Point>>> getTotalTumorPoints()
        {
            List<List<List<Point>>> tumors = new List<List<List<Point>>>();
            for(int i=0; i< frames.Count; i++)
            {
                tumors.Add(new List<List<Point>>());
                for(int j =0; j < frames[i].Tumors.Count; j++)
                {
                    tumors[i].Add(new List<Point>());
                    for(int  k=0; k < frames[i].Tumors[j].points.Count; k++)
                    {
                        tumors[i][j].Add(frames[i].Tumors[j].points[k]);
                    }
                }
            }
            return tumors;
        }


        public FrameMetrics getBladderMetrics()
        {
            return new FrameMetrics() { area = frames[getSliderValue()].Bladder.area, perimeter = frames[getSliderValue()].Bladder.perimeter };
        }

        public FrameMetrics getBladderMetrics(int index)
        {
            return new FrameMetrics() { area = frames[index].Bladder.area, perimeter = frames[index].Bladder.perimeter };
        }

        public FrameMetrics getThicknessMetrics()
        {
            return new FrameMetrics() { area = frames[getSliderValue()].Thickness.area, perimeter = frames[getSliderValue()].Thickness.perimeter, meanThickness = frames[getSliderValue()].Thickness.meanThickness };
        }

        public FrameMetrics getThicknessMetrics(int index)
        {
            return new FrameMetrics() { area = frames[index].Thickness.area, perimeter = frames[index].Thickness.perimeter, meanThickness = frames[index].Thickness.meanThickness };
        }

        public FrameMetrics getTumorMetrics(int index)
        {
            return new FrameMetrics() { area = frames[getSliderValue()].Tumors[index].area, perimeter = frames[getSliderValue()].Tumors[index].perimeter };
        }

        public int getTumorItemsCount()
        {
            return frames[getSliderValue()].Tumors.Count;
        }

        //---------------------------------- SETTERS ---------------------------------------
        //----------------------------------------------------------------------------------

        public void setBladderData(int index, List<Point> points)
        {
            frames[index].Bladder.points.Clear();
            frames[index].Bladder.points.AddRange(points);
            frames[index].Bladder.area = context.getMetrics().calulateArea(points);
            frames[index].Bladder.perimeter = context.getMetrics().calulatePerimeter(points);
        }

        public void setThicknessData(int index, List<Point> points, double meanThickness)
        {
            frames[index].Thickness.points.Clear();
            frames[index].Thickness.points.AddRange(points);
            frames[index].Thickness.area = context.getMetrics().calulateArea(points);
            frames[index].Thickness.perimeter = context.getMetrics().calulatePerimeter(points);
            frames[index].Thickness.meanThickness = meanThickness;
        }

        public void setTumorsData(int index, List<List<Point>> points)
        {
            frames[index].Tumors.Clear();
            for(int i=0; i<points.Count; i++)
            {
                tumorItem item = new tumorItem() { points = points[i], area = context.getMetrics().calulateArea(points[i]), perimeter = context.getMetrics().calulatePerimeter(points[i]) };
                frames[index].Tumors.Add(item);
            }
        }

        //----------------------------------- RECALCULATE METRICS --------------------------------------
        //----------------------------------------------------------------------------------------------

        public void recalculateBladderMetrics()
        {
            frames[getSliderValue()].Bladder.area = context.getMetrics().calulateArea(frames[getSliderValue()].Bladder.points);
            frames[getSliderValue()].Bladder.perimeter = context.getMetrics().calulatePerimeter(frames[getSliderValue()].Bladder.points);
        }

        public void recalculateThicknessMetrics()
        {
            frames[getSliderValue()].Thickness.area = context.getMetrics().calulateArea(frames[getSliderValue()].Thickness.points);
            frames[getSliderValue()].Thickness.perimeter = context.getMetrics().calulatePerimeter(frames[getSliderValue()].Thickness.points);
        }

        public void recalculateTumorMetrics(int index)
        {
            frames[getSliderValue()].Tumors[index].area = context.getMetrics().calulateArea(frames[getSliderValue()].Tumors[index].points);
            frames[getSliderValue()].Tumors[index].perimeter = context.getMetrics().calulatePerimeter(frames[getSliderValue()].Tumors[index].points);

        }

        //--------------------------- ADD - REMOVE TUMOR --------------------------
        //-------------------------------------------------------------------------
        public int addTumor()
        {
            int index = frames[getSliderValue()].Tumors.Count;
            tumorItem item = new tumorItem() { points = new List<Point>(), area = 0, perimeter = 0 };
            frames[getSliderValue()].Tumors.Add(item);
            return index;
        }

        public void removeTumor(int index)
        {
            frames[getSliderValue()].Tumors.RemoveAt(index);
        }
        //-------------get all sequence points Bladder or Thickness---------------
        //------------------------------------------------------------------------

        public List<List<Point>> getTotalBladderPoints()
        {
            List<List<Point>> totalPoints = new List<List<Point>>();
            foreach (Frame frame in frames)
            {
                totalPoints.Add(frame.Bladder.points);
            }
            return totalPoints;
        }

        public List<List<Point>> getTotalThicknessPoints()
        {
            List<List<Point>> totalPoints = new List<List<Point>>();
            foreach (Frame frame in frames)
            {
                totalPoints.Add(frame.Thickness.points);
            }
            return totalPoints;
        }

        public List<double> getTotalMeanThickness()
        {
            List<double> meanThickenss = new List<double>();
            foreach (Frame frame in frames)
            {
                meanThickenss.Add(frame.Thickness.meanThickness);
            }
            return meanThickenss;
        }

        //----------------------FILL FRAMES FROM BACK-END--------------------------
        //-------------------------------------------------------------------------

        //Convert Point to EDITCore.CVPoint
        public void fillBladderFromBackEnd(List<List<EDITCore.CVPoint>> cvp, int startingFrame)
        {
            for (int i = 0; i < cvp.Count; i++)
            {
                frames[i + startingFrame].Bladder.points.Clear();
                for (int j = 0; j < cvp[i].Count(); j++)
                {
                    frames[i + startingFrame].Bladder.points.Add(new Point(cvp[i][j].X, cvp[i][j].Y)); // * (1 / calibration_x)
                    frames[i + startingFrame].Bladder.area = context.getMetrics().calulateArea(frames[i + startingFrame].Bladder.points);
                    frames[i + startingFrame].Bladder.perimeter = context.getMetrics().calulatePerimeter(frames[i + startingFrame].Bladder.points);
                }
            }
        }

        public void fillUniqueFrameBladderFromBackEnd(List<EDITCore.CVPoint> cvp)
        {
            frames[getSliderValue()].Bladder.points.Clear();
            for (int j = 0; j < cvp.Count(); j++)
            {
                frames[getSliderValue()].Bladder.points.Add(new Point(cvp[j].X, cvp[j].Y)); // * (1 / calibration_x)
                frames[getSliderValue()].Bladder.area = context.getMetrics().calulateArea(frames[getSliderValue()].Bladder.points);
                frames[getSliderValue()].Bladder.perimeter = context.getMetrics().calulatePerimeter(frames[getSliderValue()].Bladder.points);
            }
        }


        public void fillTumorFromBackEnd(List<List<EDITCore.CVPoint>> cvp, int startingFrame)
        {
            for (int i = 0; i < cvp.Count; i++)
            {
                frames[i + startingFrame].Tumors.Clear();
                tumorItem tumor = new tumorItem() { points = new List<Point>(), area = 0, perimeter = 0 };
                for (int j = 0; j < cvp[i].Count(); j++)
                {
                    tumor.points.Add(new Point(cvp[i][j].X, cvp[i][j].Y));
                   
                    frames[i + startingFrame].Bladder.area = context.getMetrics().calulateArea(frames[i + startingFrame].Bladder.points);
                    frames[i + startingFrame].Bladder.perimeter = context.getMetrics().calulatePerimeter(frames[i + startingFrame].Bladder.points);
                }
                tumor.area = context.getMetrics().calulateArea(tumor.points);
                tumor.perimeter = context.getMetrics().calulatePerimeter(tumor.points);
                frames[i + startingFrame].Tumors.Add(tumor);
            }
        }


        public void fillThicknessFromBackEnd(List<List<EDITCore.CVPoint>> cvp, List<double> meanThickness, int startingFrame)
        {
            for (int i = 0; i < cvp.Count; i++)
            {
                frames[i + startingFrame].Thickness.points.Clear();
                for (int j = 0; j < cvp[i].Count(); j++)
                {
                    frames[i + startingFrame].Thickness.points.Add(new Point(cvp[i][j].X, cvp[i][j].Y)); // * (1 / calibration_x)
                    frames[i + startingFrame].Thickness.area = context.getMetrics().calulateArea(frames[i + startingFrame].Thickness.points);
                    frames[i + startingFrame].Thickness.perimeter = context.getMetrics().calulatePerimeter(frames[i + startingFrame].Thickness.points);
                    frames[i + startingFrame].Thickness.meanThickness = meanThickness[i];
                }
            }
        }

        public void fillUniqueFrameThicknessFromBackEnd(List<EDITCore.CVPoint> cvp, double meanThickness)
        {
            frames[getSliderValue()].Thickness.points.Clear();
            for (int j = 0; j < cvp.Count(); j++)
            {
                frames[getSliderValue()].Thickness.points.Add(new Point(cvp[j].X, cvp[j].Y)); // * (1 / calibration_x)
                frames[getSliderValue()].Thickness.area = context.getMetrics().calulateArea(frames[getSliderValue()].Thickness.points);
                frames[getSliderValue()].Thickness.perimeter = context.getMetrics().calulatePerimeter(frames[getSliderValue()].Thickness.points);
                frames[getSliderValue()].Thickness.meanThickness = meanThickness;
            }
        }


        //Convert EDITCore.CVPoint to Point
        public List<List<EDITCore.CVPoint>> getAllFramesCVPoints(List<List<Point>> points)
        {
            List<List<EDITCore.CVPoint>> cvp = new List<List<EDITCore.CVPoint>>();

            string message = context.getCheck().checkForSegmentationGaps(points);
            if (message != null)
            {
                CustomMessageBox.Show(message, context.getMessages().warning, MessageBoxButton.OK);
                return cvp;
            }

            List<EDITCore.CVPoint> contour = new List<EDITCore.CVPoint>();

            //for (int i = startingFrame; i <= endingFrame; i++)
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Any())
                {
                    for (int j = 0; j < points[i].Count(); j++)
                    {
                        contour.Add(new EDITCore.CVPoint(points[i][j].X, points[i][j].Y));
                    }
                    cvp.Add(contour.ToList());
                    contour.Clear();
                }
            }
            return cvp;
        }

        public List<EDITCore.CVPoint> getUniqueFrameCVPoints(List<Point> points)
        {
            List<EDITCore.CVPoint> cvp = new List<EDITCore.CVPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                cvp.Add(new EDITCore.CVPoint(points[i].X, points[i].Y));

            }
            return cvp;
        }

        public List<Point> getForUniqueFrameWPFPoints(List<EDITCore.CVPoint> cvp)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < cvp.Count; i++)
            {
                points.Add(new Point(cvp[i].X, cvp[i].Y)); // * (1 / calibration_x)
            }
            return points;
        }


        //private 
        private int getSliderValue()
        {
            return context.getUltrasoundPart().slider_value;
        }
    }

    public class FrameMetrics
    {
        public double area { set; get; }
        public double perimeter { set; get; }
        public double meanThickness { set; get; }
    }
}
