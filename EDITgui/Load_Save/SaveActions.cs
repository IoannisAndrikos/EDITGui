using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Xml;
using Microsoft.Win32;

namespace EDITgui
{
    public class SaveActions : StudyFile
    {


        Messages messages;
        coreFunctionality core;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        metricsCalculations metrics;
        MainWindow mainWindow;
        string workingPath;

        public SaveActions(MainWindow mainWindow, coreFunctionality core, UltrasoundPart ultrasound, PhotoAcousticPart photo, string workingPath)
        {
            this.mainWindow = mainWindow;
            this.core = core;
            this.ultrasound = ultrasound;
            this.photoAcoustic = photo;
            this.workingPath = workingPath;
            metrics = new metricsCalculations();
            messages = new Messages();
        }

        public void doSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = messages.saveStudy;
            if (saveFileDialog.ShowDialog() == true)
            {
                saveAvailableData(saveFileDialog.FileName);
                this.mainWindow.loadedStudyPath = saveFileDialog.FileName;
            }
        }


        public void saveAvailableData(string path)
        {
            //save bladder
            writePointsToTXT(path, ultrasound.getBladderPoints(), SaveActions.FileType.bladderPoints);
            writeMetricsToTXT(path, ultrasound.getBladderArea(), SaveActions.FileType.Bladder2DArea);
            writeMetricsToTXT(path, ultrasound.getBladderPerimeter(), SaveActions.FileType.Bladder2DPerimeter);

            //save thickness
            writePointsToTXT(path, photoAcoustic.getThicknessPoints(), SaveActions.FileType.thicknessPoints);
            writeMetricsToTXT(path, photoAcoustic.getMeanThickness(), SaveActions.FileType.MeanThickness);

            //save geometries
            foreach (Geometry geometry in this.mainWindow.STLGeometries)
            {
                copyFileToSaveStudyFolder(path, geometry.Path, geometry.getSaveActionFileType());
            }

            //save Dicom
            if (ultrasound.ultrasoundDicomFile != null) copyFileToSaveStudyFolder(path, ultrasound.ultrasoundDicomFile, SaveActions.FileType.UltrasoundDicomFile);
            if (photoAcoustic.OXYDicomFile != null) copyFileToSaveStudyFolder(path, photoAcoustic.OXYDicomFile, SaveActions.FileType.OXYDicomFile);
            if (photoAcoustic.DeOXYDicomFile != null) copyFileToSaveStudyFolder(path, photoAcoustic.DeOXYDicomFile, SaveActions.FileType.DeOXYDicomFile);

            //save logfiles
            copyLogFilesToFolderOfStudy(path, workingPath);

            //save info
            // saveActions.writeInfoTXTFile(saveFileDialog.FileName, saveActions.collectAllStudyInfo(), SaveActions.FileType.info);
            writeInfoXMLFile(path, collectAllStudySetings(), SaveActions.FileType.settings);
        }



        public void writePointsToTXT(string path, List<List<Point>> points, FileType type) 
        {
            string filePath;
            for (int i=0; i<points.Count; i++)
            {
                filePath = getFolderName(path, type, true) + i.ToString() + ".txt";
               StreamWriter sw = new StreamWriter(filePath);
               for(int j=0; j<points[i].Count; j++)
               {
                    sw.WriteLine(points[i][j].X + " " + points[i][j].Y);
               }
                sw.Close();
            }
        }

        public void writeMetricsToTXT(string path, List<double> metrics, FileType type)
        {
            if (!metrics.Any()) return;

            string filename = getProperFileName(type);

            string filePath = getFolderName(path, type, true) + filename;
            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < metrics.Count; i++)
            {
                sw.WriteLine(messages.frame + i.ToString() + " " + metrics[i]);
            }
            sw.Close();
        }


        public void copyFileToSaveStudyFolder(string path, string sourceFileLocation, FileType type)
        {
            if (!File.Exists(sourceFileLocation)) return;

            string filename = getProperFileName(type, Path.GetExtension(sourceFileLocation));
            string sourceFileNewLocation = getFolderName(path, type, true) + filename;
            File.Copy(sourceFileLocation, sourceFileNewLocation, true);
        }


        public void copyLogFilesToFolderOfStudy(string path, string workingDirectory)
        {
            if (!Directory.Exists(workingDirectory)) return;
            string[] fileEntries = Directory.GetFiles(workingDirectory);
            foreach (string file in fileEntries)
            {
                if(Path.GetExtension(file) == ".txt")
                {
                    if(Directory.Exists(file))
                    File.Copy(file, path + Path.DirectorySeparatorChar + Path.GetFileName(file), true);
                }
            }
        }

        public void writeInfoXMLFile(string path, List<StudySetting> info, FileType type)
        {
            string filename = getProperFileName(type);
            string filePath = getFolderName(path, type, true) + getProperFileName(type);
            XmlWriter xmlWriter = XmlWriter.Create(filePath);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("settings");
            for (int i = 0; i < info.Count; i++)
            {
                xmlWriter.WriteElementString(info[i].infoName.ToString(), info[i].infoValue.ToString());
            }
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();


        }

        public List<StudySetting> collectAllStudySetings()
        {
            List<StudySetting> studyInfo = new List<StudySetting>();
            studyInfo.Add(new StudySetting() { infoName = settingType.StartingFrame, infoValue = ultrasound.startingFrame });
            studyInfo.Add(new StudySetting() { infoName = settingType.EndingFrame, infoValue = ultrasound.endingFrame });

            if (ultrasound.userPoints.Count ==2)
            {
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointX, infoValue = ultrasound.userPoints[0].X });
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointY, infoValue = ultrasound.userPoints[0].Y });
            }
            else
            {
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointX, infoValue = 0 });
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointY, infoValue = 0 });
            }
            studyInfo.Add(new StudySetting() { infoName = settingType.Repeats, infoValue = double.Parse(ultrasound.Repeats.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Smoothing, infoValue = double.Parse(ultrasound.Smoothing.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Lamda1, infoValue = double.Parse(ultrasound.Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Lamda2, infoValue = double.Parse(ultrasound.Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.LevelSize, infoValue = double.Parse(ultrasound.LevelsetSize.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Filtering, infoValue = double.Parse((ToInt(ultrasound.chechBox_FIltering.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Logger, infoValue = double.Parse((ToInt(ultrasound.chechBox_Logger.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.ClosedSurface, infoValue = double.Parse((ToInt(ultrasound.closedSurface.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.minThickness, infoValue = double.Parse(photoAcoustic.minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.maxThickness, infoValue = double.Parse(photoAcoustic.maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            return studyInfo;
        }


        public int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

    }

}

