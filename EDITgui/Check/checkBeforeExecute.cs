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
        Context context;

        public checkBeforeExecute(Context context)
        {
            this.context = context;
        }

        public enum executionType { 
            extract2DBladder, 
            extract3DBladder, 
            extract3DLayer, 
            extract2DThickness,
            extract2DTumor,
            recalculate, 
            extract3DThickness,
            extractOXYDeOXY,
            extract3DTumor
        };

        public string getMessage(executionType type)
        {
            switch (type)
            {
                case executionType.extract2DBladder:
                    if(context.getUltrasoundPart().ultrasoundDicomFile == null)
                    {
                        return context.getMessages().ultrasoundFileNotLoaded;
                    }
                    if (context.getUltrasoundPart().userPoints.Count < 2)
                    {
                        return context.getMessages().notEnoughUserPoints;
                    }
                    break;
                case executionType.extract3DBladder:
                    if (context.getUltrasoundPart().ultrasoundDicomFile == null)
                    {
                        return context.getMessages().ultrasoundFileNotLoaded;
                    }
                    break;
                case executionType.extract3DLayer:
                    if (context.getUltrasoundPart().ultrasoundDicomFile == null)
                    {
                        return context.getMessages().ultrasoundFileNotLoaded;
                    }
                    break;
                case executionType.extract2DThickness:
                    if (context.getPhotoAcousticPart().OXYDicomFile == null)
                    {
                        return context.getMessages().noOXYdicom;
                    }
                    break;
                case executionType.extract2DTumor:
                    if (context.getPhotoAcousticPart().OXYDicomFile == null)
                    {
                        return context.getMessages().noOXYdicom;
                    }
                    break;
                case executionType.recalculate:
                    if (context.getPhotoAcousticPart().OXYDicomFile == null)
                    {
                        return context.getMessages().noOXYdicom;
                        if (!context.getPhotoAcousticPart().areTherePoints())
                        {
                            return context.getMessages().noThicknessForUniqueFrame;
                        }
                    }
                    break;
                case executionType.extract3DThickness:
                    if (context.getPhotoAcousticPart().OXYDicomFile == null)
                    {
                        return context.getMessages().noOXYdicom;
                    }
                    break;
                case executionType.extractOXYDeOXY:
                    if (context.getPhotoAcousticPart().OXYDicomFile == null || context.getPhotoAcousticPart().DeOXYDicomFile == null)
                    {
                        return context.getMessages().noOXYAndDeOXYImages;
                    }
                    if (context.getPhotoAcousticPart().thicknessGeometryPath == null || context.getUltrasoundPart().bladderGeometryPath == null)
                    {
                        return context.getMessages().noBadderOrThickness3DModels;
                    }
                    break;
                case executionType.extract3DTumor:
                    if (context.getPhotoAcousticPart().thicknessGeometryPath == null || context.getUltrasoundPart().bladderGeometryPath == null)
                    {
                        return context.getMessages().noBadderOrThickness3DModelsForTumor;
                    }
                    break;
            }
            return null;
        }

        List<int> segmentationGaps = new List<int>();
        public string checkForSegmentationGaps(List<List<Point>> points)
        {
            segmentationGaps.Clear();

            if (context.getUltrasoundPart().autoExecutionUserPointsWereSet())
            {
                for(int i= context.getUltrasoundPart().startingFrame; i<= context.getUltrasoundPart().endingFrame; i++)
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
                return context.getMessages().makeUserAwareOfSegmentationGaps(segmentationGaps);
            }
        }

    }
}
