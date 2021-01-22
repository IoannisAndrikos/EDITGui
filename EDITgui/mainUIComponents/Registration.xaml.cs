using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDITgui
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : UserControl
    {
        private Context context;

        private Point zeroPoint = new Point(0, 0);

        private enum setState { R1, R2, R3, NONE};

        private setState currentState = setState.NONE;

        public List<Point> registrationPoints;
        public List<int> framesOfRegistrationPoints;

        public Registration()
        {
            InitializeComponent();
        }

        public Registration(Context context)
        {
            registrationPoints = new List<Point>();
            framesOfRegistrationPoints = new List<int>();
            InitializeComponent();
            this.context = context;
            this.Visibility = Visibility.Collapsed;
            initializeRegistrationPoints();
        }

        public void initializeRegistrationPoints()
        {
            this.currentState = setState.NONE;
            this.registrationPoints.Clear();
            this.framesOfRegistrationPoints.Clear();

            this.registrationPoints.Add(zeroPoint);
            this.registrationPoints.Add(zeroPoint);
            this.registrationPoints.Add(zeroPoint);

            this.framesOfRegistrationPoints.Add(-1);
            this.framesOfRegistrationPoints.Add(-1);
            this.framesOfRegistrationPoints.Add(-1);

            R1_Frame.Content = context.getMessages().frame + ": -";
            R2_Frame.Content = context.getMessages().frame + ": -";
            R3_Frame.Content = context.getMessages().frame + ": -";

        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            if(registration_panel.Visibility == Visibility.Collapsed)
            {
                registration_panel.Visibility = Visibility.Visible;
            }else if(registration_panel.Visibility == Visibility.Visible)
            {
                registration_panel.Visibility = Visibility.Collapsed;
            }
        }

        int result;
        public Point getRegistrationPoints(int slider_value)
        {
            result = framesOfRegistrationPoints.IndexOf(slider_value);

            if (result != -1)
            {
                Console.WriteLine(result);
                Console.WriteLine(registrationPoints[result]);
                return registrationPoints[result];
            }
            else
            {
                return zeroPoint;
            }
        }

        public List<string> getRegistrationPointsTextList()
        {
            List<string> registrationPointsTextList = new List<string>();

            for (int i=0; i < registrationPoints.Count; i++ )
            {
                registrationPointsTextList.Add(registrationPoints[i].X.ToString() + " " + registrationPoints[i].Y.ToString() + " " + context.getMessages().frame + framesOfRegistrationPoints[i].ToString());
            }
            return registrationPointsTextList;
        }


        public int setRegistrationPoint(Point point, int frame)
        {
            switch (this.currentState)
            {
                case setState.R1:
                   return Set_R1(point, frame);
                    break;
                case setState.R2:
                    return Set_R2(point, frame);
                    break;
                case setState.R3:
                    return Set_R3(point, frame);
                    break;
                default:
                    return 0;
            }
        }


        private void Set_R1_Click(object sender, RoutedEventArgs e)
        {
            context.getUltrasoundPart().addRegistrationPoint();
            this.currentState = setState.R1;
        }

        private void Set_R2_Click(object sender, RoutedEventArgs e)
        {
            context.getUltrasoundPart().addRegistrationPoint();
            this.currentState = setState.R2;
        }

        private void Set_R3_Click(object sender, RoutedEventArgs e)
        {
            context.getUltrasoundPart().addRegistrationPoint();
            this.currentState = setState.R3;
        }

        private int Set_R1(Point point, int frame)
        {
            if(framesOfRegistrationPoints[1] != frame && framesOfRegistrationPoints[2] != frame)
            {
                this.registrationPoints[0] = point;
                this.framesOfRegistrationPoints[0] = frame;
                R1_Frame.Content = context.getMessages().frame + ": " + frame.ToString();
                return 0;
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().alreadySpecifiedRegistrationPointForCurrentFrame, context.getMessages().warning, MessageBoxButton.OK);
                return 1;
            }
        }

        private int Set_R2(Point point, int frame)
        {
            if (framesOfRegistrationPoints[0] != frame && framesOfRegistrationPoints[2] != frame)
            {
                this.registrationPoints[1] = point;
                this.framesOfRegistrationPoints[1] = frame;
                R2_Frame.Content = context.getMessages().frame + ": " + frame.ToString();
                return 0;
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().alreadySpecifiedRegistrationPointForCurrentFrame, context.getMessages().warning, MessageBoxButton.OK);
                return 1;
            }
        }

        private int Set_R3(Point point, int frame)
        {
            if (framesOfRegistrationPoints[0] != frame && framesOfRegistrationPoints[1] != frame)
            {
                this.registrationPoints[2] = point;
                this.framesOfRegistrationPoints[2] = frame;
                R3_Frame.Content = context.getMessages().frame + ": " + frame.ToString();
                return 0;
            }
            else
            {
                CustomMessageBox.Show(context.getMessages().alreadySpecifiedRegistrationPointForCurrentFrame, context.getMessages().warning, MessageBoxButton.OK);
                return 1;
            }
        }
      
    }
}
