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
        private enum savedOrNot { SAVED, NOTSAVED }
        private savedOrNot status;

        public SaveActions(Context context)
        {
            this.context = context;
            status = savedOrNot.SAVED;
        }

        public void saved()
        {
            this.status = savedOrNot.SAVED;
        }

        public void dataUpdatedWithoutSave()
        {
            this.status = savedOrNot.NOTSAVED;
        }

        public bool checkIfStudyWasSaved()
        {
            if (status == savedOrNot.SAVED)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            }
        }


        public void saveAvailableData(string path)
        {
            //save bladder
            writeBladderData(path);

            //save thickness
            writeThicknessData(path);
            writeMetricsToTXT(path, context.getImages().getTotalMeanThickness(), FileType.MeanThickness);

            writeTumorData(path);

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

            //save Algorithm frames settings
            writeAlgorithmFrameSettingsTXT(path);

            //update status
            context.getLoadActions().setLoadedStudyPath(path);
            saved();
        }



        public void writeBladderData(string path) 
        {
            List<List<Point>> bladder = context.getImages().getTotalBladderPoints();
            string filePath;
            for (int i=0; i< bladder.Count; i++)
            {
                filePath = getFolderName(path, SaveActions.FileType.bladderPoints, true) + i.ToString() + ".txt";
               StreamWriter sw = new StreamWriter(filePath);
               for(int j=0; j< bladder[i].Count; j++)
               {
                    sw.WriteLine(bladder[i][j].X + " " + bladder[i][j].Y);
               }
                sw.Close();
            }
            bladder.Clear();
        }


        public void writeThicknessData(string path)
        {
            List<List<Point>> thickness = context.getImages().getTotalThicknessPoints();

            string filePath;
            for (int i = 0; i < thickness.Count; i++)
            {
                filePath = getFolderName(path, FileType.thicknessPoints, true) + i.ToString() + ".txt";
                StreamWriter sw = new StreamWriter(filePath);
                for (int j = 0; j < thickness[i].Count; j++)
                {
                    sw.WriteLine(thickness[i][j].X + " " + thickness[i][j].Y);
                }
                sw.Close();
            }
            thickness.Clear();
        }

        public void writeTumorData(string path)
        {
            string filePath;
            List<List<List<Point>>> tumors = context.getImages().getTotalTumorPoints();
            StreamWriter sw;
            for (int i = 0; i <tumors.Count; i++)
            {
                filePath = getFolderName(path, SaveActions.FileType.Tumors2D, true) + i.ToString() + ".txt";
                sw = new StreamWriter(filePath);
                for (int j = 0; j < tumors[i].Count; j++)
                {
                    sw.WriteLine("tumor: " + j.ToString() );
                    for (int k = 0; k < tumors[i][j].Count; k++)
                    {
                        sw.WriteLine(tumors[i][j][k].X + " " + tumors[i][j][k].Y);
                    }
                }
                sw.Close();
            }
            tumors.Clear();
        }


        public void writeMetricsToTXT(string path, List<double> metrics, FileType type)
        {
            if (!metrics.Any()) return;

            string filename = getProperFileName(FileType.MeanThickness);

            string filePath = getFolderName(path, FileType.MeanThickness, true) + filename;
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


        //public void writeRegistrationPointsTXT(string path)
        //{
        //    string filePath = getFolderName(path, SaveActions.FileType.Registration, true) + getProperFileName(FileType.Registration);
        //    List<string> registrationPointsTextList = context.getRegistration().getRegistrationPointsTextList();
            
        //    StreamWriter sw = new StreamWriter(filePath);
        //    foreach (string registrationPointString in registrationPointsTextList)
        //    {
        //        sw.WriteLine(registrationPointString.Replace(",", "."));
        //    }
        //    sw.Close();
        //}



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


        public void writeAlgorithmFrameSettingsTXT(string path)
        {
            context.getUltrasoundPart().saveFrameAlgorithmSetting();

            string filename = getProperFileName(FileType.algorithmFrameSettings);

            string filePath = getFolderName(path, FileType.algorithmFrameSettings, true) + filename;
            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < context.getUltrasoundPart().fileCount -1 ; i++)
            {
                sw.WriteLine(messages.frame + ": " +  i.ToString() + " " + 
                     context.getImages().getFrameSettings(i).repeats.ToString() +
                    " " + context.getImages().getFrameSettings(i).smoothing.ToString() +
                    " " + context.getImages().getFrameSettings(i).lamda1.ToString() + 
                    " " + context.getImages().getFrameSettings(i).lamda2.ToString() + 
                    " " + context.getImages().getFrameSettings(i).levelSize.ToString() +
                    " " + (Convert.ToInt32(context.getImages().getFrameSettings(i).filtering)).ToString() +
                    " " + (Convert.ToInt32(context.getImages().getFrameSettings(i).probeArtifactCorrection)).ToString() +
                    " " + context.getImages().getFrameSettings(i).maxThickness.ToString() +
                    " " + context.getImages().getFrameSettings(i).minThickness.ToString() + 
                    " " + (Convert.ToInt32(context.getImages().getFrameSettings(i).majorThicknessExistence)).ToString() 
                    );
            }
            sw.Close();
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
           
            return studyInfo;
        }


        public int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

    }

}

