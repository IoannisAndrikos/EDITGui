using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

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


        public void doLoad()
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = browserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //load available 2D Data
                loadAvailableData(browserDialog.SelectedPath);
                this.mainWindow.loadedStudyPath = browserDialog.SelectedPath;
            }
        }

        public async void loadAvailableData(string path)
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
            string studyName = Path.GetFileName(path);

            await Task.Run(() =>
            {
                ultrasoundDicomFile = getFolderName(path, FileType.UltrasoundDicomFile, false) + getProperFileName(FileType.UltrasoundDicomFile);
                if (File.Exists(ultrasoundDicomFile))
                {
                    ultrasoundDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), ultrasoundDicomFile, FileType.UltrasoundDicomFile);
                    UltrasoundimagesDir = core.exportImages(ultrasoundDicomFile, false);
                    pixelSpacing = core.pixelSpacing;
                    imageSize = core.imageSize;
                }
                OXYDicomFile = getFolderName(path, FileType.OXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.OXYDicomFile);
                if (File.Exists(OXYDicomFile))
                {
                    OXYDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), OXYDicomFile, FileType.OXYDicomFile);
                    OXYImagesDir = core.exportOXYImages(OXYDicomFile, false);
                }
                DeOXYDicomFile = getFolderName(path, FileType.DeOXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.DeOXYDicomFile);
                if (File.Exists(DeOXYDicomFile))
                {
                    DeOXYDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), DeOXYDicomFile, FileType.DeOXYDicomFile);
                    DeOXYImagesDir = core.exportDeOXYImages(DeOXYDicomFile, false);
                }
            });

            ultrasound.stopSpinner();
            photoAcoustic.stopSpinner();

            if (UltrasoundimagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                metrics.setPixelSpacing(pixelSpacing);
                ultrasound.AfterLoadUltrasoundDicom(studyName, ultrasoundDicomFile, UltrasoundimagesDir, pixelSpacing, imageSize);
            }

            if (OXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                photoAcoustic.AfterLoadOXYDicom(studyName, OXYDicomFile, OXYImagesDir, pixelSpacing, imageSize);
            }

            if (DeOXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                photoAcoustic.AfterLoadDeOXYDicom(studyName, DeOXYDicomFile, DeOXYImagesDir, pixelSpacing, imageSize);
            }

            //The order below plays significant role! Be careful here!
            loadAvailableBladderPoints(path);
            loadXMLSettingsFile(path);
            loadAvailableThicknessPoints(path);
            loadAvailableThicknessMetrics(path);
            load3DAvailableData(path);
            fill2DBackendVariables();
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

        public void loadXMLSettingsFile(string path)
        {
            string infoFilePath = getFolderName(path, FileType.settings, false) + getProperFileName(FileType.settings);
            if (File.Exists(infoFilePath))
            {
                Dictionary<string, double> settings = new Dictionary<string, double>();
                XmlDocument doc = new XmlDocument();
                doc.Load(infoFilePath);
                double y;
                foreach (XmlElement xmlElement in doc.DocumentElement)
                {
                    y = double.Parse(xmlElement.InnerText.Replace(",", "."), CultureInfo.InvariantCulture);
                    settings.Add(xmlElement.Name, y);
                }
                setStudySettings(settings);
            }
        }


        void setStudySettings(Dictionary<string, double> settings)
        {
            try
            {
                //set settings of study
                ultrasound.startingFrame = (int)settings[settingType.StartingFrame.ToString()];
                ultrasound.endingFrame = (int)settings[settingType.EndingFrame.ToString()];
                Point clickpoint = new Point();
                clickpoint.X = (double)settings[settingType.ClickPointX.ToString()];
                clickpoint.Y = (double)settings[settingType.ClickPointY.ToString()];
                if (ultrasound.startingFrame == -1 || ultrasound.endingFrame == -1)
                {
                    ultrasound.contourSeg = UltrasoundPart.ContourSegmentation.INSERT_USER_POINTS;
                }
                else
                {
                    if (clickpoint.X != 0 && clickpoint.Y != 0)
                    {
                        ultrasound.userPoints.Add(clickpoint);
                        ultrasound.userPoints.Add(clickpoint);
                        ultrasound.contourSeg = UltrasoundPart.ContourSegmentation.CORRECTION;
                    }
                }
               
                ultrasound.Repeats.Text = ((int)settings[settingType.Repeats.ToString()]).ToString();
                ultrasound.Smoothing.Text = ((int)settings[settingType.Smoothing.ToString()]).ToString();
                ultrasound.Lamda1.Text = string.Format("{0:0.0}", ((double)settings[settingType.Lamda1.ToString()]));
                ultrasound.Lamda2.Text = string.Format("{0:0.0}", ((double)settings[settingType.Lamda2.ToString()]));
                ultrasound.LevelsetSize.Text = ((int)settings[settingType.LevelSize.ToString()]).ToString();
                ultrasound.chechBox_FIltering.IsChecked = ToBool((int)settings[settingType.Filtering.ToString()]);
                ultrasound.chechBox_Logger.IsChecked = ToBool((int)settings[settingType.Logger.ToString()]);
                ultrasound.closedSurface.IsChecked = ToBool((int)settings[settingType.ClosedSurface.ToString()]);
                core.fillHoles = ultrasound.closedSurface.IsChecked.Value;
                photoAcoustic.minThickness.Text = string.Format("{0:0.0}", ((double)settings[settingType.minThickness.ToString()]));
                photoAcoustic.maxThickness.Text = string.Format("{0:0.0}", ((double)settings[settingType.maxThickness.ToString()]));
            }
            catch (Exception e)
            {
                CustomMessageBox.Show(messages.limitedSettingsFile, messages.warning, MessageBoxButton.OK);
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
                if (count > 0) {
                    photoAcoustic.thickness.Clear();
                    photoAcoustic.thicknessArea.Clear();
                    photoAcoustic.thicknessPerimeter.Clear();
                }
                for (int i = 0; i < count; i++)
                {
                    framePointsFile = pointsDir + Path.DirectorySeparatorChar + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        photoAcoustic.thickness.Add(readPointsTXTFile(framePointsFile));
                        photoAcoustic.thicknessArea.Add(metrics.calulateArea(photoAcoustic.thickness[i]));
                        photoAcoustic.thicknessPerimeter.Add(metrics.calulatePerimeter(photoAcoustic.thickness[i]));
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
