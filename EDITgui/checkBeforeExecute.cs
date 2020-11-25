using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDITgui
{
    public class checkBeforeExecute
    {
        Messages messages;
        UltrasoundPart ultrasound;
        PhotoAcousticPart photoAcoustic;
        MainWindow mainWindow;

        public checkBeforeExecute(MainWindow mainWindow, UltrasoundPart ultrasound, PhotoAcousticPart photoAcoustic)
        {
            this.mainWindow = mainWindow;
            this.ultrasound = ultrasound;
            this.photoAcoustic = photoAcoustic;
            messages = new Messages();
        }

        public enum executionType { extract2DBladder, extract3DBladder, extract3DLayer ,extract2DThickness, recalculate, extract3DThickness, extractOXYDeOXY };

        public string getMessage(executionType type)
        {
            switch (type)
            {
                case executionType.extract2DBladder:
                    if(ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                    }
                    if(ultrasound.userPoints.Count < 2)
                    {
                        return messages.notEnoughUserPoints;
                    }
                    break;
                case executionType.extract3DBladder:
                    if (ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                    }
                    if (!ultrasound.bladder.Any())
                    {
                        return messages.noBladderSegmentation;
                    }
                    break;
                case executionType.extract3DLayer:
                    if (ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                    }
                    if (!ultrasound.bladder.Any())
                    {
                        return messages.noBladderSegmentation;
                    }
                    break;
                case executionType.extract2DThickness:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                    }
                    if (!ultrasound.bladder.Any())
                    {
                        return messages.noBladderSegmentation;

                    }
                    break;
                case executionType.recalculate:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                    }
                    if (!photoAcoustic.areTherePoints())
                    {
                        return messages.noThicknessForUniqueFrame;
                    }
                    break;
                case executionType.extract3DThickness:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                    }
                    if (!ultrasound.bladder.Any())
                    {
                        return messages.noBladderSegmentation;
                    }
                    break;
                case executionType.extractOXYDeOXY:
                    if (!photoAcoustic.thickness.Any())
                    {
                        return messages.noBladderSegmentation;
                    }
                    if (photoAcoustic.OXYDicomFile == null || photoAcoustic.DeOXYDicomFile == null)
                    {
                        return messages.noOXYAndDeOXYImages;
                    }
                    if (photoAcoustic.thicknessGeometryPath == null || ultrasound.bladderGeometryPath == null)
                    {
                        return messages.noBadderOrThickness3DModels;
                    }
                    break;
            }
            return null;
        }

    }
}
