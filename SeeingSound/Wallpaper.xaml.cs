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

using Microsoft.Kinect;


namespace SeeingSound
{
    /// <summary>
    /// Interaction logic for SoundLines.xaml
    /// </summary>
    public partial class Wallpaper : UserControl
    {
        protected KinectSensor sensor;
        protected Dictionary<int, Player> players = new Dictionary<int, Player>();
        protected Skeleton[] SkeletonData;
        protected Binding canvasHeightBinding = new Binding("ActualHeight");

        public Wallpaper()
        {
            InitializeComponent();
        }

        public void setup(KinectSensor sensor, Skeleton[] SkeletonData)
        {
            this.sensor = sensor;
            this.SkeletonData = SkeletonData;
            initializeCanvas();
            initializeSoundSource();
        }

        public void shutdown()
        {
            if (sensor == null) return;
            sensor.AudioSource.Stop();
            sensor.Stop();
        }

        public void NewSkeletonFound(Skeleton skeleton)
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

        private void initializeCanvas()
        {
            canvasHeightBinding.Source = DrawingArea;
        }

        private void initializeSoundSource()
        {
            if (sensor == null) return;
            sensor.AudioSource.SoundSourceAngleChanged += AudioSource_SoundSourceAngleChanged;
            sensor.AudioSource.Start();
        }

        private void AudioSource_SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            drawPlayerSound();
        }

        private void drawPlayerSound()
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
            else
            {
                Console.WriteLine("Found sound not player");
            }
        }


        /*
         * Find the player closest to the sound generated.
         * 
         * TODO test this (I need another person and a quiet room to test)
         * TODO use the confidence level and a cutoff for how far away the skeleton and sound can be
         */
        private Player findPlayerAtSound()
        {
            Skeleton closestSkeleton = null;
            double closestAngleDifference = double.PositiveInfinity;

            // go through each skeleton computing it's angle from the center of the kinect, then find the closest angle
            foreach (Skeleton skeleton in this.SkeletonData.Where(s => (s != null) && (s.TrackingState != SkeletonTrackingState.NotTracked)))
            {
                double skeletonAngle = MathHelper.Arcsin(skeleton.Position.X / skeleton.Position.Z);
                double angleDifference = Math.Abs(skeletonAngle - sensor.AudioSource.SoundSourceAngle);
                if (angleDifference < closestAngleDifference)
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

        /* Converts x skeleton coordinate to a position on the canvas
         * 
         * TODO take into account that the screen size scales when projected
         */
        private double kinectXToCanvasX(float xPosition)
        {
            return (xPosition * DrawingArea.ActualWidth / 2) + (ActualWidth / 2);
        }

        /**
String s = "Source angle: " + sensor.AudioSource.SoundSourceAngle + "\n" +
    "Confidence: " + sensor.AudioSource.SoundSourceAngleConfidence + "\n" +
    "Beam Angle: " + sensor.AudioSource.BeamAngle;
 * **/


    }
}
