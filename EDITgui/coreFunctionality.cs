using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    //here is the core functionality of backend
    class coreFunctionality
    {
        public static EDITProcessor.Processor editPro = new EDITProcessor.Processor();


        public void setExaminationsDirectory(String path)
        {
            editPro.setExaminationsDirectory(path);
        }


        public string exportImages(string dcmfile, bool enablelogging)
        {

            string imagesDir = editPro.exportImages(dcmfile, enablelogging);
            return imagesDir;
        }


        public List<List<EDITCore.CVPoint>> Bladder2DExtraction(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist,
            int startingFrame, int endingFrame, List<Point> userPoints)
        {
            editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
            List<List<EDITCore.CVPoint>> bladderCvPoints = editPro.extractBladder(startingFrame, endingFrame, new EDITCore.CVPoint(userPoints[0].X, userPoints[0].Y));
            return bladderCvPoints;
        }

        public void repeatSegmentation()
        {
            editPro.repeatSegmentation();
        }


        public void extractBladderSTL(List<List<EDITCore.CVPoint>> bladderCvPoints)
        {
            editPro.extractBladderSTL(bladderCvPoints);
        }

        public void writePointsAndImages()
        {
            editPro.writePointsAndImages();
        }

        public void extractSkin()
        {
            editPro.extractSkin();
        }


    }

}
