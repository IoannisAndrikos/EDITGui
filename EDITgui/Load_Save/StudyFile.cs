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

        string workspaceDirName = "Edit_Workspace";

        //Be careful! This folder names are produced form the backend code!
        //if you change them here remember to change them at the backend, as well!
        string workspace3DObjectsDirName = "stl_objects"; 

        string workspaceDicomFilesDirName = "dicom_files";

        public enum FileType
        {
            bladderPoints, thicknessPoints, settings, algorithmFrameSettings ,Bladder3D, Thickness3D, OXY3D, DeOXY3D, Layer3D, Registration,
            UltrasoundDicomFile, OXYDicomFile, DeOXYDicomFile, GNRDicomFile, MeanThickness, Tumors2D, Tumors3D
        }

        public string getWorkspace()
        {
            return Path.GetTempPath() + workspaceDirName;
        }

        public string getWorkingObjetcts3DPath()
        {
            return getWorkspace() + Path.DirectorySeparatorChar + workspace3DObjectsDirName + Path.DirectorySeparatorChar;
        }

        public string getWorkspaceDicomPath()
        {
            string dicomFilesDir = getWorkspace() + Path.DirectorySeparatorChar + workspaceDicomFilesDirName;
            if (!Directory.Exists(dicomFilesDir))
            {
                Directory.CreateDirectory(dicomFilesDir);
            }
            return dicomFilesDir + Path.DirectorySeparatorChar;
        }


        public string getProperFileName(FileType type, string sourceFileName = null)
        {
            string filename = null;
            switch (type)
            {
                case FileType.MeanThickness:
                    filename = messages.meanThicknessTXT;
                    break;
                case FileType.settings:
                    filename = messages.settingsXML;
                    break;
                case FileType.algorithmFrameSettings:
                    filename = messages.algorithmsIndexes;
                    break;
                case FileType.UltrasoundDicomFile:
                    filename = messages.ulrasoundDCM;
                    break;
                case FileType.OXYDicomFile:
                    filename = messages.oxyDCM;
                    break;
                case FileType.DeOXYDicomFile:
                    filename = messages.deoxyDCM;
                    break;
                case FileType.GNRDicomFile:
                    filename = messages.GNRDCM;
                    break;
                case FileType.Bladder3D:
                    filename = messages.bladderSTL;
                    break;
                case FileType.Thickness3D:
                    filename = messages.thicknessSTL;
                    break;
                case FileType.OXY3D:
                    filename = messages.oxyTXT;
                    break;
                case FileType.Tumors3D:
                    filename = messages.tumorSTL;
                    break;
                case FileType.Registration:
                    filename = messages.registrationTXT;
                    break;
                case FileType.DeOXY3D:
                    filename = messages.deoxyTXT;
                    break;
                case FileType.Layer3D:
                    filename = messages.layerSTL;
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
                    dir = path + Path.DirectorySeparatorChar + messages.points2D + Path.DirectorySeparatorChar + messages.bladderPoints + Path.DirectorySeparatorChar;
                    break;
                case FileType.thicknessPoints:
                    dir = path + Path.DirectorySeparatorChar + messages.points2D + Path.DirectorySeparatorChar + messages.thicknessPoints + Path.DirectorySeparatorChar;
                    break;
                case FileType.Tumors2D:
                    dir = path + Path.DirectorySeparatorChar + messages.points2D + Path.DirectorySeparatorChar + messages.tumors + Path.DirectorySeparatorChar;
                    break;
                case FileType.settings:
                    dir = path + Path.DirectorySeparatorChar;
                    break;
                case FileType.algorithmFrameSettings:
                    dir = path + Path.DirectorySeparatorChar + messages.algorithmsConfigurations + Path.DirectorySeparatorChar;
                    break;
                case FileType.Bladder3D:
                case FileType.Thickness3D:
                case FileType.OXY3D:
                case FileType.Layer3D:
                case FileType.DeOXY3D:
                case FileType.Tumors3D:
                case FileType.Registration:
                    dir = path + Path.DirectorySeparatorChar + messages.objects3D + Path.DirectorySeparatorChar;
                    break;
                case FileType.UltrasoundDicomFile:
                case FileType.OXYDicomFile:
                case FileType.DeOXYDicomFile:
                case FileType.GNRDicomFile:
                    dir = path + Path.DirectorySeparatorChar + messages.dicomFiles + Path.DirectorySeparatorChar;
                    break;
                case FileType.MeanThickness:
                    dir = path + Path.DirectorySeparatorChar + messages.metrics + Path.DirectorySeparatorChar;
                    break;
            }
            if(!Directory.Exists(dir) && create)
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }


        public string copyFileToWorkspace(string path, string sourceFileLocation, FileType type)
        {
            if (!File.Exists(sourceFileLocation)) return null;

            string sourceFileNewLocation = path + getProperFileName(type);
            File.Copy(sourceFileLocation, sourceFileNewLocation, true);

            return sourceFileNewLocation;
        }



        public enum settingType
        {
            StartingFrame, EndingFrame, StartingClickPointX, StartingClickPointY, EndingClickPointX, EndingClickPointY, Repeats, Smoothing, Lamda1,
            Lamda2, LevelSize, Filtering, Logger, ClosedSurface, minThickness, maxThickness
        };

        public class StudySetting
        {
            public settingType infoName { get; set; }
            public double infoValue { get; set; }
        }
    }
}
