using System;
using System.Windows;
using System.Windows.Media.Animation;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{

    /// <summary>
    /// Simple factory class for the animation that slides tiles around the puzzle
    /// </summary>
    public class TileAnimator
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a storyboard that animates a tile moving from its current position to a new position specified by <paramref name="targetX"/> and <paramref name="targetY"/>
        /// </summary>
        /// <param name="tvm">The tile that is to be moved.  The animation will be bound to the <see cref="TileViewModel.Top"/> and <see cref="TileViewModel.Left"/> properties.</param>
        /// <param name="targetX">The x-coordinate that the tile should be moved to (in absolute pixels from the left edge of the phone screen</param>
        /// <param name="targetY">The y-coordinate that the tile should be moved to (in absoulte pixels from the top edge of the phone screen</param>
        /// <returns>A storyboard ready to be executed</returns>
        public static Storyboard CreateSlideAnimation(TileViewModel tvm, double targetX, double targetY)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(0.2));
            Logger.Info("Creating an animation lasting {0} to move tile {1} from ({2},{3}) to ({4},{5})", duration,
                        tvm.Position, tvm.Left, tvm.Top, targetX, targetY);

            Storyboard tileStoryboard = new Storyboard();
            tileStoryboard.Duration = duration;

            DoubleAnimation tileHorizontalAnimation = new DoubleAnimation();
            tileHorizontalAnimation.Duration = duration;
            tileHorizontalAnimation.To = targetX;
            Storyboard.SetTarget(tileHorizontalAnimation, tvm);
            Storyboard.SetTargetProperty(tileHorizontalAnimation, new PropertyPath(TileViewModel.LeftProperty) );
            tileStoryboard.Children.Add(tileHorizontalAnimation);

            DoubleAnimation tileVerticalAnimation = new DoubleAnimation();
            tileVerticalAnimation.Duration = duration;
            tileVerticalAnimation.To = targetY;
            Storyboard.SetTarget(tileVerticalAnimation, tvm);
            Storyboard.SetTargetProperty(tileVerticalAnimation, new PropertyPath(TileViewModel.TopProperty));
            tileStoryboard.Children.Add(tileVerticalAnimation);

            return tileStoryboard;
        }

    }
}
