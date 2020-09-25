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
        public string cannotLoadDicom = "Cannot load the selected DICOM file";

        public string notEnoughUserPoints = "You have to specify the initial and last frame by clicking on the ultrasound sequence";

        public string noBladderSegmentation = "There is no bladder 2D segmentation. Accomplice the segmentation of bladder";


        public string noThicknessForUniqueFrame = "There are no initial thickness points for the current frame";

        //------------------------------PROBLEM TO PRODUCE THE STL-------------------------------------------------------
        public string problemToProduceSTL = "Cannot produce the STL object. There is an issue on the 2D segmnetation";

    }
}
