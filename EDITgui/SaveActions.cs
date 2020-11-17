using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

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


        public void copyFileToFolderOfStudy(string path, string sourceFileLocation, FileType type)
        {
            if (!File.Exists(sourceFileLocation)) return;

            string filename = getProperFileName(type, Path.GetExtension(sourceFileLocation));
            string sourceFileNewLocation = getFolderName(path, type, true) + filename;
            File.Copy(sourceFileLocation, sourceFileNewLocation, true);
        }


        public void copyLogFilesToFolderOfStudy(string path, string workingDirectory)
        {
            if (!Directory.Exists(workingDirectory)) return;
            Console.WriteLine(workingDirectory + Path.DirectorySeparatorChar);
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

        public void writeInfoTXTFile(string path, List<StudyInfo> info, FileType type)
        {
            string filename = getProperFileName(type);

            string filePath = getFolderName(path, type, true) + filename;
            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < info.Count; i++)
            {
                sw.WriteLine(info[i].infoName.ToString() + " " + info[i].infoValue);
            }
            sw.Close();
        }


        public List<StudyInfo> collectAllStudyInfo()
        {
            List<StudyInfo> studyInfo = new List<StudyInfo>();
            studyInfo.Add(new StudyInfo() { infoName = infoType.StartingFrame, infoValue = ultrasound.startingFrame });
            studyInfo.Add(new StudyInfo() { infoName = infoType.EndingFrame, infoValue = ultrasound.endingFrame });

            if (ultrasound.userPoints.Count ==2)
            {
                studyInfo.Add(new StudyInfo() { infoName = infoType.ClickPointX, infoValue = ultrasound.userPoints[0].X });
                studyInfo.Add(new StudyInfo() { infoName = infoType.ClickPointY, infoValue = ultrasound.userPoints[0].Y });
            }
            else
            {
                studyInfo.Add(new StudyInfo() { infoName = infoType.ClickPointX, infoValue = 0 });
                studyInfo.Add(new StudyInfo() { infoName = infoType.ClickPointY, infoValue = 0 });
            }
            studyInfo.Add(new StudyInfo() { infoName = infoType.Repeats, infoValue = double.Parse(ultrasound.Repeats.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.Smoothing, infoValue = double.Parse(ultrasound.Smoothing.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.Lamda1, infoValue = double.Parse(ultrasound.Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.Lamda2, infoValue = double.Parse(ultrasound.Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.LevelSize, infoValue = double.Parse(ultrasound.LevelsetSize.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.Filtering, infoValue = double.Parse((ToInt(ultrasound.chechBox_FIltering.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.Logger, infoValue = double.Parse((ToInt(ultrasound.chechBox_Logger.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.ClosedSurface, infoValue = double.Parse((ToInt(ultrasound.closedSurface.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.minThickness, infoValue = double.Parse(photoAcoustic.minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudyInfo() { infoName = infoType.maxThickness, infoValue = double.Parse(photoAcoustic.maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });

            return studyInfo;
        }


        public int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

    }

}

