using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public enum executionType { 
            extract2DBladder, 
            extract3DBladder, 
            extract3DLayer, 
            extract2DThickness,
            recalculate, 
            extract3DThickness,
            extractOXYDeOXY
        };

        public string getMessage(executionType type)
        {
            switch (type)
            {
                case executionType.extract2DBladder:
                    if(ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                        if (ultrasound.userPoints.Count < 2)
                        {
                            return messages.notEnoughUserPoints;
                        }
                    }
                    break;
                case executionType.extract3DBladder:
                    if (ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                        if (!ultrasound.bladder.Any())
                        {
                            return messages.noBladderSegmentation;
                        }
                    }
                    break;
                case executionType.extract3DLayer:
                    if (ultrasound.ultrasoundDicomFile == null)
                    {
                        return messages.ultrasoundFileNotLoaded;
                        if (!ultrasound.bladder.Any())
                        {
                            return messages.noBladderSegmentation;
                        }
                    }
                    break;
                case executionType.extract2DThickness:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                        if (!ultrasound.bladder.Any())
                        {
                            return messages.noBladderSegmentation;
                        }
                    }
                    break;
                case executionType.recalculate:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                        if (!photoAcoustic.areTherePoints())
                        {
                            return messages.noThicknessForUniqueFrame;
                        }
                    }
                    break;
                case executionType.extract3DThickness:
                    if (photoAcoustic.OXYDicomFile == null)
                    {
                        return messages.noOXYdicom;
                        if (!ultrasound.bladder.Any())
                        {
                            return messages.noBladderSegmentation;
                        }
                    }
                    break;
                case executionType.extractOXYDeOXY:
                    if (photoAcoustic.OXYDicomFile == null || photoAcoustic.DeOXYDicomFile == null)
                    {
                        return messages.noOXYAndDeOXYImages;
                        if (!photoAcoustic.thickness.Any())
                        {
                            return messages.noBladderSegmentation;
                        }
                        else if (photoAcoustic.thicknessGeometryPath == null || ultrasound.bladderGeometryPath == null)
                        {
                            return messages.noBadderOrThickness3DModels;
                        }
                    }
                    break;
            }
            return null;
        }



        List<int> segmentationGaps = new List<int>();
        public string checkForSegmentationGaps(List<List<Point>> points)
        {
            segmentationGaps.Clear();

            if (ultrasound.processWasExecutedAuto())
            {
                for(int i=ultrasound.startingFrame; i<=ultrasound.endingFrame; i++)
                {
                    if (!points[i].Any())
                    {
                        segmentationGaps.Add(i);
                    }
                }

            }
            else //manual segmentation
            {
                int starting = 0;
                int ending = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Any())
                    {
                        starting = i;
                        break;
                    }
                }
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    if (points[i].Any())
                    {
                        ending = i;
                        break;
                    }
                }

                for (int i = starting; i <= ending; i++)
                {
                    if (!points[i].Any())
                    {
                        segmentationGaps.Add(i);
                    }
                }
            }
         
            if (!segmentationGaps.Any())
            {
                return null;
            }
            else
            {
                return messages.makeUserAwareOfSegmentationGaps(segmentationGaps);
            }
        }

    }
}
