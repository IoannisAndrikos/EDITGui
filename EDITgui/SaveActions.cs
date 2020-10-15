using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace EDITgui
{
    public class SaveActions
    {
        Messages messages = new Messages();

        public enum FileType { bladderPoints, thicknessPoints, info, Bladder3D, Thickness3D, OXY3D, DeOXY3D, Layer3D,
            UltrasoundDicomFile, OXYDicomFile, DeOXYDicomFile, Bladder2DArea, Bladder2DPerimeter, MeanThickness
        }

        public void writePointsToTXT(string path, List<List<Point>> points, FileType type) 
        {
            string filePath;
            for (int i=0; i<points.Count; i++)
            {
                filePath = getFolderName(path, type) + i.ToString() + ".txt";
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

            string filePath = getFolderName(path, type) + filename;
            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < metrics.Count; i++)
            {
                sw.WriteLine(messages.frame + "-->" + i.ToString() + ": " + metrics[i]);
            }
            sw.Close();
        }


        public void copyFileToFolderOfStudy(string path, string sourceFileLocation, FileType type)
        {
            if (!File.Exists(sourceFileLocation)) return;

            string filename = getProperFileName(type, Path.GetExtension(sourceFileLocation));
            string sourceFileNewLocation = getFolderName(path, type) + filename;
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

            string filePath = getFolderName(path, type) + filename;
            StreamWriter sw = new StreamWriter(filePath);
            for (int i = 0; i < info.Count; i++)
            {
                sw.WriteLine(info[i].infoName.ToString() + ": " + info[i].infoValue.ToString());
            }
            sw.Close();

        }


        private string getFolderName(string path, FileType type) {
            string dir = path;
            
            switch (type)
            {
                case FileType.bladderPoints:
                    dir =  path + Path.DirectorySeparatorChar + "Points_2D" + Path.DirectorySeparatorChar + "Bladder_Points" + Path.DirectorySeparatorChar;
                    break;
                case FileType.thicknessPoints:
                    dir = path + Path.DirectorySeparatorChar + "Points_2D" + Path.DirectorySeparatorChar + "Thickness_Points" + Path.DirectorySeparatorChar;
                    break; 
                case FileType.info:
                    dir = path + Path.DirectorySeparatorChar;
                    break;
                case FileType.Bladder3D:
                case FileType.Thickness3D:
                case FileType.OXY3D:
                case FileType.Layer3D:
                case FileType.DeOXY3D:
                    dir = path + Path.DirectorySeparatorChar + "Objects_3D" + Path.DirectorySeparatorChar;
                    break;
                case FileType.UltrasoundDicomFile:
                case FileType.OXYDicomFile:
                case FileType.DeOXYDicomFile:
                    dir = path + Path.DirectorySeparatorChar + "Dicom_files" + Path.DirectorySeparatorChar;
                    break;
                case FileType.Bladder2DArea:
                case FileType.Bladder2DPerimeter:
                case FileType.MeanThickness:
                    dir = path + Path.DirectorySeparatorChar + "Metrics" + Path.DirectorySeparatorChar;
                    break;
            }

            if (Directory.Exists(dir))
            {
                return dir;
            }
            else
            {
                Directory.CreateDirectory(dir);
                return dir;
            }

        }


        private string getProperFileName(FileType type, string sourceFileName = null)
        {
            string filename = null;
            switch (type)
            {
                case FileType.Bladder2DArea:
                    filename = "BladderArea.txt";
                    break;
                case FileType.Bladder2DPerimeter:
                    filename = "BladderPerimeter.txt";
                    break;
                case FileType.MeanThickness:
                    filename = "MeanThickness.txt";
                    break;
                case FileType.info:
                    filename = "Info.txt";
                    break;
                case FileType.UltrasoundDicomFile:
                    filename = "ultrasound" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.OXYDicomFile:
                    filename = "OXY" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.DeOXYDicomFile:
                    filename = "DeOXY" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.Bladder3D:
                    filename = "Bladder" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.Thickness3D:
                    filename = "Thickness" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.OXY3D:
                    filename = "OXY" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.DeOXY3D:
                    filename = "DeOXY" + Path.GetExtension(sourceFileName);
                    break;
                case FileType.Layer3D:
                    filename = "Layer" + Path.GetExtension(sourceFileName);
                    break;
            }
            return filename;
        }


    }

}

