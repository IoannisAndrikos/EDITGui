using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    public class Messages
    {
        public string ultrasound = "ultrasound";

        public string oxy = "OXY";

        public string deoxy = "DeOXY";

        public string selectDicom = "Select Dicom File";

        public string saveStudy  = "Save Current Study";

        public string area  = "Area";

        public string perimeter  = "Perimeter";

        public string meanThickness  = "Mean thickness";

        public string frame = "Frame";

        public string mm = "mm";
        
        public string mmB2 = "mm\xB2";

        public string mmB3 = "mm\xB3";


        //---------------------------------------SETTINGS FILE VARIABLES--------------------------------------
        public string startingFrame = "StartingFrame";

        public string endingFrame = "EndingFrame";

        public string clickPointX = "clickPointX";

        public string clickPointY = "clickPointY";

        public string repeats = "Repeats";

        public string smoothing = "Smoothing";

        public string lamda1 = "Lamda1";

        public string lamda2 = "Lamda2";

        public string levelSize = "LevelSize";

        public string filtering = "Filtering";

        public string logger = "Logger";

        public string closesSurface = "closesSurface";

        public string minThickness = "minThickness";

        public string maxThickness = "maxThickness";

        //--------------------------------------SAVE STUDY NAMES-----------------------------------------

        public string metrics = "Metrics";

        public string points2D = "Points_2D";

        public string objects3D = "Objects_3D";

        public string dicomFiles = "Dicom_files";

        public string bladderPoints = "Bladder_Points";

        public string thicknessPoints = "Thickness_Points";

        public string bladderAreaTXT = "BladderArea.txt";

        public string bladderPerimeterTXT = "BladderPerimeter.txt";

        public string meanThicknessTXT = "MeanThickness.txt";

        public string settingsXML = "Settings.xml";

        public string ulrasoundDCM = "ultrasound.dcm";

        public string oxyDCM = "OXY.dcm";

        public string deoxyDCM = "DeOXY.dcm";

        public string bladderSTL = "Bladder.stl";

        public string thicknessSTL = "Thickness.stl";

        public string oxyTXT = "OXY.txt";

        public string deoxyTXT = "DeOXY.txt";

        public string layerSTL = "Layer.stl";
        //--------------------------------------NOTIFY MESSAGES-----------------------------------------------

        public string warning = "Warning";

        public string makeUserAwareOfRepeatProcess = "Are sure for repeating the process. Any changes that you have already performed will be lost";

        public string getOverwriteExistingStudyQuestion(string path)
        {
            return "The current study will overwrite the existing study : '" + path + "'. Do you agree?";
        }

        //--------------------------------------ERROR DURING DATA LOADING-------------------------------------
        public string limitedSettingsFile = "The Settings.xml file of the loaded study is limited";

        //---------------------------------------ERROR CATEGORY---------------------------------------------------------------
        public string error = "Error";

        public string errorOccured = "An error  has occurred. Check the previous step of the process";

        public string ultrasoundFileNotLoaded = "Ultrasound File was not loaded";


        public string cannotLoadDicom = "Cannot load the selected DICOM file";

        public string notEnoughUserPoints = "You have to specify the initial and last frame by clicking on the ultrasound sequence";

        public string noBladderSegmentation = "There is no bladder 2D segmentation. Accomplice the segmentation of bladder";

        public string noOXYdicom = "No OXY DICOM file was loaded";

        public string noThicknessForUniqueFrame = "There are no initial thickness points for the current frame";

        public string noOXYAndDeOXYImages = "No OXY Or DeOXY DICOM file was loaded";

        public string noBadderOrThickness3DModels = "Before proceeding with the extraction of the OXY and DeOXY points you have to extract both the bladder and thickness 3D model";

        //------------------------------PROBLEM TO PRODUCE THE STL-------------------------------------------------------
        public string problemToProduceSTL = "Cannot produce the STL object. There is an issue on the 2D segmnetation";

        //--------------------------------------ERROR IN 3D VIEWER-------------------------------------
        public string noObject3DLoaded = "An minor error occured. Select again the geometry if is not rendered";
    }
}
