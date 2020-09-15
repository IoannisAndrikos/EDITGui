using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.ExceptionServices;

namespace EDITgui
{
    //here is the core functionality of backend
    public class coreFunctionality
    {
        public static EDITProcessor.Processor editPro = new EDITProcessor.Processor();
        public static Messages errorMessages = new Messages();
 
        public void setExaminationsDirectory(String path)
        {
            try
            {
                editPro.setExaminationsDirectory(path);
            }
            catch (Exception)
            {
                MessageBox.Show("Problem!");
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public string exportImages(string dcmfile, bool enablelogging)
        {
            try
            {
                string imagesDir = editPro.exportImages(dcmfile, enablelogging);
                return imagesDir;
            }
            catch (Exception)
            {
                MessageBox.Show("Problem!");
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public List<double> getPixelSpacing()
        {
            try
            {
                List<double> pixelSpacing = editPro.getPixelSpacing();
                return pixelSpacing;
            }
            catch (Exception)
            {
                MessageBox.Show("Problem!");
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public List<List<EDITCore.CVPoint>> Bladder2DExtraction(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist,
            int startingFrame, int endingFrame, List<Point> userPoints)
        {
            try
            {
                editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
                List<List<EDITCore.CVPoint>> bladderCvPoints = editPro.extractBladder(startingFrame, endingFrame, new EDITCore.CVPoint(userPoints[0].X, userPoints[0].Y));
                return bladderCvPoints;
            }catch(Exception e){
                MessageBox.Show("Problem!");
            }
            return new List<List<EDITCore.CVPoint>>();
        }

        public void repeatSegmentation()
        {
            editPro.repeatSegmentation();
        }

        [HandleProcessCorruptedStateExceptions]
        public void extractBladderSTL(List<List<EDITCore.CVPoint>> bladderCvPoints)
        {
            try
            {
                editPro.extractBladderSTL(bladderCvPoints);
            }
            catch (Exception e)
            {
                MessageBox.Show(errorMessages.problemToProduceSTL);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public void writePointsAndImages()
        {
            try
            {
                editPro.writePointsAndImages();
            }
            catch (Exception e)
            {
                MessageBox.Show("Problem!");
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public void extractSkin()
        {
            try
            {
                editPro.extractSkin();
            }
            catch (Exception e)
            {
                MessageBox.Show("Problem!");
            }
        }

    }

}
