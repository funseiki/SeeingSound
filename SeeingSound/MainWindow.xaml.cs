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
        protected Skeleton[] SkeletonData;
        protected Dictionary<int, Player> players = new Dictionary<int,Player>();

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
                double xLoc = i * segmentWidth;
                double yLoc = DrawingArea.ActualHeight - 10;

                l.X1 = xLoc;
                l.Y1 = yLoc;
                l.X2 = xLoc;
                l.Y2 = yLoc + 10;
                l.Stroke = Brushes.Black;

                if(i > 5)
                {
                    xLoc -= 20;
                }
                TextBlock t = new TextBlock();
                t.Text = Convert.ToString(i * pixelsPerDegree - 50);
                t.Margin = new Thickness(xLoc, yLoc - 20, 0, 0);

                DrawingArea.Children.Add(l);
                DrawingArea.Children.Add(t);
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

        private void setSkeletonData()
        {
            sensor.SkeletonStream.Enable();
            SkeletonData = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this.SkeletonData == null) || (this.SkeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.SkeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }
                    skeletonFrame.CopySkeletonDataTo(SkeletonData);
                    foreach (Skeleton skeleton in this.SkeletonData)
                    {
                        if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                        {
                            Player player;
                            int track_id = skeleton.TrackingId;
                            if (players.ContainsKey(track_id))
                            {
                                player = players[track_id];
                            }
                            else
                            {
                                player = new Player(track_id);
                                players.Add(track_id, player);
                            }
                            Line l = new Line();
                            double xLoc = (skeleton.Position.X*DrawingArea.ActualWidth/2) + (ActualWidth/2);
                            double yLoc = skeleton.Position.Y;
                            Console.WriteLine("Xpos, XposFloat: " + xLoc + "," + skeleton.Position.X);
                            l.X1 = xLoc;
                            l.Y1 = yLoc;
                            l.X2 = xLoc;
                            l.Y2 = yLoc + 20;
                            l.Stroke = Brushes.Blue;

                            DrawingArea.Children.Add(l);
                        }
                    }
                }
            }
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
                    // Setting skeleton stuff
                    setSkeletonData();

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
