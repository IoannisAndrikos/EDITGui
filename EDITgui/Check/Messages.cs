﻿using System;
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

        public string manage = "Manage...";

        public string area  = "Area";

        public string perimeter  = "Perimeter";

        public string meanThickness  = "Mean thickness";

        public string frame = "Frame";

        public string mm = "mm";
        
        public string mmB2 = "mm\xB2";

        public string mmB3 = "mm\xB3";

        public string volume = "Volume";

        public string surface = "Surface";

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

        public string tumors = "Tumor_Points";

        public string algorithmsConfigurations = "Configurations";

        public string bladderAreaTXT = "BladderArea.txt";

        public string bladderPerimeterTXT = "BladderPerimeter.txt";

        public string meanThicknessTXT = "MeanThickness.txt";

        public string settingsXML = "Settings.xml";

        public string algorithmsIndexes = "Indexes_of_algorithms.txt";

        public string tumorGroups = "Tumor_groups.txt";

        public string ulrasoundDCM = "ultrasound.dcm";

        public string oxyDCM = "OXY.dcm";

        public string deoxyDCM = "DeOXY.dcm";

        public string GNRDCM = "GNR.dcm";

        public string bladderSTL = "Bladder.stl";

        public string thicknessSTL = "Thickness.stl";

        public string tumorSTL = "Tumor.stl";

        public string oxyTXT = "OXY.txt";

        public string deoxyTXT = "DeOXY.txt";

        public string registrationTXT = "Registration.txt";

        public string layerSTL = "Layer.stl";
        //--------------------------------------OBJECTS 2D MESSAGES-------------------------------------------
        public const string outerwallSegmentation = "outerwall_Segmentation";

        public const string bladderSegmentation = "Bladder_Segmentation";

        public const string tumor1Segmentation = "Tumor1_Segmentation";

        public const string tumor2Segmentation = "Tumor2_Segmentation";
        //--------------------------------------OBJECTS 3D MESSAGES-------------------------------------------
        public const string outerWallGeometry = "Outer Wall";

        public const string bladderGeometry = "Bladder";

        public const string tumorGeometry = "Tumor";

        public const string layerGeometry = "Layer";

        public const string oxyGeometry = "OXY";

        public const string deoxyGeometry = "DeOXY";

        public const string GNRGeometry = "GNRs";
        //--------------------------------------NOTIFY MESSAGES-----------------------------------------------

        public string warning = "Warning";

        public const string warningConst = "Warning";

        public string thisTumorgroupAlreadyExists = "This tumor group already exists";

        public string emptyTumorGroupName = "Tumor group name cannot be empty";

        public string makeUserAwareOfRepeatProcess = "Are sure for repeating the process. Any changes that you have already performed will be lost.";

        public string getMessageAfterChangingSystemMode(MainWindow.Mode currentMode, MainWindow.Mode newMode)
        {
            return "Are sure for changing the system mode from " + currentMode.ToString() + " to " + newMode.ToString() + ". Any changes that you have already performed will be lost.";
        }


        public string specialCharacterWarning(string character)
        {
            return "The given name can not contain the character [" + character + "]";
        }

        public string changeAutoManualMode = "Are sure for changing the process mode. Any changes that you have already performed will be lost.";

        public string addTumorWithoutBladderAnnotation = "Before proceeding with the tumor annotation you should have performed the bladder annotation";

        public string addTumorWithoutThicknessAnnotation = "Before proceeding with the tumor annotation you should have performed the thickness annotation";
        
        public string loadOXYWithoutUltraasound = "Before loading the OXY image modality you should have loaded the ultrasound image modality";

        public string loadDeOXYWithoutUltrasound = "Before loading the DeOXY image modality you should have loaded the ultrasound image modality";

        public string loadGNRWithoutUltrasound = "Before loading the GNR image modality you should have loaded the ultrasound image modality";

        public string getOverwriteExistingStudyQuestion(string path)
        {
            return "Do you want to overwrite the existing study : '" + path;
        }

        public string saveBeforeExit = "Do you want to save your changes before closing the program";

        public string  alreadySpecifiedRegistrationPointForCurrentFrame = "There is an already specified registration point for the current frame. Select another frame to insert the new registration point";
      
        public string makeUserAwareOfSegmentationGaps(List<int> frames)
        {
            string message = "No segmentation points were found for the frames: [ ";
            foreach(int f in frames)
            {
                message += f.ToString() + " "; 
            }

            message += "].";

            return message;
        }

        public string notCorrectUserPass = "Username or Password is not correct.";

        public string correctDoubleFormat = "Check settings fields pressing the gear button. None of the fields should be empty, while every decimal number should be depicted with the following format: '##.###' or '##,###'.";

        //--------------------------------------ERROR DURING DATA LOADING-------------------------------------
        public string limitedSettingsFile = "The 'Settings.xml' file of the loaded study is limited.";

        //---------------------------------------ERROR CATEGORY---------------------------------------------------------------
        public string error = "Error";

        public string errorOccured = "An error has been occurred. Check the previous step of the process and repeat it.";

        public string ultrasoundFileNotLoaded = "Ultrasound File was not loaded.";


        public string cannotLoadDicom = "Cannot load the selected DICOM file.";

        public string notEnoughUserPoints = "You have to specify the initial and last frame by clicking on the ultrasound sequence.";

        public string noBladderSegmentation = "There is no bladder 2D segmentation. Accomplice the segmentation of bladder.";

        public string noOXYdicom = "No OXY DICOM file was loaded.";

        public string noThicknessForUniqueFrame = "There are no initial thickness points for the current frame.";

        public string noBladderForUniqueFrame = "There are no initial bladder points for the current frame.";

        public string noOXYAndDeOXYImages = "No OXY Or DeOXY DICOM file was loaded.";

        public string noBadderOrThickness3DModels = "Before proceeding with the extraction of the OXY and DeOXY points you have to extract both the bladder and thickness 3D model.";
        
        public string noBadderOrThickness3DModelsForTumor = "Before proceeding with the extraction of the Tumor 3D points you have to extract both the bladder and thickness 3D model.";

        //------------------------------PROBLEM TO PRODUCE THE STL-------------------------------------------------------
        public string problemToProduceSTL = "Cannot produce the STL object. There is an issue on the 2D segmnetation.";

        //--------------------------------------ERROR IN 3D VIEWER-------------------------------------
        public string noObject3DLoaded = "An minor error occured. Select again the geometry if is not rendered.";

        public string noAvailableGeometry = "There is no available geometry to apply the slicer.";

        public string visualizationOfbladderGeomteryisNeeded = "Before applying slicer you have to render the 3D geometry of the bladder.";

        public string no3DObjectsWereFound = "The selected folder does not contain any study 3D geometries.";

        public string noCorrectStudyFolder = "The selected folder is not a study folder.";
    }
}
