using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            StatusInfo.Text = text;
        }

        private void setAudioInfo()
        {
            String s = "Source angle: " + sensor.AudioSource.SoundSourceAngle + "\n" +
                "Confidence: " + sensor.AudioSource.SoundSourceAngleConfidence +
                "Beam Angle: " + sensor.AudioSource.BeamAngle;
            AudioInfo.Text = s;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sensor == null) return;
            sensor.AudioSource.Stop();
            sensor.Stop();
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
                return;
            }

            setListeners();
            sensor.AudioSource.Start();
        }

        private void setListeners()
        {
            if (sensor == null) return;

            sensor.AudioSource.BeamAngleChanged += AudioSource_BeamAngleChanged;
            sensor.AudioSource.SoundSourceAngleChanged += AudioSource_SoundSourceAngleChanged;

        }

        void AudioSource_SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            setAudioInfo();
        }

        void AudioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            setAudioInfo();
        }

    }
}
