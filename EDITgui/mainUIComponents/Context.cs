using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDITgui
{
    public class Context
    {
        private MainWindow mainWindow;
        private Login user;
        private ImageSequence images;
        private UltrasoundPart ultrasoundPart;
        private PhotoAcousticPart photoAcousticPart;
        private manage2DUltrasound ultrasoundPoints2D;
        //private Registration registration;
        private manage2DPhotoAcoustic photoAcousticPoints2D;
        private metricsCalculations metrics;
        private coreFunctionality core;
        private Settings studySettings;
        private Messages messages;
        private checkBeforeExecute check;
        private StudyFile studyFile;
        private LoadActions loadActions;
        private SaveActions saveActions;
        private Slicer3D slicer;
        private Pallet pallet;

        public Context(MainWindow mainWindow, Login user)
        {
            //Be careful here! The order below plays significant role
            this.user = user;
            this.messages = new Messages();
            this.mainWindow = mainWindow;
            this.images = new ImageSequence(this);
            this.ultrasoundPoints2D = new manage2DUltrasound(this);
            //this.registration = new Registration(this);
            this.photoAcousticPoints2D = new manage2DPhotoAcoustic(this);
            this.ultrasoundPart = new UltrasoundPart(this);
            this.photoAcousticPart = new PhotoAcousticPart(this);
            this.metrics = new metricsCalculations(this);
            this.slicer = new Slicer3D(this);
            this.core = new coreFunctionality(this);
            this.studySettings = new Settings(this);
            this.check = new checkBeforeExecute(this);
            this.loadActions = new LoadActions(this);
            this.saveActions = new SaveActions(this);
            this.studyFile = new StudyFile();
            this.pallet = new Pallet(this);
        }


        public MainWindow getMainWindow()
        {
            return this.mainWindow;
        }

        public Login getUser()
        {
            return this.user;
        }

        public ImageSequence getImages()
        {
            return this.images;
        }

        public UltrasoundPart getUltrasoundPart()
        {
            return this.ultrasoundPart;
        }

        public PhotoAcousticPart getPhotoAcousticPart()
        {
            return this.photoAcousticPart;
        }

        public manage2DUltrasound getUltrasoundPoints2D()
        {
            return this.ultrasoundPoints2D;
        }

        //public Registration getRegistration()
        //{
        //    return this.registration;
        //}


        public manage2DPhotoAcoustic getPhotoAcousticPoints2D()
        {
            return this.photoAcousticPoints2D;
        }

        public metricsCalculations getMetrics()
        {
            return this.metrics;
        }

        public Slicer3D getSlicer()
        {
            return this.slicer;
        }
        
        public coreFunctionality getCore()
        {
            return this.core;
        }

        public Settings getStudySettings()
        {
            return this.studySettings;
        }

        public Messages getMessages()
        {
            return this.messages;
        }

        public checkBeforeExecute getCheck()
        {
            return this.check;
        }

        public LoadActions getLoadActions()
        {
            return this.loadActions;
        }

        public SaveActions getSaveActions()
        {
            return this.saveActions;
        }

        public StudyFile getStudyFile()
        {
            return this.studyFile;
        }

        public Pallet getPallet()
        {
            return this.pallet;
        }

    }
}
