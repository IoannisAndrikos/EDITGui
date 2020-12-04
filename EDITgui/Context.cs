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
        private UltrasoundPart ultrasoundPart;
        private PhotoAcousticPart photoAcousticPart;
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
            messages = new Messages();
            this.mainWindow = mainWindow;
            this.user = user;
            ultrasoundPart = new UltrasoundPart(this);
            photoAcousticPart = new PhotoAcousticPart(this);
            metrics = new metricsCalculations(this);
            core = new coreFunctionality(this);
            studySettings = new settings(this);
            check = new checkBeforeExecute(this);
            studyFile = new StudyFile();
            loadActions = new LoadActions(this);
            saveActions = new SaveActions(this);
        }


        public MainWindow getMainWindow()
        {
            return this.mainWindow;
        }

        public Login getUser()
        {
            return this.user;
        }

        public UltrasoundPart getUltrasoundPart()
        {
            return this.ultrasoundPart;
        }

        public PhotoAcousticPart getPhotoAcousticPart()
        {
            return this.photoAcousticPart;
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
