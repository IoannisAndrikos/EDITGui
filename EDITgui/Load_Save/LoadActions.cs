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
        Context context;

        private string loadedStudyPath = null;

        public LoadActions(Context context)
        {
            this.context = context;
        }


        public string getLoadedStudyPath()
        {
            return this.loadedStudyPath;
        }

        public void setLoadedStudyPath(string path)
        {
            this.loadedStudyPath = path;
        }


        public void doLoad()
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = browserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //load available 2D Data
                loadAvailableData(browserDialog.SelectedPath);
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
            context.getUltrasoundPart().startSpinner();
            context.getPhotoAcousticPart().startSpinner();
            string studyName = Path.GetFileName(path);


            bool isStudyFolder = false;

            await Task.Run(() =>
            {
                ultrasoundDicomFile = getFolderName(path, FileType.UltrasoundDicomFile, false) + getProperFileName(FileType.UltrasoundDicomFile);
                if (File.Exists(ultrasoundDicomFile))
                {
                    ultrasoundDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), ultrasoundDicomFile, FileType.UltrasoundDicomFile);
                    UltrasoundimagesDir = context.getCore().exportImages(ultrasoundDicomFile);
                    pixelSpacing = context.getCore().pixelSpacing;
                    imageSize = context.getCore().imageSize;
                    isStudyFolder = true;
                }
                OXYDicomFile = getFolderName(path, FileType.OXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.OXYDicomFile);
                if (File.Exists(OXYDicomFile))
                {
                    OXYDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), OXYDicomFile, FileType.OXYDicomFile);
                    OXYImagesDir = context.getCore().exportOXYImages(OXYDicomFile, false);
                }
                DeOXYDicomFile = getFolderName(path, FileType.DeOXYDicomFile, false) + Path.DirectorySeparatorChar + getProperFileName(FileType.DeOXYDicomFile);
                if (File.Exists(DeOXYDicomFile))
                {
                    DeOXYDicomFile = copyFileToWorkspace(getWorkspaceDicomPath(), DeOXYDicomFile, FileType.DeOXYDicomFile);
                    DeOXYImagesDir = context.getCore().exportDeOXYImages(DeOXYDicomFile, false);
                }
            });

            context.getUltrasoundPart().stopSpinner();
            context.getPhotoAcousticPart().stopSpinner();

            if (!isStudyFolder)
            {
                CustomMessageBox.Show(context.getMessages().noCorrectStudyFolder, context.getMessages().warning, MessageBoxButton.OK);
                setLoadedStudyPath(null);
                return;
            }

            if (UltrasoundimagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                context.getMetrics().setPixelSpacing(pixelSpacing);
                context.getUltrasoundPart().AfterLoadUltrasoundDicom(studyName, ultrasoundDicomFile, UltrasoundimagesDir, pixelSpacing, imageSize);
            }

            if (OXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                context.getPhotoAcousticPart().AfterLoadOXYDicom(studyName, OXYDicomFile, OXYImagesDir, pixelSpacing, imageSize);
            }

            if (DeOXYImagesDir != null && pixelSpacing.Any() && imageSize.Any())
            {
                context.getPhotoAcousticPart().AfterLoadDeOXYDicom(studyName, DeOXYDicomFile, DeOXYImagesDir, pixelSpacing, imageSize);
            }

            //The order below plays significant role! Be careful here!
            loadAvailableBladderPoints(path);
            loadXMLSettingsFile(path);
            loadAlgorithmsSettings(path);
            loadAvailableThicknessPoints(path);
            loadAvailableTumorsPoints(path);
            load3DAvailableData(path);
            fill2DBackendVariables();

            context.getUltrasoundPart().updateCanvas();
            context.getPhotoAcousticPart().updateCanvas();

            setLoadedStudyPath(path);
            context.getSaveActions().saved();
        }

        public void load3DAvailableData(string path)
        {
            string oblectFileLocation = getWorkingObjetcts3DPath();

            context.getMainWindow().cleanVTKRender();

            string object3D;
            EDITgui.Geometry geometry;

            string Oblect3DFile = getFolderName(path, FileType.Bladder3D, false) + getProperFileName(FileType.Bladder3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Bladder3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.bladderGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
                context.getUltrasoundPart().bladderGeometryPath = object3D;
            }
            Oblect3DFile = getFolderName(path, FileType.Thickness3D, false) + getProperFileName(FileType.Thickness3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Thickness3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.outerWallGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
                context.getPhotoAcousticPart().thicknessGeometryPath = object3D;
            }
            Oblect3DFile = getFolderName(path, FileType.Layer3D, false) + getProperFileName(FileType.Layer3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Layer3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.layerGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
            }
            Oblect3DFile = getFolderName(path, FileType.OXY3D, false) + getProperFileName(FileType.OXY3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.OXY3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.oxyGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
            }
            Oblect3DFile = getFolderName(path, FileType.DeOXY3D, false) + getProperFileName(FileType.DeOXY3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.DeOXY3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.deoxyGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
            }
            Oblect3DFile = getFolderName(path, FileType.Tumors3D, false) + getProperFileName(FileType.Tumors3D);
            if (File.Exists(Oblect3DFile))
            {
                object3D = oblectFileLocation + getProperFileName(FileType.Tumors3D);
                File.Copy(Oblect3DFile, object3D, true);
                geometry = new Geometry() { geometryName = Messages.deoxyGeometry, Path = object3D, actor = null };
                context.getMainWindow().OnAddAvailableGeometry(geometry);
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
                setStudyGlogalSettings(settings);
            }
        }


        void setStudyGlogalSettings(Dictionary<string, double> settings)
        {
            try
            {
                //set settings of study
                context.getUltrasoundPart().startingFrame = (int)settings[settingType.StartingFrame.ToString()];
                context.getUltrasoundPart().endingFrame = (int)settings[settingType.EndingFrame.ToString()];
                Point clickpoint = new Point();
                clickpoint.X = (double)settings[settingType.ClickPointX.ToString()];
                clickpoint.Y = (double)settings[settingType.ClickPointY.ToString()];
                if (context.getUltrasoundPart().startingFrame == -1 || context.getUltrasoundPart().endingFrame == -1)
                {
                    context.getUltrasoundPart().contourSeg = UltrasoundPart.ContourSegmentation.INSERT_USER_POINTS;
                }
                else
                {
                    if (clickpoint.X != 0 && clickpoint.Y != 0)
                    {
                        context.getUltrasoundPart().userPoints.Add(clickpoint);
                        context.getUltrasoundPart().userPoints.Add(clickpoint);
                        context.getUltrasoundPart().contourSeg = UltrasoundPart.ContourSegmentation.CORRECTION;
                    }
                }

                //context.getUltrasoundPart().Repeats.Text = ((int)settings[settingType.Repeats.ToString()]).ToString();
                //context.getUltrasoundPart().Smoothing.Text = ((int)settings[settingType.Smoothing.ToString()]).ToString();
                //context.getUltrasoundPart().Lamda1.Text = string.Format("{0:0.0}", ((double)settings[settingType.Lamda1.ToString()]));
                //context.getUltrasoundPart().Lamda2.Text = string.Format("{0:0.0}", ((double)settings[settingType.Lamda2.ToString()]));
                //context.getUltrasoundPart().LevelsetSize.Text = ((int)settings[settingType.LevelSize.ToString()]).ToString();
                //context.getUltrasoundPart().chechBox_FIltering.IsChecked = ToBool((int)settings[settingType.Filtering.ToString()]);
                //context.getUltrasoundPart().closedSurface.IsChecked = ToBool((int)settings[settingType.ClosedSurface.ToString()]);
                //context.getCore().fillHoles = context.getUltrasoundPart().closedSurface.IsChecked.Value;
                //context.getPhotoAcousticPart().minThickness.Text = string.Format("{0:0.0}", ((double)settings[settingType.minThickness.ToString()]));
                //context.getPhotoAcousticPart().maxThickness.Text = string.Format("{0:0.0}", ((double)settings[settingType.maxThickness.ToString()]));
            }
            catch (Exception e)
            {
                CustomMessageBox.Show(messages.limitedSettingsFile, messages.warning, MessageBoxButton.OK);
            }
        }

        private void loadAlgorithmsSettings(string path)
        {
            string algoritmsSettingsFile = getFolderName(path, FileType.algorithmFrameSettings, false) + getProperFileName(FileType.algorithmFrameSettings);
            if (File.Exists(algoritmsSettingsFile))
            {
                StreamReader sr = new StreamReader(algoritmsSettingsFile);
                String line;
                int frame = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] col = line.Split(' ');
                    frame = Int32.Parse(col[1]);
                    context.getImages().getFrameSettings(frame).repeats = Int32.Parse(col[2]);
                    context.getImages().getFrameSettings(frame).smoothing = Int32.Parse(col[3]);
                    context.getImages().getFrameSettings(frame).lamda1 = Double.Parse(col[4], CultureInfo.InvariantCulture);
                    context.getImages().getFrameSettings(frame).lamda2 = Double.Parse(col[5], CultureInfo.InvariantCulture);
                    context.getImages().getFrameSettings(frame).levelSize = Int32.Parse(col[6]);
                    context.getImages().getFrameSettings(frame).filtering = ToBool(Int32.Parse(col[7]));
                    context.getImages().getFrameSettings(frame).probeArtifactCorrection = ToBool(Int32.Parse(col[8]));
                    context.getImages().getFrameSettings(frame).maxThickness = Double.Parse(col[9], CultureInfo.InvariantCulture);
                    context.getImages().getFrameSettings(frame).minThickness = Double.Parse(col[10], CultureInfo.InvariantCulture);
                    context.getImages().getFrameSettings(frame).majorThicknessExistence = ToBool(Int32.Parse(col[11]));
                }

                context.getUltrasoundPart().updateFrameAlgorithmSetting();
            }
        }

        private void loadAvailableBladderPoints(string path)
        {
            string framePointsFile;
            string pointsDir = getFolderName(path, FileType.bladderPoints, false);
            if (Directory.Exists(pointsDir))
            {
                int count = Directory.GetFiles(pointsDir).Length;
            
                for (int i = 0; i < count; i++)
                {
                    framePointsFile = pointsDir  + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        context.getImages().setBladderData(i, readPointsTXTFile(framePointsFile));
                    }
                }
            }
        }

        private void loadAvailableThicknessPoints(string path)
        {
            string framePointsFile;
            string pointsDir = getFolderName(path, FileType.thicknessPoints, false);
            if (Directory.Exists(pointsDir))
            {
                List<double> meanThinchnesslist = loadAvailableThicknessMetrics(path);

                int count = Directory.GetFiles(pointsDir).Length;
                for (int i = 0; i < count; i++)
                {
                    framePointsFile = pointsDir + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        context.getImages().setThicknessData(i, readPointsTXTFile(framePointsFile), meanThinchnesslist[i]);
                    }
                }

                meanThinchnesslist.Clear();
            }
        }

        private void loadAvailableTumorsPoints(string path)
        {
            string framePointsFile;
            string pointsDir = getFolderName(path, FileType.Tumors2D, false);
            List<List<Point>> points = new List<List<Point>>();
            if (Directory.Exists(pointsDir))
            {
                int count = Directory.GetFiles(pointsDir).Length;
                for (int i = 0; i < count; i++)
                {
                    framePointsFile = pointsDir + i.ToString() + ".txt";
                    if (File.Exists(framePointsFile))
                    {
                        StreamReader sr = new StreamReader(framePointsFile);
                        String line;
                        double x, y;
                        int k = -1;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains("tumor"))
                            {
                                k++;
                                points.Add(new List<Point>());
                            }
                            else
                            {
                                string[] col = line.Split(' ');
                                x = double.Parse(col[0].Replace(",", "."), CultureInfo.InvariantCulture);
                                y = double.Parse(col[1].Replace(",", "."), CultureInfo.InvariantCulture);
                                points[k].Add(new Point(x, y));
                            }
                        }
                        sr.Close();
                        context.getImages().setTumorsData(i, points);
                        points.Clear();
                    }
                }
            }
        }

        private List<double> loadAvailableThicknessMetrics(string path)
        {
            List<double> meanThicknessList = new List<double>();

            string metricsFile;
            string metricsDir = getFolderName(path, FileType.MeanThickness, false);
            if (Directory.Exists(metricsDir))
            {
                int count = Directory.GetFiles(metricsDir).Length;
                metricsFile = metricsDir + getProperFileName(FileType.MeanThickness);
                if (File.Exists(metricsFile))
                {
                    List<double> meanthickness = readMetricsTXTFile(metricsFile);
                    for (int i = 0; i < meanthickness.Count; i++)
                    {
                        meanThicknessList.Add(meanthickness[i]);
                    }
                }
            }

            return meanThicknessList;
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
            if(context.getUltrasoundPart().startingFrame!=-1 && context.getUltrasoundPart().endingFrame != -1)
            {
                context.getCore().fill2DVariablesWhenLoadDataFromUI(context.getUltrasoundPart().startingFrame, context.getUltrasoundPart().endingFrame);
            }
        }
    }
}
