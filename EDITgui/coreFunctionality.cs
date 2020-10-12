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


        public bool fillHoles = true;
        public List<double> meanThickness;
        public List<double> pixelSpacing;

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
                pixelSpacing = editPro.getPixelSpacing();
                return imagesDir;
            }
            catch (Exception)
            {
                MessageBox.Show("Problem!");
            }
            return null;
        }


        //[HandleProcessCorruptedStateExceptions]
        //public List<double> getPixelSpacing()
        //{
        //    try
        //    {
        //        List<double> pixelSpacing = editPro.getPixelSpacing();
        //        return pixelSpacing;
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Problem!");
        //    }
        //    return null;
        //}


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
                MessageBox.Show(errorMessages.errorOccured);
            }
            return new List<List<EDITCore.CVPoint>>();
        }

        public void repeatSegmentation()
        {
            editPro.repeatSegmentation();
        }

        [HandleProcessCorruptedStateExceptions]
        public String extractBladderSTL(List<List<EDITCore.CVPoint>> bladderCvPoints)
        {
            try
            {
               String STLPath = editPro.extractBladderSTL(bladderCvPoints, fillHoles);
                return STLPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(errorMessages.problemToProduceSTL);
            }
            return null;
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
                MessageBox.Show(errorMessages.errorOccured);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public String extractSkin(List<List<EDITCore.CVPoint>> bladderCvPoints)
        {
            try
            {
               String STLPath =  editPro.extractSkinSTL(bladderCvPoints, fillHoles);
                return STLPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return null;
        }

        //------------------------------------PHOTOACOUSTIC----------------------------

        [HandleProcessCorruptedStateExceptions]
        public string exportOXYImages(string dcmfile, bool enablelogging)
        {
            try
            {
                string imagesDir = editPro.exportOXYImages(dcmfile, enablelogging);
                return imagesDir;
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return null;
        }



        [HandleProcessCorruptedStateExceptions]
        public string exportDeOXYImages(string dcmfile, bool enablelogging)
        {
            try
            {
                string imagesDir = editPro.exportDeOXYImages(dcmfile, enablelogging);
                return imagesDir;
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return null;
        }



        [HandleProcessCorruptedStateExceptions]
        public List<List<EDITCore.CVPoint>> extractThickness(List<List<EDITCore.CVPoint>> bladderCvPoints, double minThickness, double maxThickness)
        {
            try
            {
                editPro.setPhotoAcousticSegmentationConfigurations(minThickness, maxThickness);
                List<List<EDITCore.CVPoint>> thicknessCvPoints = editPro.extractThickness(bladderCvPoints);
                meanThickness = editPro.getMeanThickness();
                return thicknessCvPoints;
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return new List<List<EDITCore.CVPoint>>();
        }


        [HandleProcessCorruptedStateExceptions]
        public List<EDITCore.CVPoint> recalculateThicknessOfContour(int frame, List<EDITCore.CVPoint> thicknessCvPoints, double minThickness, double maxThickness)
        {
            try
            {
                editPro.setPhotoAcousticSegmentationConfigurations(minThickness, maxThickness);
                List<EDITCore.CVPoint> newThicknessCvPoints = editPro.extractThicknessForUniqueFrame(frame, thicknessCvPoints);
                meanThickness = editPro.getMeanThickness();
                return newThicknessCvPoints;
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return new List<EDITCore.CVPoint>();
        }


        [HandleProcessCorruptedStateExceptions]
        public String extractThicknessSTL(List<List<EDITCore.CVPoint>> thicknessCvPoints)
        {
            try
            {
                String STLPath = editPro.extractThicknessSTL(thicknessCvPoints, fillHoles);
                return STLPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(errorMessages.problemToProduceSTL);
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public List<String> extractOXYandDeOXYPoints(List<List<EDITCore.CVPoint>> bladderCvPoints, List<List<EDITCore.CVPoint>> thicknessCvPoints)
        {
            try
            {
                List<String> txtPath = editPro.extractOXYandDeOXYPoints(bladderCvPoints, thicknessCvPoints);
                return txtPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(errorMessages.problemToProduceSTL);
            }
            return null;
        }




        [HandleProcessCorruptedStateExceptions]
        public void writeThicknessPoints(List<List<EDITCore.CVPoint>> thicknessCvPoints)
        {
            try
            {
                editPro.writeThicknessPoints(thicknessCvPoints);

            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }

        }



    }

}
