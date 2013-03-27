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

        // Audio sample related
        /// <summary>
        /// Number of milliseconds between audio pings
        /// </summary>
        protected const int AudioPollingInterval = 50;

        /// <summary>
        /// How many audio samples to get each millisecond
        /// </summary>
        protected const int SamplesPerMillisecond = 16;

        /// <summary>
        /// How large, in bytes, each audio sample is
        /// </summary>
        protected const int BytesPerSample = 2;

        /// <summary>
        /// The number of audio samples we need before recording the energy here
        /// </summary>
        protected int SamplesPerLine = 40;

        /// <summary>
        /// Used to hold audio energy data
        /// </summary>
        protected byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        /// <summary>
        /// Stream to grab audio data from Kinect
        /// </summary>
        private Stream audioStream;

        private Thread readingThread;

        private object readingLock = new object();
        private Boolean _reading = false;
        private Boolean Reading
        {
            get
            {
                lock(readingLock)
                {
                    return _reading;
                }
            }
            set
            {
                lock(readingLock)
                {
                    _reading = value;
                }
            }
        }

        private object EnergyLock = new object();

        private double _lastKnownEnergy = 0;

        public Double LastKnownEnergy
        {
            get
            {
                lock(EnergyLock)
                {
                    return _lastKnownEnergy;
                }
            }

            set
            {
                lock(EnergyLock)
                {
                    _lastKnownEnergy = value;
                }
            }
        }

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
            initializeReadingThread();
        }

        public void shutdown()
        {
            Reading = false;
            if (readingThread != null)
            {
                readingThread.Join();
            }

            if (sensor != null)
            {
                sensor.AudioSource.SoundSourceAngleChanged -= AudioSource_SoundSourceAngleChanged;
                sensor.AudioSource.Stop();
                sensor.Stop();
            }
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
                Console.WriteLine("Made new skeleton");
                player = new Player(track_id);
                players.Add(track_id, player);
            }

            player.XPosition = kinectXToCanvasX(skeleton.Position.X);
        }

        private void initializeCanvas()
        {
            canvasHeightBinding.Source = DrawingArea;
        }

        private void initializeReadingThread()
        {
            Reading = true;
            readingThread = new Thread(AudioReadingThread);
            readingThread.Start();
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


        private void AudioReadingThread()
        {
            double minEnergy = 0.2;
            int sampleCount = 0;
            double squareSum = 0;

            while (Reading)
            {
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);

                for (int i = 0; i < readCount; i += BytesPerSample)
                {
                    short audioSample = BitConverter.ToInt16(audioBuffer, i);
                    squareSum += audioSample * audioSample;
                    ++sampleCount;

                    // We need at least 40 Samples before recording a new energy
                    if(sampleCount < SamplesPerLine)
                    {
                        continue;
                    }

                    double meanSquare = squareSum / SamplesPerLine;
                    double energyAmount = Math.Log(meanSquare) / Math.Log(int.MaxValue);

                    LastKnownEnergy = Math.Max(0, energyAmount - minEnergy) / (1 - minEnergy);
                }
            }
        }
    }
}
