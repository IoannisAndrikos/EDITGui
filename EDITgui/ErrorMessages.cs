using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDITgui
{
    class ErrorMessages
    {
        public string notEnoughUserPoints = "You have to specify the initial and last frame by clicking on the ultrasound sequence";

        public string noBladderSegmentation = "There is no bladder 2D segmentation. Accomplice the 2D segmentation step berore the extraction of the 3D model";


        //------------------------------PROBLEM TO PRODUCE THE STL-------------------------------------------------------
        public string problemToProduceSTL = "Cannot produce the STL object. There is an issue on the 2D segmnetation";

    }
}
