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
        public string selectDicom = "Select Dicom File";

        public string saveStudy  = "Save Current Study";

        public string area  = "Area";

        public string perimeter  = "Perimeter";

        public string meanThickness  = "Mean thickness";

        public string frame = "Frame";

        public string mm = "mm";
        
        public string mmB2 = "mm\xB2";

        public string mmB3 = "mm\xB3";

        public string startingFrame = "Starting_Frame";

        public string endingFrame = "Ending_Frame";

       














        //---------------------------------------ERROR CATEGORY---------------------------------------------------------------
        public string errorOccured = "An error  has occurred. Check the previous step of the process";
        
        public string cannotLoadDicom = "Cannot load the selected DICOM file";

        public string notEnoughUserPoints = "You have to specify the initial and last frame by clicking on the ultrasound sequence";

        public string noBladderSegmentation = "There is no bladder 2D segmentation. Accomplice the segmentation of bladder";

        public string noOXYdicom = "No OXY DICOM file was loaded";

        public string noThicknessForUniqueFrame = "There are no initial thickness points for the current frame";

        public string noOXYAndDeOXYImages = "No OXY Or DeOXY DICOM file was loaded";

        public string noBadderOrThickness3DModels = "Before proceeding with the extraction of the OXY and DeOXY points you have to extract both the bladder and thickness 3D model";

        //------------------------------PROBLEM TO PRODUCE THE STL-------------------------------------------------------
        public string problemToProduceSTL = "Cannot produce the STL object. There is an issue on the 2D segmnetation";

    }
}
