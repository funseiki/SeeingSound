
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
        static Color[] PlayerColors = { Colors.Aqua, Colors.Beige, Colors.BlanchedAlmond,
                             Colors.BurlyWood, Colors.Coral, Colors.Cornsilk };
        protected int _skeletonID = -1;
        public int SkeletonID
        {
            get { return _skeletonID; }
            set { _skeletonID = value; }
        }
        protected SolidColorBrush _color = null;
        public SolidColorBrush Color
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
            Color = new SolidColorBrush(Player.PlayerColors[0]);
            SkeletonID = id;
        }

        public Line CreateLineAtCurrentPosition()
        {
            Line line = new Line();
            
            // Y1, Y2 are set by the main window
            line.X1 = XPosition;
            line.X2 = XPosition;
            line.Stroke = this.Color;

            return line;
        }

    }
}
