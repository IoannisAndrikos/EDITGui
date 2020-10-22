﻿using System;
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
        public static EDITCore.EDITResponse response = EDITCore.EDITResponse.Instance;

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
                MessageBox.Show(errorMessages.errorOccured);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public string exportImages(string dcmfile, bool enablelogging)
        {
            try
            {
                editPro.exportUltrasoundImages(dcmfile);
                if (response.isSuccessful())
                {
                    pixelSpacing = response.getNumericData();
                    return response.getPath();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public void setLoggingOnOff(bool OnOff)
        {
            try
            {
                editPro.setLoggingOnOff(OnOff);
               
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessages.errorOccured);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public List<List<EDITCore.CVPoint>> Bladder2DExtraction(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist,
            int startingFrame, int endingFrame, List<Point> userPoints)
        {
            try
            {
                editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
                editPro.extractBladder(startingFrame, endingFrame, new EDITCore.CVPoint(userPoints[0].X, userPoints[0].Y));
                if (response.isSuccessful())
                {
                    return response.getAllFramesData();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
                
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
               editPro.extractBladderSTL(bladderCvPoints, fillHoles);
                if (response.isSuccessful())
                {
                    return response.getPath();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
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
               editPro.extractSkinSTL(bladderCvPoints, fillHoles);
                return response.getPath();
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
                editPro.exportOXYImages(dcmfile);
                if (response.isSuccessful())
                {
                    return response.getPath();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
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
                editPro.exportDeOXYImages(dcmfile);
                if (response.isSuccessful())
                {
                    return response.getPath();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
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
               editPro.extractThickness(bladderCvPoints);
               meanThickness = response.getNumericData();
                return response.getAllFramesData();
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
                editPro.extractThicknessForUniqueFrame(frame, thicknessCvPoints);
                meanThickness = response.getNumericData();
                return response.getUniqueFramesData();
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
               editPro.extractThicknessSTL(thicknessCvPoints, fillHoles);
                if (response.isSuccessful())
                {
                    return response.getPath();
                }
                else
                {
                    MessageBox.Show(response.getFailure());
                }
                
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
                editPro.extractOXYandDeOXYPoints(bladderCvPoints, thicknessCvPoints);
                return response.getPaths();
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
