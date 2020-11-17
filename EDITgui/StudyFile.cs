using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDITgui
{
   public class StudyFile
   {
        public Messages messages = new Messages();

        string workingDir = "Edit_Current_working_study";

        string workingObjects3DFolderName = "stl_objects";

        public enum FileType
        {
            bladderPoints, thicknessPoints, info, Bladder3D, Thickness3D, OXY3D, DeOXY3D, Layer3D,
            UltrasoundDicomFile, OXYDicomFile, DeOXYDicomFile, Bladder2DArea, Bladder2DPerimeter, MeanThickness
        }


        public string getWorkingPath()
        {
            return Path.GetTempPath() + workingDir;
        }

        public string getWorkingObjetcts3DPath()
        {
            return getWorkingPath() + Path.DirectorySeparatorChar + workingObjects3DFolderName + Path.DirectorySeparatorChar;
        }


        public string getProperFileName(FileType type, string sourceFileName = null)
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
                    filename = "ultrasound.dcm"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.OXYDicomFile:
                    filename = "OXY.dcm"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.DeOXYDicomFile:
                    filename = "DeOXY.dcm"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.Bladder3D:
                    filename = "Bladder.stl"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.Thickness3D:
                    filename = "Thickness.stl"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.OXY3D:
                    filename = "OXY.txt"; //+ Path.GetExtension(sourceFileName);
                    break;
                case FileType.DeOXY3D:
                    filename = "DeOXY.txt";// + Path.GetExtension(sourceFileName);
                    break;
                case FileType.Layer3D:
                    filename = "Layer.stl"; //+ Path.GetExtension(sourceFileName);
                    break;
            }
            return filename;
        }



        public string getFolderName(string path, FileType type, bool create = false)
        {
            string dir = path;

            switch (type)
            {
                case FileType.bladderPoints:
                    dir = path + Path.DirectorySeparatorChar + "Points_2D" + Path.DirectorySeparatorChar + "Bladder_Points" + Path.DirectorySeparatorChar;
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
            if(!Directory.Exists(dir) && create)
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public enum infoType
        {
            StartingFrame, EndingFrame, ClickPointX, ClickPointY, Repeats, Smoothing, Lamda1,
            Lamda2, LevelSize, Filtering, Logger, ClosedSurface, minThickness, maxThickness
        };

        public class StudyInfo
        {
            public infoType infoName { get; set; }
            public double infoValue { get; set; }
        }
    }
}
