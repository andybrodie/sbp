using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.Model;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    /// <summary>
    /// The view model object for a single tile in the puzzle.
    /// </summary>
    public class TileViewModel : DependencyViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The vertical pixel offset from the top of the puzzle that the tile occupies.
        /// </summary>
        /// <remarks>
        /// This is a dependency property for two reasons:
        /// <list type="number">
        /// <item><description>It can be bound to the <see cref="ModelTop"/> property, for when the tile is not moving</description></item>
        /// <item><description>When the tile is moving, the animation overrides this binding (see <see cref="TileAnimator"/>) temporarily to move the tile</description></item>
        /// </list>
        /// </remarks>
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof (double),
                                                                                            typeof (TileViewModel),
                                                                                            new PropertyMetadata(0.00));

        /// <summary>
        /// The horizontal pixel offset from the top of the puzzle that the tile occupies.
        /// </summary>
        /// <remarks>
        /// This is a dependency property for two reasons:
        /// <list type="number">
        /// <item><description>It can be bound to the <see cref="ModelTop"/> property, for when the tile is not moving</description></item>
        /// <item><description>When the tile is moving, the animation overrides this binding (see <see cref="TileAnimator"/>) temporarily to move the tile</description></item>
        /// </list>
        /// </remarks>
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof (double),
                                                                                             typeof (TileViewModel),
                                                                                             new PropertyMetadata(0.00));

        private readonly TileModel _tile;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private double _height;

        /// <summary>
        /// Backing field for <see cref="ImageHeight"/>
        /// </summary>
        private double _imageHeight;

        /// <summary>
        /// Backing field for <see cref="ImageLeft"/>
        /// </summary>
        private double _imageLeft;

        /// <summary>
        /// Backing field for <see cref="ImageHeight"/>
        /// </summary>
        private double _imageWidth;

        /// <summary>
        /// Backing field for <see cref="ImageTop"/>
        /// </summary>
        private double _imageTop;

        /// <summary>
        /// Backing field for <see cref="TileBorder"/>
        /// </summary>
        private double _tileBorder;

        /// <summary>
        /// Backing field for <see cref="Width"/>
        /// </summary>
        private double _width;


        /// <summary>
        /// Initialises this view model, locking it to the <paramref name="tileModel"/> passed.  Also hooks up <see cref="PropertyChangedCallback"/> to
        /// our own <see cref="UpdatePositionBasedOnModelChange"/>
        /// </summary>
        /// <param name="tileModel"></param>
        public TileViewModel(TileModel tileModel)
        {
            _tile = tileModel;
            PropertyChanged += UpdatePositionBasedOnModelChange;

            // Detects changes to the X and Y position of the tile from the puzzle (relative to the other tiles, e.g. 0,0; 0,1; 2,2; etc. (not to be confused with Top and Left which are pixel offsets)
            tileModel.TileMoved += TileModelOnTileMoved;
        }

        /// <summary>
        /// Pixel offset from the top of the puzzle.  <see cref="TopProperty"/> binds to this when no animation is running on the tile
        /// </summary>
        public double ModelTop
        {
            get { return Position.Y*Height; }
        }

        /// <summary>
        /// Pixel offset from the left of the puzzle.  <see cref="LeftProperty"/> binds to this when no animation is running on the tile
        /// </summary>
        public double ModelLeft
        {
            get { return Position.X*Width; }
        }


        /// <summary>
        /// The pixel width of this tile
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is what the <see cref="FrameworkElement.Width"/> property should be bound to</para>
        /// <para>
        /// Changing this has two knock-on effects:
        /// <list type="number">
        /// <item><description>It notifies a change to the calculated <see cref="ModelLeft"/> property</description></item>
        /// <item><description>It changes the <see cref="ImageWidth"/> property to recalculate the width of hte image within the total width (dependant on <see cref="TileBorder"/>)</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public double Width
        {
            get { return _width; }
            set
            {
                Logger.Debug("Tile {0} width updated from {1} to {2}", Position , _width, value);
                _width = value;
                OnNotifyPropertyChanged("Width");
                OnNotifyPropertyChanged("ModelLeft");
                ImageWidth = Width - TileBorder*2;
            }
        }

        /// <summary>
        /// The pixel height of this tile
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is what the <see cref="FrameworkElement.Height"/> property should be bound to</para>
        /// <para>
        /// Changing this has two knock-on effects:
        /// <list type="number">
        /// <item><description>It notifies a change to the calculated <see cref="ModelTop"/> property</description></item>
        /// <item><description>It changes the <see cref="ImageHeight"/> property to recalculate the width of hte image within the total width (dependant on <see cref="TileBorder"/>)</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public double Height
        {
            get { return _height; }
            set
            {
                Logger.Debug("Tile {0} height updated from {0} to {1}", Position, _height, value);
                _height = value;
                OnNotifyPropertyChanged("Height");
                OnNotifyPropertyChanged("ModelTop");
                ImageHeight = Height - TileBorder*2;
            }
        }


        /// <summary>
        ///   TileControl's binds its <see cref="Canvas.TopProperty" /> proprerty to this one to determine where on the screen the tile should sit vertically.
        /// </summary>
        /// <remarks>
        ///   This is a dependency property itself ( <see cref="TopProperty" /> ). Whilst it is usually bound to the model via the <see
        ///    cref="ModelTop" /> property, when being animated this binding is temporarily removed and handed to the animation system. Once the animation is complete, the binding to <see
        ///    cref="ModelTop" /> is restored.
        /// </remarks>
        public double Top
        {
            get { return (double) GetValue(TopProperty); }
            set
            {
                SetValue(TopProperty, value);
                OnNotifyPropertyChanged("Top");
            }
        }


        /// <summary>
        ///   TileControl's binds its <see cref="Canvas.TopProperty" /> proprerty to this one to determine where on the screen the tile should sit vertically.
        /// </summary>
        /// <remarks>
        ///   This is a dependency property itself ( <see cref="LeftProperty" /> ). Whilst it is usually bound to the model via the <see
        ///    cref="ModelTop" /> property, when being animated this binding is temporarily removed and handed to the animation system. Once the animation is complete, the binding to <see
        ///    cref="ModelTop" /> is restored.
        /// </remarks>
        public double Left
        {
            get { return (double) GetValue(LeftProperty); }
            set
            {
                SetValue(LeftProperty, value);
                OnNotifyPropertyChanged("Left");
            }
        }

        /// <summary>
        /// The width of the border around each tile.
        /// </summary>
        /// <remarks>
        /// <para>Unlike normal Silverlight the border eats in to the total width and height of the control, it doesn't wrap around it.
        /// This is because the puzzle is a fixed width and height and everything has to fit within it, rather than the puzzle being sized to accommodate the tiles.</para>
        /// <para>Changing this causes changes to <see cref="ImageWidth"/> and <see cref="ImageHeight"/> as well because of this.</para>
        /// 
        /// </remarks>
        public double TileBorder
        {
            get { return _tileBorder; }
            set
            {
                // Suppress anyhting that doesn't change the value
                if (Math.Abs(_tileBorder - value) < 1)
                {
                    Logger.Info("Ignoring update of TileBorder from {0} to {1}", _tileBorder, value);
                }
                else
                {
                    Logger.Info("Updating TileBorder from {0} to {1}", _tileBorder, value);
                    _tileBorder = value;
                    OnNotifyPropertyChanged("TileBorder");
                    ImageWidth = Width - TileBorder*2;
                    ImageHeight = Height - TileBorder*2;
                }
            }
        }


        /// <summary>
        /// The position of this tile, relative to all the other tiles
        /// </summary>
        public Position Position
        {
            get { return _tile.Position; }
        }

        /// <summary>
        /// The position this tile will be in when it's in the right position to finish the puzzle
        /// </summary>
        public Position SolvedPosition
        {
            get { return _tile.SolvedPosition; }
        }

        /// <summary>
        /// The width of the image within the tile. 
        /// </summary>
        /// <remarks>
        /// This is never changed directly, it's changed as a side-effect of changing <see cref="TileBorder"/> or <see cref="Width"/></remarks>
        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                Logger.Debug("Tile {0} ImageWidth updated from {1} to {2}", this, _imageWidth, value);
                _imageWidth = value;
                OnNotifyPropertyChanged("ImageWidth");
            }
        }

        /// <summary>
        /// The height of the image within the tile. 
        /// </summary>
        /// <remarks>
        /// This is never changed directly, it's changed as a side-effect of changing <see cref="TileBorder"/> or <see cref="Height"/></remarks>
        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                Logger.Debug("Tile {0} ImageHeight updated from {1} to {2}", this, _imageHeight, value);
                _imageHeight = value;
                OnNotifyPropertyChanged("ImageHeight");
            }
        }


        /// <summary>
        /// The horizontal pixel offset, from the top left of the tile, where the image starts
        /// </summary>
        public double ImageLeft
        {
            get { return _imageLeft; }
            set
            {
                _imageLeft = value;
                OnNotifyPropertyChanged("ImageLeft");
            }
        }

        /// <summary>
        /// The vertical pixel offset, from the top left of the tile, where the image starts
        /// </summary>
        public double ImageTop
        {
            get { return _imageTop; }
            set
            {
                _imageTop = value;
                OnNotifyPropertyChanged("ImageTop");
            }
        }


        /// <summary>
        /// Determines whether the player tile should be visible (it's not visible if this is not a player tile)
        /// </summary>
        public Visibility PlayerTileVisibility
        {
            get { return _tile.IsPlayerTile ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Determines whether the tile image should should be visible (it's not visible if this is a player tile)
        /// </summary>
        public Visibility TileImageVisibility
        {
            get { return _tile.IsPlayerTile ? Visibility.Collapsed : Visibility.Visible; }
        }


        /// <summary>
        /// The brush to paint the image with (this is the image for the tile)
        /// </summary>
        public Brush TileBrush
        {
            get { return _tile.TileBrush; }
        }


        /// <summary>
        /// The brush to paint the image with when a player is using this tile (typically this is just a black square)
        /// </summary>
        public Brush PlayerBrush
        {
            get
            {
                // For now we'll ignore the player's preferred color, and just use a transparent brush, TODO fix this later when multiple players are available
                Color originalColor = _tile.PlayerBrush;
                originalColor.A = 0;
                return new SolidColorBrush(originalColor);
            }
        }

        /// <summary>
        /// Invoked when an animation on this tile starts.
        /// </summary>
        /// <remarks>
        /// This allows the <see cref="PuzzleViewModel"/> (see <see cref="PuzzleViewModel.Configure"/>) to keep track of all tile animations</remarks>
        public event EventHandler<EventArgs> TileAnimationStarted;

        /// <summary>
        /// Invoked when an animation on this tile completes.
        /// </summary>
        /// <remarks>
        /// This allows the <see cref="PuzzleViewModel"/> (see <see cref="PuzzleViewModel.Configure"/>) to keep track of all tile animations.  We don't want to
        /// declare a level finished until all the animations on tiles have been completed</remarks>
        public event EventHandler<EventArgs> TileAnimationCompleted;


        /// <summary>
        ///   Resyncs the <see cref="Top" /> and <see cref="Left" /> properties to the model, stored in <see cref="ModelLeft" /> and <see
        ///    cref="ModelTop" />
        /// </summary>
        /// <remarks>
        ///   This is the <see cref="INotifyPropertyChanged.PropertyChanged" /> event handler for the viewmodel to keep the position of the tiles in sync with the model when the model changes.
        /// </remarks>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void UpdatePositionBasedOnModelChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModelTop")
            {
                Logger.Debug("Resyncing Top with ModelTop for {0}", this);
                Top = ModelTop;
            }
            if (e.PropertyName == "ModelLeft")
            {
                Logger.Debug("Resyncing Left with ModelLeft for {0}", this);
                Left = ModelLeft;
            }
        }


        /// <summary>
        /// Invoked when the model has moved this tile from <see cref="TileMoveEventArgs.OldPosition"/> to <see cref="TileMoveEventArgs.NewPosition"/>
        /// </summary>
        /// <remarks>
        /// Once the model has moved the tile, we need to animate the tile moving from <see cref="TileMoveEventArgs.OldPosition"/> to <see cref="TileMoveEventArgs.NewPosition"/>
        /// within <paramref name="tileMoveEventArgs"/></remarks>
        /// <param name="sender">The <see cref="TileModel"/> object for this view model.  Unused.</param>
        /// <param name="tileMoveEventArgs">The details of the change</param>
        private void TileModelOnTileMoved(object sender, TileMoveEventArgs tileMoveEventArgs)
        {
            // On the start of a move, kick off the animation for the tile sliding
            if (!_tile.IsPlayerTile)
            {
                Logger.Info("TileModelOnTileMoving for non-player tile invoked");
                Storyboard sb = TileAnimator.CreateSlideAnimation(this, tileMoveEventArgs.NewPosition.X*Width,
                                                                  tileMoveEventArgs.NewPosition.Y*Height);
                sb.Children[0].Completed += (tile, args) =>
                                                {
                                                    Logger.Debug("Tile animation on {0} completed", tile);
                                                    // Resync with the model
                                                    OnNotifyPropertyChanged("ModelTop");
                                                    OnNotifyPropertyChanged("ModelLeft");

                                                    // Raise notification that we've finished running animations
                                                    SafeRaise.Raise(TileAnimationCompleted, this);
                                                };
                Logger.Info("Calling begin on tile animation for PlayerTile ({0})", this);
                sb.Begin();
                SafeRaise.Raise(TileAnimationStarted, this);
            }
            else
            {
                // No animation currently for the player tile, it just flicks across
                OnNotifyPropertyChanged("ModelTop");
                OnNotifyPropertyChanged("ModelLeft");
            }
        }


        /// <summary>
        /// Overrides default implementation to include the position of the tile
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}({1})", base.ToString(), Position);
        }
    }
}