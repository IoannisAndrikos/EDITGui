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
        Context context;

        public coreFunctionality(Context context)
        {
            this.context = context;
        }

        public static EDITProcessor.Processor editPro = new EDITProcessor.Processor();
        public static EDITCore.EDITResponse response = EDITCore.EDITResponse.Instance;



        public bool fillHoles = true;
        public List<double> meanThickness;
        public double uniqueContourMeanThickness;
        public List<double> pixelSpacing;
        public List<double> imageSize;

        [HandleProcessCorruptedStateExceptions]
        public void setExaminationsDirectory(String path)
        {
            try
            {
                editPro.setExaminationsDirectory(path);
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public void setStudySettings(double distanceBetweenFrames, double xspace, double yspace)
        {
            try
            {
                editPro.setStudySettings(distanceBetweenFrames, xspace, yspace);
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public string exportImages(string dcmfile)
        {
            try
            {
                editPro.exportUltrasoundImages(dcmfile);
                if (response.isSuccessful())
                {
                    pixelSpacing = response.getNumericData().GetRange(0,2);
                    imageSize = response.getNumericData().GetRange(2,2);
                    return response.getPath();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                displayFailureMessage(context.getMessages().errorOccured);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public List<List<EDITCore.CVPoint>> Bladder2DExtraction(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist,
            int startingFrame, int endingFrame, List<Point> userPoints, bool fixArtifact)
        {
            try
            {
                editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
                editPro.extractBladder(startingFrame, endingFrame, new EDITCore.CVPoint(userPoints[0].X, userPoints[0].Y), fixArtifact);
                if (response.isSuccessful())
                {
                    return response.getAllFramesData();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
                
            }catch(Exception e){
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return new List<List<EDITCore.CVPoint>>();
        }

        [HandleProcessCorruptedStateExceptions]
        public List<EDITCore.CVPoint> recalculateBladderOfContour(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist, 
            int frame, List<EDITCore.CVPoint> thicknessCvPoints, bool fixArtifact)
        {
            try
            {
                editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
                editPro.extractBladderForUniqueFrame(frame, thicknessCvPoints, fixArtifact);
                if (response.isSuccessful())
                {
                    return response.getUniqueFramesData();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return new List<EDITCore.CVPoint>();
        }



        //[HandleProcessCorruptedStateExceptions]
        //public List<List<EDITCore.CVPoint>> fixArtifact(int repeats, int smoothing, double lamda1, double lamda2, int levelsetSize, bool applyEqualizeHist,
        //   int startingFrame, int endingFrame, List<Point> userPoints, List<List<EDITCore.CVPoint>> bladderCvPoints)
        //{
        //    try
        //    {
        //        editPro.setSegmentationConfigurations(repeats, smoothing, lamda1, lamda2, levelsetSize, applyEqualizeHist);
        //        editPro.fixArtifact(new EDITCore.CVPoint(userPoints[0].X, userPoints[0].Y), bladderCvPoints);
        //        if (response.isSuccessful())
        //        {
        //            return response.getAllFramesData();
        //        }
        //        else
        //        {
        //            displayFailureMessage(response.getFailure());
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        displayFailureMessage(context.getMessages().errorOccured);
        //    }
        //    return new List<List<EDITCore.CVPoint>>();
        //}

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
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                displayFailureMessage(context.getMessages().errorOccured);
            }
        }


        [HandleProcessCorruptedStateExceptions]
        public String extractSkin(List<List<EDITCore.CVPoint>> bladderCvPoints)
        {
            try
            {
               editPro.extractSkinSTL(bladderCvPoints, fillHoles);
               if (response.isSuccessful())
               {
                   return response.getPath();
               }
               else
               {
                    displayFailureMessage(response.getFailure());
                }

            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                    pixelSpacing = response.getNumericData().GetRange(0, 2);
                    imageSize = response.getNumericData().GetRange(2, 2);
                    return response.getPath();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                    pixelSpacing = response.getNumericData().GetRange(0, 2);
                    imageSize = response.getNumericData().GetRange(2, 2);
                    return response.getPath();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public string exportGNRImages(string dcmfile, bool enablelogging)
        {
            try
            {
                editPro.exportGNRImages(dcmfile);
                if (response.isSuccessful())
                {
                    pixelSpacing = response.getNumericData().GetRange(0, 2);
                    imageSize = response.getNumericData().GetRange(2, 2);
                    return response.getPath();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public List<List<EDITCore.CVPoint>> extractThickness(List<List<EDITCore.CVPoint>> bladderCvPoints, double minThickness, double maxThickness, bool bigTumor)
        {
            try
            {
               editPro.setPhotoAcousticSegmentationConfigurations(minThickness, maxThickness, bigTumor);
               editPro.extractThickness(bladderCvPoints);
                if (response.isSuccessful())
                {
                    meanThickness = response.getNumericData();
                    return response.getAllFramesData();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return new List<List<EDITCore.CVPoint>>();
        }


        [HandleProcessCorruptedStateExceptions]
        public List<EDITCore.CVPoint> recalculateThicknessOfContour(int frame, List<EDITCore.CVPoint> thicknessCvPoints, double minThickness, double maxThickness, bool bigTumor)
        {
            try
            {
                editPro.setPhotoAcousticSegmentationConfigurations(minThickness, maxThickness, bigTumor);
                editPro.extractThicknessForUniqueFrame(frame, thicknessCvPoints);
                if (response.isSuccessful())
                {
                    uniqueContourMeanThickness = response.getNumericData()[0];
                    return response.getUniqueFramesData();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }   
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                    displayFailureMessage(response.getFailure());
                }
                
            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return null;
        }


        [HandleProcessCorruptedStateExceptions]
        public List<String> extractOXYandDeOXYPoints(List<List<EDITCore.CVPoint>> bladderCvPoints, List<List<EDITCore.CVPoint>> thicknessCvPoints, String bladderGeometryPath, String thicknessGeometryPath)
        {
            try
            {
                editPro.extractOXYandDeOXYPoints(bladderCvPoints, thicknessCvPoints, bladderGeometryPath, thicknessGeometryPath);
                if(response.isSuccessful())
                {
                    return response.getPaths();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }
              
            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return null;
        }

        [HandleProcessCorruptedStateExceptions]
        public String extractGNRPoints(List<List<EDITCore.CVPoint>> bladderCvPoints, List<List<EDITCore.CVPoint>> thicknessCvPoints, String bladderGeometryPath, String thicknessGeometryPath)
        {
            try
            {
                editPro.extractGNRPoints(bladderCvPoints, thicknessCvPoints, bladderGeometryPath, thicknessGeometryPath);
                if (response.isSuccessful())
                {
                    return response.getPath();
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }

            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
            return null;
        }



        //[HandleProcessCorruptedStateExceptions]
        //public List<List<EDITCore.CVPoint>> Tumor2DExtraction2D(Point startingPoint, List<List<EDITCore.CVPoint>> bladderCvPoints, List<List<EDITCore.CVPoint>> thicknessCvPoints, String bladderGeometryPath, String thicknessGeometryPath)
        //{
        //    try
        //    {
        //        editPro.extractTumor2D(new EDITCore.CVPoint(startingPoint.X, startingPoint.Y), bladderCvPoints, thicknessCvPoints, bladderGeometryPath, thicknessGeometryPath);
        //        if (response.isSuccessful())
        //        {
        //            return response.getAllFramesData();
        //        }
        //        else
        //        {
        //            displayFailureMessage(response.getFailure());
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        displayFailureMessage(context.getMessages().errorOccured);
        //    }
        //    return null;
        //}



        //[HandleProcessCorruptedStateExceptions]
        //public String Tumor2DExtraction3D()
        //{
        //    try
        //    {
        //        editPro.extractTumor3D();
        //        if (response.isSuccessful())
        //        {
        //            List<String> paths = response.getPaths();
        //            return paths[0];
        //        }
        //        else
        //        {
        //            displayFailureMessage(response.getFailure());
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        displayFailureMessage(context.getMessages().errorOccured);
        //    }
        //    return null;
        //}



        [HandleProcessCorruptedStateExceptions]
        public String Tumor3DExtraction(List<List<List<EDITCore.CVPoint>>> tumorCvPoints, int startingFrame, string bladderSTLPath, string thicknessSTLPath)
        {
            try
            {
                editPro.extractTumor3D(tumorCvPoints, startingFrame, bladderSTLPath, thicknessSTLPath);
                if (response.isSuccessful())
                {
                    String paths = response.getPath();
                    return paths;
                }
                else
                {
                    displayFailureMessage(response.getFailure());
                }

            }
            catch (Exception e)
            {
                displayFailureMessage(context.getMessages().errorOccured);
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
                displayFailureMessage(context.getMessages().errorOccured);
            }

        }


        //----------------------------------------------FILL VARIABLES WHEN LOAD DATA FROM UI---------------------------------

        [HandleProcessCorruptedStateExceptions]
        public void fill2DVariablesWhenLoadDataFromUI(int startingFrame, int endingFrame)
        {
            try
            {
                editPro.fill2DVariablesWhenLoadDataFromUI(startingFrame, endingFrame);
                if (!response.isSuccessful())
                {
                    displayFailureMessage(response.getFailure());
                }
            }
            catch (Exception)
            {
                displayFailureMessage(context.getMessages().errorOccured);
            }
        }


        public void displayFailureMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CustomMessageBox.Show(message, context.getMessages().error, MessageBoxButton.OK);
            }));
            return;
        }

    }
}
