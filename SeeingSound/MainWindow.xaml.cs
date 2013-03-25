using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SeeingSound
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        protected KinectSensor sensor;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void setStatus(String text)
        {
            KinectText.Text = text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            setStatus("Kinecting...");
            foreach(var potential_sensor in KinectSensor.KinectSensors)
            {
                if(potential_sensor.Status == KinectStatus.Connected)
                {
                    sensor = potential_sensor;
                    break;
                }
            }

            if(sensor != null)
            {
                try
                {
                    sensor.Start();
                    setStatus("Kinected!");
                }
                catch(IOException)
                {
                    sensor = null;
                    setStatus("Sensor is in use elsewhere");
                }
            }
            else
            {
                setStatus("Unable to find the Kinect");
            }
        }

    }
}
