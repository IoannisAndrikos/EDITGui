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
        private Points2D points2D;
        private UltrasoundPart ultrasoundPart;
        private PhotoAcousticPart photoAcousticPart;
        private manage2DUltrasound ultrasoundPoints2D;
        private manage2DPhotoAcoustic photoAcousticPoints2D;
        private metricsCalculations metrics;
        private coreFunctionality core;
        private settings studySettings;
        private Messages messages;
        private checkBeforeExecute check;
        private StudyFile studyFile;
        private LoadActions loadActions;
        private SaveActions saveActions;

        public Context(MainWindow mainWindow, Login user)
        {
            //Be careful here! The order below plays significant role
            this.messages = new Messages();
            this.mainWindow = mainWindow;
            this.points2D = new Points2D(this);
            this.ultrasoundPoints2D = new manage2DUltrasound(this);
            this.photoAcousticPoints2D = new manage2DPhotoAcoustic(this);
            this.user = user;
            this.ultrasoundPart = new UltrasoundPart(this);
            this.photoAcousticPart = new PhotoAcousticPart(this);
            this.metrics = new metricsCalculations(this);
            this.core = new coreFunctionality(this);
            this.studySettings = new settings(this);
            this.check = new checkBeforeExecute(this);
            this.studyFile = new StudyFile();
            this.loadActions = new LoadActions(this);
            this.saveActions = new SaveActions(this);
        }


        public MainWindow getMainWindow()
        {
            return this.mainWindow;
        }

        public Login getUser()
        {
            return this.user;
        }

        public Points2D getPoints2D()
        {
            return this.points2D;
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

        public manage2DPhotoAcoustic getPhotoAcousticPoints2D()
        {
            return this.photoAcousticPoints2D;
        }

        public metricsCalculations getMetrics()
        {
            return this.metrics;
        }
        
        public coreFunctionality getCore()
        {
            return this.core;
        }

        public settings getStudySettings()
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

        public StudyFile getStudyFile()
        {
            return this.studyFile;
        }

        public LoadActions getLoadActions()
        {
            return this.loadActions;
        }

        public SaveActions getSaveActions()
        {
            return this.saveActions;
        }


    }
}
