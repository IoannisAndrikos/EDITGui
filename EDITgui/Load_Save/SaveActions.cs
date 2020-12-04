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
        Context context;

        public SaveActions(Context context)
        {
            this.context = context;
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
                context.getMainWindow().loadedStudyPath = saveFileDialog.FileName;
            }
        }


        public void saveAvailableData(string path)
        {
            //save bladder
            writePointsToTXT(path, context.getUltrasoundPart().getBladderPoints(), SaveActions.FileType.bladderPoints);
            writeMetricsToTXT(path, context.getUltrasoundPart().getBladderArea(), SaveActions.FileType.Bladder2DArea);
            writeMetricsToTXT(path, context.getUltrasoundPart().getBladderPerimeter(), SaveActions.FileType.Bladder2DPerimeter);

            //save thickness
            writePointsToTXT(path, context.getPhotoAcousticPart().getThicknessPoints(), SaveActions.FileType.thicknessPoints);
            writeMetricsToTXT(path, context.getPhotoAcousticPart().getMeanThickness(), SaveActions.FileType.MeanThickness);

            //save geometries
            foreach (Geometry geometry in context.getMainWindow().STLGeometries)
            {
                copyFileToSaveStudyFolder(path, geometry.Path, geometry.getSaveActionFileType());
            }

            //save Dicom
            if (context.getUltrasoundPart().ultrasoundDicomFile != null) copyFileToSaveStudyFolder(path, context.getUltrasoundPart().ultrasoundDicomFile, SaveActions.FileType.UltrasoundDicomFile);
            if (context.getPhotoAcousticPart().OXYDicomFile != null) copyFileToSaveStudyFolder(path, context.getPhotoAcousticPart().OXYDicomFile, SaveActions.FileType.OXYDicomFile);
            if (context.getPhotoAcousticPart().DeOXYDicomFile != null) copyFileToSaveStudyFolder(path, context.getPhotoAcousticPart().DeOXYDicomFile, SaveActions.FileType.DeOXYDicomFile);

            //save logfiles
            copyLogFilesToFolderOfStudy(path, context.getStudyFile().getWorkspace());

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
            studyInfo.Add(new StudySetting() { infoName = settingType.StartingFrame, infoValue = context.getUltrasoundPart().startingFrame });
            studyInfo.Add(new StudySetting() { infoName = settingType.EndingFrame, infoValue = context.getUltrasoundPart().endingFrame });

            if (context.getUltrasoundPart().userPoints.Count ==2)
            {
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointX, infoValue = context.getUltrasoundPart().userPoints[0].X });
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointY, infoValue = context.getUltrasoundPart().userPoints[0].Y });
            }
            else
            {
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointX, infoValue = 0 });
                studyInfo.Add(new StudySetting() { infoName = settingType.ClickPointY, infoValue = 0 });
            }
            studyInfo.Add(new StudySetting() { infoName = settingType.Repeats, infoValue = double.Parse(context.getUltrasoundPart().Repeats.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Smoothing, infoValue = double.Parse(context.getUltrasoundPart().Smoothing.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Lamda1, infoValue = double.Parse(context.getUltrasoundPart().Lamda1.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Lamda2, infoValue = double.Parse(context.getUltrasoundPart().Lamda2.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.LevelSize, infoValue = double.Parse(context.getUltrasoundPart().LevelsetSize.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Filtering, infoValue = double.Parse((ToInt(context.getUltrasoundPart().chechBox_FIltering.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.Logger, infoValue = double.Parse((ToInt(context.getUltrasoundPart().chechBox_Logger.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.ClosedSurface, infoValue = double.Parse((ToInt(context.getUltrasoundPart().closedSurface.IsChecked.Value)).ToString()) });
            studyInfo.Add(new StudySetting() { infoName = settingType.minThickness, infoValue = double.Parse(context.getPhotoAcousticPart().minThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            studyInfo.Add(new StudySetting() { infoName = settingType.maxThickness, infoValue = double.Parse(context.getPhotoAcousticPart().maxThickness.Text.Replace(",", "."), CultureInfo.InvariantCulture) });
            return studyInfo;
        }


        public int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

    }

}

