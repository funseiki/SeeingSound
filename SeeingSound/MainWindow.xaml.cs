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
        protected Binding canvasHeightBinding = new Binding("ActualHeight");

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

        private void initializeCanvas()
        {
            canvasHeightBinding.Source = DrawingArea;
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

                            player.XPosition = kinectXToCanvasX(skeleton.Position.X);
                        }
                    }
                }
            }
        }

        /* Converts x skeleton coordinate to a position on the canvas
         * 
         * TODO take into account that the screen size scales when projected
         */
        private double kinectXToCanvasX(float xPosition)
        {
            return (xPosition * DrawingArea.ActualWidth / 2) + (ActualWidth / 2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawMarkers();
            initializeCanvas();
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
            drawPlayerSound();
        }

        void AudioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            // Do we even need to know when the beam angle changes? It might be
            // useful if we can point the beam at skeletons, or have it
            // automatically listen by person
            setAudioInfo();
        }

        void drawPlayerSound()
        {
            Player player = findPlayerAtSound();
            if (player != null)
            {
                Console.WriteLine("we found a player!");
                Line line = player.CreateLineAtCurrentPosition();
                line.Y1 = 0;
                line.SetBinding(Line.Y2Property, canvasHeightBinding);

                DrawingArea.Children.Add(line);
            }
            else {
                Console.WriteLine("Found sound not player");
            }
        }

        /*
         * Find the player closest to the sound generated.
         * 
         * TODO test this (I need another person and a quiet room to test)
         * TODO use the confidence level and a cutoff for how far away the skeleton and sound can be
         */
        Player findPlayerAtSound()
        {
            Skeleton closestSkeleton = null;
            double closestAngleDifference = double.PositiveInfinity;

            // go through each skeleton computing it's angle from the center of the kinect, then find the closest angle
            foreach (Skeleton skeleton in this.SkeletonData.Where(s => (s != null) && (s.TrackingState != SkeletonTrackingState.NotTracked)))
            {
                double skeletonAngle = MathHelper.Arcsin(skeleton.Position.X / skeleton.Position.Z);
                double angleDifference = Math.Abs(skeletonAngle - sensor.AudioSource.SoundSourceAngle);
                if ( angleDifference < closestAngleDifference)
                {
                    closestAngleDifference = angleDifference;
                    closestSkeleton = skeleton;
                }
            }

            if (closestSkeleton != null)
            {
                return players[closestSkeleton.TrackingId];
            }
            else
            {
                return null;
            }
        }

    }
}
