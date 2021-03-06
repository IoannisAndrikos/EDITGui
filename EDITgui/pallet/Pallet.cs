﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EDITgui
{
    public class Pallet
    {
        Context context;
        PalletWindow palletWindow;

        public Pallet(Context context)
        {
            this.context = context;
        }

        public void CreatePalletWindow()
        {
            if (palletWindow == null)
            {
                this.palletWindow = new PalletWindow(this);
            }
        }

        public double pointSize = 2.5;

        public static SolidColorBrush cyan = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FFFF"));
        public static SolidColorBrush yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3FF00"));
        public static SolidColorBrush orange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFB700"));
        public static SolidColorBrush red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF3C3C"));
        public static SolidColorBrush magenta = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEB00FF"));
        public static SolidColorBrush green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF3A"));
        public static SolidColorBrush blue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008CFF"));
        public static SolidColorBrush purple = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA200FF"));
        public static SolidColorBrush pink = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF07D4"));
        public static SolidColorBrush silver = System.Windows.Media.Brushes.Silver;
        //public  SolidColorBrush magenta = System.Windows.Media.Brushes.Magenta;
        public static SolidColorBrush greenMarker = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
        public static SolidColorBrush redMarker = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));
        public static SolidColorBrush selectSlicerImageGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4FA6A6"));

        //-----------COLORS 3D-----------------
        public double[] bladder3DColor = { 1, 1, 1 };
        public double bladder3DOpacity = 1;

        public double[] outerWall3DColor = { 1, 1, 0 };
        public double outerWall3DOpacity = 0.8;

        public double[] layer3DColor = { 0, 1, 0 };
        public double layer3DOpacity = 0.3;

        public double[] OXY3DColor = { 1, 0, 0 };
        public double OXY3DOpacity = 0.6;

        public double[] DeOXY3DColor = { 0, 0, 1 };
        public double DeOXY3DOpacity = 0.6;

        public double[] GNR3DColor = { 0, 1, 0 };
        public double GNR3DOpacity = 0.6;

        public double[] tumor3DColor = { 1, 0, 1 };
        public double tumor3DOpacity = 0.6;


        public void updatePalletColorsAndOpacities(List<Object3DViewAspects> viewAspects)
        {
            //3D
            bladder3DColor = viewAspects.Find(x => x.objectName == Messages.bladderGeometry).colors;
            bladder3DOpacity = viewAspects.Find(x => x.objectName == Messages.bladderGeometry).opacity;

            outerWall3DColor = viewAspects.Find(x => x.objectName == Messages.outerWallGeometry).colors;
            outerWall3DOpacity = viewAspects.Find(x => x.objectName == Messages.outerWallGeometry).opacity;

            layer3DColor = viewAspects.Find(x => x.objectName == Messages.layerGeometry).colors;
            layer3DOpacity = viewAspects.Find(x => x.objectName == Messages.layerGeometry).opacity;

            OXY3DColor = viewAspects.Find(x => x.objectName == Messages.oxyGeometry).colors;
            OXY3DOpacity = viewAspects.Find(x => x.objectName == Messages.oxyGeometry).opacity;

            DeOXY3DColor = viewAspects.Find(x => x.objectName == Messages.deoxyGeometry).colors;
            DeOXY3DOpacity = viewAspects.Find(x => x.objectName == Messages.deoxyGeometry).opacity;

            GNR3DColor = viewAspects.Find(x => x.objectName == Messages.GNRGeometry).colors;
            GNR3DOpacity = viewAspects.Find(x => x.objectName == Messages.GNRGeometry).opacity;

            tumor3DColor = viewAspects.Find(x => x.objectName == Messages.tumorGeometry).colors;
            tumor3DOpacity = viewAspects.Find(x => x.objectName == Messages.tumorGeometry).opacity;
        }

        public List<Object3DViewAspects> getPalletColorsAndOpacities()
        {
            List<Object3DViewAspects> object3DViewAspects = new List<Object3DViewAspects>();
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.bladderGeometry, colors = bladder3DColor, opacity = bladder3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.outerWallGeometry, colors = outerWall3DColor, opacity = outerWall3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.layerGeometry, colors = layer3DColor, opacity = layer3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.oxyGeometry, colors = OXY3DColor, opacity = OXY3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.deoxyGeometry, colors = DeOXY3DColor, opacity = DeOXY3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.GNRGeometry, colors = GNR3DColor, opacity = GNR3DOpacity });
            object3DViewAspects.Add(new Object3DViewAspects() {objectName = Messages.tumorGeometry, colors = tumor3DColor, opacity = tumor3DOpacity });

            return object3DViewAspects;
        }

        public void updateGeometryColor(Geometry geometry)
        {
            if (geometry.actor == null) return;

            switch (geometry.geometryName)
            {
                case Messages.bladderGeometry:
                    geometry.actor.GetProperty().SetColor(bladder3DColor[0], bladder3DColor[1], bladder3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(bladder3DOpacity);
                    break;
                case Messages.outerWallGeometry:
                    geometry.actor.GetProperty().SetColor(outerWall3DColor[0], outerWall3DColor[1], outerWall3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(outerWall3DOpacity);
                    break;
                case Messages.layerGeometry:
                    geometry.actor.GetProperty().SetColor(layer3DColor[0], layer3DColor[1], layer3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(layer3DOpacity);
                    break;
                case Messages.oxyGeometry:
                    geometry.actor.GetProperty().SetColor(OXY3DColor[0], OXY3DColor[1], OXY3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(OXY3DOpacity);
                    break;
                case Messages.deoxyGeometry:
                    geometry.actor.GetProperty().SetColor(DeOXY3DColor[0], DeOXY3DColor[1], DeOXY3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(DeOXY3DOpacity);
                    break;
                case Messages.GNRGeometry:
                    geometry.actor.GetProperty().SetColor(GNR3DColor[0], GNR3DColor[1], GNR3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(GNR3DOpacity);
                    break;
                case Messages.tumorGeometry:
                    geometry.actor.GetProperty().SetColor(tumor3DColor[0], tumor3DColor[1], tumor3DColor[2]);
                    geometry.actor.GetProperty().SetOpacity(tumor3DOpacity);
                    break;
            }

        }

        public void updateRenderer()
        {
            foreach (Geometry geometry in context.getMainWindow().STLGeometries)
            {
                updateGeometryColor(geometry);
            }
            context.getMainWindow().renderer.GetRenderWindow().Render();
        }


        public void onPalletWindowClosing()
        {
            this.palletWindow = null;
        }


    }

    public class Object3DViewAspects
    {
        public string objectName { get; set; }
        public double[] colors { get; set; } //[0]red, [1]green, [2]blue
        public double opacity { get; set; }
    }
}
