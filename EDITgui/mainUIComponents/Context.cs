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
        private ultrasoundDataParser ultrasoundData;
        //private Registration registration;
        private photoAcousticDataParser photoAcousticData;
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
        private manageTumorGroups manageTumorGroups;
        private Comparator3D comparator;

        public Context(MainWindow mainWindow, Login user)
        {
            //Be careful here! The order below plays significant role
            this.user = user;
            this.messages = new Messages();
            this.mainWindow = mainWindow;
            this.images = new ImageSequence(this);
            this.ultrasoundData = new ultrasoundDataParser(this);
            //this.registration = new Registration(this);
            this.photoAcousticData = new photoAcousticDataParser(this);
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
            this.manageTumorGroups = new manageTumorGroups(this);
            this.comparator = new Comparator3D(this);
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

        public ultrasoundDataParser getUltrasoundData()
        {
            return this.ultrasoundData;
        }

        //public Registration getRegistration()
        //{
        //    return this.registration;
        //}


        public photoAcousticDataParser getPhotoAcousticData()
        {
            return this.photoAcousticData;
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

        public manageTumorGroups getTumorGroups()
        {
            return this.manageTumorGroups;
        }

        public Comparator3D getComparator()
        {
            return this.comparator;
        }
    }
}
