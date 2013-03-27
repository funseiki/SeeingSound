
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;


/** A Player is a person within the view who talks
 */
namespace SeeingSound
{
    public class Player
    {
        protected int _skeletonID = -1;
        public int SkeletonID
        {
            get { return _skeletonID; }
            set { _skeletonID = value; }
        }
        protected SolidColorBrush _color = null;
        public SolidColorBrush LineColor
        {
            get { return _color; }
            set { _color = value; }
        }

        protected double _xPosition = 0;
        public double XPosition {
            get { return _xPosition; }
            set { _xPosition = value; }
        }

        public Player(int id)
        {
            LineColor = RandomColor();
            SkeletonID = id;
        }

        private SolidColorBrush RandomColor()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, PlayerColors.Colors.Length);
            return new SolidColorBrush(PlayerColors.Colors[randomNumber]);
        }

        public Line CreateLineAtCurrentPosition(double thickness)
        {
            Line line = new Line();
            
            // Y1, Y2 are set by the main window
            line.X1 = XPosition;
            line.X2 = XPosition;
            line.Stroke = this.LineColor;
            line.StrokeThickness = thickness;
            return line;
        }

    }
}
