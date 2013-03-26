
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
    }
}
