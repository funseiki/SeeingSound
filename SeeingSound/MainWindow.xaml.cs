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
        protected Skeleton[] SkeletonData;

        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Console.WriteLine("Kinecting...");
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

                    WallpaperPage.setup(sensor, SkeletonData);

                    Console.WriteLine("Kinected!");
                }
                catch(IOException)
                {
                    sensor = null;
                    Console.WriteLine("Sensor is in use elsewhere");
                }
            }
            else
            {
                Console.WriteLine("Unable to find the Kinect");
                return;
            }
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WallpaperPage.shutdown();
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
                            WallpaperPage.NewSkeletonFound(skeleton);
                        }
                    }
                }
            }
        }
    }
}
