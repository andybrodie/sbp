using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Model;
using NLog;

namespace Locima.SlidingBlock.ViewModel
{
    public class TileViewModel : DependencyViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof (double),
                                                                                            typeof (TileViewModel),
                                                                                            new PropertyMetadata(0.00));

        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof (double),
                                                                                             typeof (TileViewModel),
                                                                                             new PropertyMetadata(0.00));

        private double _height;
        private double _imageHeight;
        private double _imageWidth;
        private double _imageX;
        private double _imageY;
        private double _tileBorder;
        private double _width;

        public event EventHandler<EventArgs> TileAnimationStarted;
        public event EventHandler<EventArgs> TileAnimationCompleted;

        public TileViewModel(TileModel tileModel)
        {
            _tile = tileModel;
            PropertyChanged += UpdatePositionBasedOnModelChange;
            // Detects changes to the X and Y position of the tile from the puzzle (relative to the other tiles, e.g. 0,0; 0,1; 2,2; etc. (not to be confused with Top and Left which are pixel offsets)
            tileModel.TileMoved += TileModelOnTileMoved;
        }

        /// <summary>
        /// Where the tile should be if synced to the model (i.e. not taking in to account animations)
        /// </summary>
        public double ModelTop
        {
            get { return Position.Y*Height; }
        }

        /// <summary>
        /// Where the tile should be if synced to the model (i.e. not taking in to account animations)
        /// </summary>
        public double ModelLeft
        {
            get { return Position.X*Width; }
        }

        /// <summary>
        /// The pixel width of this tile
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                Logger.Debug("Tile width updated from {0} to {1}", _width, value);
                _width = value;
                OnNotifyPropertyChanged("Width");
                OnNotifyPropertyChanged("ModelLeft");
                ImageWidth = Width - TileBorder*2;
            }
        }

        /// <summary>
        /// The pixel height of this tile
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                Logger.Debug("Tile height updated from {0} to {1}", _height, value);
                _height = value;
                OnNotifyPropertyChanged("Height");
                OnNotifyPropertyChanged("ModelTop");
                ImageHeight = Height - TileBorder*2;
            }
        }


        /// <summary>
        ///   SimpleTile's binds its <see cref="Canvas.TopProperty" /> proprerty to this one to determine where on the screen the tile should sit vertically.
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
        ///   SimpleTile's binds its <see cref="Canvas.TopProperty" /> proprerty to this one to determine where on the screen the tile should sit vertically.
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

        public double TileBorder
        {
            get { return _tileBorder; }
            set
            {
                Logger.Info("Updating TileBorder from {0} to {1}", _tileBorder, value);
                _tileBorder = value;
                OnNotifyPropertyChanged("TileBorder");
                ImageWidth = Width - TileBorder*2;
                ImageHeight = Height - TileBorder*2;
            }
        }


        public Position Position
        {
            get { return _tile.Position; }
        }

        public Position SolvedPosition
        {
            get { return _tile.SolvedPosition; }
        }

        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                OnNotifyPropertyChanged("ImageWidth");
            }
        }

        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                OnNotifyPropertyChanged("ImageHeight");
            }
        }

        public double ImageX
        {
            get { return _imageX; }
            set
            {
                _imageX = value;
                OnNotifyPropertyChanged("ImageX");
            }
        }

        public double ImageY
        {
            get { return _imageY; }
            set
            {
                _imageY = value;
                OnNotifyPropertyChanged("ImageY");
            }
        }

        public Visibility PlayerTileVisibility
        {
            get { return _tile.IsPlayerTile ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility TileImageVisibility
        {
            get { return _tile.IsPlayerTile ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Brush TileBrush
        {
            get { return _tile.TileBrush; }
        }


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


        private readonly TileModel _tile;

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
                Logger.Info("Tile animation for PlayerTile kicked off");
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