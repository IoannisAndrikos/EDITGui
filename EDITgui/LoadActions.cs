using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
   public class LoadActions : StudyFile
   {
        Messages messages;
        coreFunctionality core;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        metricsCalculations metrics;
        MainWindow mainWindow;
        string workingPath;
        List<string> ObjectsPath;

        public LoadActions(MainWindow mainWindow, coreFunctionality core, UltrasoundPart ultrasound, PhotoAcousticPart photo, string workingPath)
        {
            this.mainWindow = mainWindow;
            this.core = core;
            this.ultrasound = ultrasound;
            this.photoAcoustic = photo;
            this.workingPath = workingPath;
            metrics = new metricsCalculations();
            messages = new Messages();
        }

        public async void loadAvailableData(string path)//async
        {
            List<double> pixelSpacing = new List<double>();
            List<double> imageSize = new List<double>();
            string ultrasoundDicomFile = null;
            string OXYDicomFile = null;
            string DeOXYDicomFile = null;
            string UltrasoundimagesDir = null;
            string OXYImagesDir = null;
            string DeOXYImagesDir = null;

            ultrasound.startSpinner();
            photoAcoustic.startSpinner();


           await Task.Run(() => {
                ultrasoundDicomFile = getFolderName(path, FileType.UltrasoundDicomFile, false) + getProperFileName(FileType.UltrasoundDicomFile);
                if (File.Exists(ultrasoundDicomFile))
                {
                    UltrasoundimagesDir = core.exportImages(ultrasoundDicomFile, false);
                    pixelSpacing = core.pixelSpacing;
                    imageSize = core.imageSize;
                }
                OXYDicomFile = getFolderName(path, FileType.OXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.OXYDicomFile);
                if (File.Exists(OXYDicomFile))
                {
                    OXYImagesDir = core.exportOXYImages(OXYDicomFile, false);
                }
                DeOXYDicomFile = getFolderName(path, FileType.DeOXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.DeOXYDicomFile);
                if (File.Exists(DeOXYDicomFile))
                {
                    DeOXYImagesDir = core.exportDeOXYImages(DeOXYDicomFile, false);
                }
            });


            if (UltrasoundimagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                metrics.setPixelSpacing(pixelSpacing);
                ultrasound.AfterLoadUltrasoundDicom(ultrasoundDicomFile, UltrasoundimagesDir, pixelSpacing, imageSize);
                
            }

            if (OXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                photoAcoustic.AfterLoadOXYDicom(OXYDicomFile, OXYImagesDir, pixelSpacing, imageSize);
            }

            if (DeOXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                photoAcoustic.AfterLoadDeOXYDicom(DeOXYDicomFile, DeOXYImagesDir, pixelSpacing, imageSize);
            }

            loadAvailableBladderPoints(path);
            loadInfoFile(path);
            loadAvailableThicknessPoints(path);
            loadAvailableThicknessMetrics(path);
            load3DAvailableData(path);
            fill2DBackendVariables();
            ultrasound.stopSpinner();
            photoAcoustic.stopSpinner();
        }


        public void load3DAvailableData(string path)
        {
            string oblectFileLocation = getWorkingObjetcts3DPath();

            mainWindow.cleanVTKRender();

            string object3D;
            EDITgui.Geometry geometry;

            string Oblect3DFile = getFolderName(path, FileType.Bladder3D, false) + getProperFileName(FileType.Bladder3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Bladder3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = "Bladder", Path = object3D, actor = null };
                mainWindow.OnAddAvailableGeometry(geometry);
                ultrasound.bladderGeometryPath = object3D;
            }
            Oblect3DFile = getFolderName(path, FileType.Thickness3D, false) + getProperFileName(FileType.Thickness3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Thickness3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = "Thickness", Path = object3D, actor = null };
                mainWindow.OnAddAvailableGeometry(geometry);
                photoAcoustic.thicknessGeometryPath = object3D;
            }
            Oblect3DFile = getFolderName(path, FileType.Layer3D, false) + getProperFileName(FileType.Layer3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Layer3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = "Layer", Path = object3D, actor = null };
                mainWindow.OnAddAvailableGeometry(geometry);
            }
            Oblect3DFile = getFolderName(path, FileType.OXY3D, false) + getProperFileName(FileType.OXY3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.OXY3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = "OXY", Path = object3D, actor = null };
                mainWindow.OnAddAvailableGeometry(geometry);
            }
            Oblect3DFile = getFolderName(path, FileType.DeOXY3D, false) + getProperFileName(FileType.DeOXY3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.DeOXY3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = "DeOXY", Path = object3D, actor = null };
                mainWindow.OnAddAvailableGeometry(geometry);
            }


        }


        public void loadInfoFile(string path)
        {
            Dictionary<string, double> info =  new Dictionary<string, double>();

            string infoFilePath = getFolderName(path, FileType.info, false) + getProperFileName(FileType.info);
            if (File.Exists(infoFilePath))
            {
                StreamReader sr = new StreamReader(infoFilePath);
                String line;
                string x;
                double y;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Any())
                    {
                        string[] col = line.Split(' ');
                        y = double.Parse(col[1].Replace(",", "."), CultureInfo.InvariantCulture);
                        info.Add(col[0], y);
                    }
                }
                sr.Close();
            }
            try
            {
                //set settings of study
                ultrasound.startingFrame = (int)info[infoType.StartingFrame.ToString()];
                ultrasound.endingFrame = (int)info[infoType.EndingFrame.ToString()];
                Point clickpoint = new Point();
                clickpoint.X = (double)info[infoType.ClickPointX.ToString()];
                clickpoint.Y = (double)info[infoType.ClickPointY.ToString()];
                if(clickpoint.X!=0 && clickpoint.Y != 0)
                {
                    ultrasound.userPoints.Add(clickpoint);
                    ultrasound.userPoints.Add(clickpoint);
                }
                ultrasound.Repeats.Text = ((int)info[infoType.Repeats.ToString()]).ToString();
                ultrasound.Smoothing.Text = ((int)info[infoType.Smoothing.ToString()]).ToString();
                ultrasound.Lamda1.Text = string.Format("{0:0.0}", ((double)info[infoType.Lamda1.ToString()]));
                ultrasound.Lamda2.Text = string.Format("{0:0.0}", ((double)info[infoType.Lamda2.ToString()]));
                ultrasound.LevelsetSize.Text = ((int)info[infoType.LevelSize.ToString()]).ToString();
                ultrasound.chechBox_FIltering.IsChecked = ToBool((int)info[infoType.Filtering.ToString()]);
                ultrasound.chechBox_Logger.IsChecked = ToBool((int)info[infoType.Logger.ToString()]);
                ultrasound.closedSurface.IsChecked = ToBool((int)info[infoType.ClosedSurface.ToString()]);
                core.fillHoles = ultrasound.closedSurface.IsChecked.Value;
                photoAcoustic.minThickness.Text = string.Format("{0:0.0}", ((double)info[infoType.minThickness.ToString()]));
                photoAcoustic.maxThickness.Text = string.Format("{0:0.0}", ((double)info[infoType.maxThickness.ToString()]));
            }
            catch (Exception e)
            {
                MessageBox.Show(messages.limitedInfoFile);
            }
          
        }



        private void loadAvailableBladderPoints(string path)
        {
            string framePointsFile;
            string pointsDir = getFolderName(path, FileType.bladderPoints, false);
            if (Directory.Exists(pointsDir))
            {
                int count = Directory.GetFiles(pointsDir).Length;
                if (count > 0)
                {
                    ultrasound.bladder.Clear();
                    ultrasound.bladderArea.Clear();
                    ultrasound.bladderPerimeter.Clear();
                }
                for (int i=0; i<count; i++)
                {
                    framePointsFile = pointsDir + Path.DirectorySeparatorChar + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        ultrasound.bladder.Add(readPointsTXTFile(framePointsFile));
                        ultrasound.bladderArea.Add(metrics.calulateArea(ultrasound.bladder[i]));
                        ultrasound.bladderPerimeter.Add(metrics.calulatePerimeter(ultrasound.bladder[i]));
                    }
                }

                if (ultrasound.bladder.Any())
                {
                    ultrasound.contourSeg = UltrasoundPart.ContourSegmentation.CORRECTION;
                }
            }   
        }



        private void loadAvailableThicknessPoints(string path)
        {
            string framePointsFile;
            string pointsDir = getFolderName(path, FileType.thicknessPoints, false);
            if (Directory.Exists(pointsDir))
            {
                int count = Directory.GetFiles(pointsDir).Length;
                if (count > 0) photoAcoustic.thickness.Clear();
                for (int i = 0; i < count; i++)
                {
                    framePointsFile = pointsDir + Path.DirectorySeparatorChar + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        photoAcoustic.thickness.Add(readPointsTXTFile(framePointsFile));
                    }
                }
                if (photoAcoustic.thickness.Any())
                {
                    photoAcoustic.contourSeg = PhotoAcousticPart.ContourSegmentation.CORRECTION;
                    //photoAcoustic.bladderUltrasound = ultrasound.getBladderPoints().ToList();
                }
            }
        }


        private void loadAvailableThicknessMetrics(string path)
        {
            string metricsFile;
            string metricsDir = getFolderName(path, FileType.MeanThickness, false);
            if (Directory.Exists(metricsDir))
            {
                int count = Directory.GetFiles(metricsDir).Length;
                metricsFile = metricsDir + getProperFileName(FileType.MeanThickness);
                if (File.Exists(metricsFile))
                {
                    photoAcoustic.meanThickness.Clear();
                    photoAcoustic.meanThickness = readMetricsTXTFile(metricsFile);
                }
            }
        }


        private List<Point> readPointsTXTFile(string filename)
        {
            List<Point> points = new List<Point>();

            StreamReader sr = new StreamReader(filename);
            String line;
            double x, y;
            while ((line = sr.ReadLine()) != null)
            {
                string[] col = line.Split(' ');

                x = double.Parse(col[0].Replace(",", "."), CultureInfo.InvariantCulture);
                y = double.Parse(col[1].Replace(",", "."), CultureInfo.InvariantCulture);

                points.Add(new Point(x, y));

            }
            sr.Close();

            return points;
        }


        private List<double> readMetricsTXTFile(string filename)
        {
            List<double> metrics = new List<double>();

            StreamReader sr = new StreamReader(filename);
            String line;
            double y;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Any())
                {
                    string[] col = line.Split(' ');
                    y = double.Parse(col[1].Replace(",", "."), CultureInfo.InvariantCulture);

                    metrics.Add(y);
                }
            }
            sr.Close();

            return metrics;
        }

        public static bool ToBool(int value)
        {
            return (value==0) ? false : true;
        }

        void fill2DBackendVariables()
        {
            if(ultrasound.startingFrame!=-1 && ultrasound.endingFrame != -1)
            {
                core.fill2DVariablesWhenLoadDataFromUI(ultrasound.startingFrame, ultrasound.endingFrame);
            }
        }
    }
}
