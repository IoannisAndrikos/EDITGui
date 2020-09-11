using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    //here is a class where we calculate some metrics such as contour area, perimeter etc
    class metricsCalculations
    {
        List<double> pixelSpacing = new List<double>();
        
        public void setPixelSpacing(List<double> pixelSpacing)
        {
            this.pixelSpacing = pixelSpacing.ToList();
        }

        public double calulateArea(List<Point> inputContour)
        {
            List<Point> contour = inputContour.ToList();

            if (contour.Count >= 3)
            {
                contour.Add(contour[0]);
                var area = Math.Abs(contour.Take(contour.Count - 1)
                   .Select((p, i) => (contour[i + 1].X * pixelSpacing[0] - p.X * pixelSpacing[0]) * (contour[i + 1].Y * pixelSpacing[1] + p.Y * pixelSpacing[1]))
                   .Sum() / 2);



                var perimeter = Math.Abs(contour.Take(contour.Count - 1)
                    .Select((p, i) => (Math.Sqrt(Math.Pow(contour[i + 1].X * pixelSpacing[0] - p.X * pixelSpacing[0], 2) + Math.Pow(contour[i + 1].Y * pixelSpacing[1] - p.Y * pixelSpacing[1], 2))))
                   .Sum());


                return area;
            }
            else
            {
                return 0;
            }
        }


        public double calulatePerimeter(List<Point> inputContour)
        {
            List<Point> contour = inputContour.ToList();

            if (contour.Count >= 3)
            {
                var perimeter = Math.Abs(contour.Take(contour.Count - 1)
                   .Select((p, i) => (Math.Sqrt(Math.Pow(contour[i + 1].X * pixelSpacing[0] - p.X * pixelSpacing[0], 2) + Math.Pow(contour[i + 1].Y * pixelSpacing[1] - p.Y * pixelSpacing[1], 2))))
                  .Sum());

                return perimeter;
            }
            else
            {
                return 0;
            }
        }
    }
}
