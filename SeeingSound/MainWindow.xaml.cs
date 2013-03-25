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
        protected double segmentWidth = 0;
        protected int pixelsPerDegree = 10;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void setStatus(String text)
        {
            //StatusInfo.Text = text;
        }

        private void drawMarkers()
        {
            segmentWidth = DrawingArea.ActualWidth / pixelsPerDegree;
            Console.WriteLine(DrawingArea.ActualWidth);
            for(int i = 0; i <= 10; i++)
            {
                Line l = new Line();
                l.X1 = i*segmentWidth;
                l.Y1 = DrawingArea.ActualHeight - 10;
                l.X2 = l.X1;
                l.Y2 = DrawingArea.ActualHeight;
                l.Stroke = Brushes.Black;
                DrawingArea.Children.Add(l);
            }
        }

        private void setAudioInfo()
        {
            String s = "Source angle: " + sensor.AudioSource.SoundSourceAngle + "\n" +
                "Confidence: " + sensor.AudioSource.SoundSourceAngleConfidence + "\n" +
                "Beam Angle: " + sensor.AudioSource.BeamAngle;
            AudioInfo.Text = s;
            AudioLine.X1 = DrawingArea.ActualWidth / 2;
            AudioLine.X2 = AudioLine.X1 + Convert.ToDouble(sensor.AudioSource.SoundSourceAngle)*pixelsPerDegree;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sensor == null) return;
            sensor.AudioSource.Stop();
            sensor.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawMarkers();
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
