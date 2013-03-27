
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
        public Player(int id)
        {
            Color = new SolidColorBrush(Player.PlayerColors[0]);
            SkeletonID = id;
        }

        public Line CreateLineAtCurrentPosition()
        {
            // TODO implement
            Line line = new Line();
            /**double xLoc = (skeleton.Position.X * DrawingArea.ActualWidth / 2) + (ActualWidth / 2);
            double yLoc = skeleton.Position.Y;**/
            
            // Y1, Y2 are set by the main window
            // xLoc is a stub
            double xLoc = 200;

            line.X1 = xLoc;
            line.X2 = xLoc;
            line.Stroke = this.Color;
            return line;
        }
    }
}
