using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using NLog;

namespace Locima.SlidingBlock.Controls
{
    public class Puzzle : Canvas
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty PauseScreenProperty = DependencyProperty.Register("PauseScreen",
                                                                                                    typeof (FrameworkElement),
                                                                                                    typeof (Puzzle),
                                                                                                    null);

        public static readonly DependencyProperty PausedProperty = DependencyProperty.Register("Paused",
                                                                                               typeof (Boolean),
                                                                                               typeof (Puzzle),
                                                                                               new PropertyMetadata(
                                                                                                   false,
                                                                                                   PausedPropertyChangeCallback));
        
        private SaveGame _game;

        public FrameworkElement PauseScreen { get; set; }

        /// <summary>
        ///   Whether the puzzle is paused or not
        /// </summary>
        public bool Paused
        {
            get { return (bool) GetValue(PausedProperty); }
            set { SetValue(PausedProperty, value); }
        }


        /// <summary>
        ///   Provides a type-safe accessor for <see cref="FrameworkElement.DataContext" />
        /// </summary>
        private PuzzleViewModel ViewModel
        {
            get { return DataContext as PuzzleViewModel; }
        }


        // TODO The puzzle shouldn't need the whole SaveGame instance, instead create individual DependencyProperties and bind them to the view model
        public SaveGame Game
        {
            get { return _game; }
            set
            {
                _game = value;
                ConfigureControlWithNewSaveGame();
            }
        }

        /// <summary>
        /// Invoked by the <see cref="Puzzle.Game"/> setter, this reconfigures the control (and all dependent models and viewmodels) around the new save game's <see cref="SaveGame.CurrentLevel"/>
        /// attribute.
        /// </summary>
        private void ConfigureControlWithNewSaveGame()
        {
            ViewModel.Configure(_game);
            ResetChildControls();
        }


        private void ResetChildControls()
        {
            Children.Clear();

            PauseScreen.Visibility = Visibility.Collapsed;
            Children.Add(PauseScreen);

            // Create the tile instances dynamically, binding all their properties to their viewmodel and add them to the puzzle canvas
            CreateTileControls();
        }


        /// <summary>
        /// Invoked when the dependency property <see cref="Paused"/> is set, this handler then sets the <see cref="Visibility"/> property on the <see cref="PauseScreen"/> control
        /// to hide the puzzle.
        /// </summary>
        /// <param name="dependencyObject">The puzzle (required because this method must be static to be used with the static declaration of <see cref="PausedProperty"/></param>
        /// <param name="dependencyPropertyChangedEventArgs"><see cref="DependencyPropertyChangedEventArgs.NewValue"/> used to detect whether we're paused or not</param>
        private static void PausedPropertyChangeCallback(DependencyObject dependencyObject,
                                                         DependencyPropertyChangedEventArgs
                                                             dependencyPropertyChangedEventArgs)
        {
            bool isPaused = (bool) dependencyPropertyChangedEventArgs.NewValue;
            Logger.Info("Paused: " + isPaused);
            Puzzle puzzle = (Puzzle) dependencyObject;
            puzzle.PauseScreen.Visibility = isPaused ? Visibility.Visible : Visibility.Collapsed;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Logger.Info("OnSizeChanged fired: {0} to {1}, setting ViewModel.PuzzleArea", e.PreviousSize, e.NewSize);
            ViewModel.PuzzleArea = new Size(ActualWidth, ActualHeight);
            // Set the pause screen size
            PauseScreen.Width = ActualWidth;
            PauseScreen.Height = ActualHeight;
            // Ensure that the pause rectangle sits over the top of everything else
            PauseScreen.SetValue(ZIndexProperty, 100);

        }


        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
//            Logger.Debug("OnLayoutUpdated fired");
            Size currentSize = ViewModel.PuzzleArea;

            if ((Math.Abs(currentSize.Width - ActualWidth) > 1) || (Math.Abs(currentSize.Height - ActualHeight) > 1))
            {
                Size newSize = new Size(ActualWidth, ActualHeight);
                Logger.Info("Grid layout changed from {0} to {1}, notifying puzzle viewmodel", currentSize, newSize);
                ViewModel.PuzzleArea = newSize;

                // Set the pause screen size
                PauseScreen.Width = ActualWidth;
                PauseScreen.Height = ActualHeight;
                // Ensure that the pause rectangle sits over the top of everything else
                PauseScreen.SetValue(ZIndexProperty, 100);
            }
        }


        private void PuzzleTap(object sender, GestureEventArgs e)
        {
            if (Paused)
            {
                Logger.Debug("Ignoring tap on the puzzle as the game is paused");
            }
            else
            {
                ViewModel.MoveTileBasedOnTap(e.GetPosition(this));
            }
            e.Handled = true;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Initialise()
        {
            Logger.Debug("Puzzle control initialise entry");
            if (ViewModel == null)
            {
                Logger.Debug("Aborting Puzzle Loaded handler because ViewModel property is null");
                return;
            }

//            Background = new SolidColorBrush(Color.FromArgb(128,128,128,128));  // Handy for debugging and ensuring that the size of the puzzle is what we think it is

            // Hook up event handlers for any changes to the grid layout, this may impact the size of our Puzzle control and thus the visuals of it
            SizeChanged -= OnSizeChanged;
            SizeChanged += OnSizeChanged;
            LayoutUpdated -= OnLayoutUpdated;
            LayoutUpdated -= OnLayoutUpdated;

            // Hook up events from the user to respond to, currently only detecting taps
            Tap -= PuzzleTap;
            Tap += PuzzleTap;

            ViewModel.PuzzleModelPuzzleResized();
            Logger.Info("Puzzle control initialise exit");
        }


        /// <summary>
        ///   Creates all the child controls of this canvas specialisation, namely the tiles that will slide around
        /// </summary>
        private void CreateTileControls()
        {
            Logger.Debug(
                "Dynamically creating {0} SimpleTile controls and binding them to the TileViewModel instances in the PuzzleViewModel",
                ViewModel.Tiles.Count);
            foreach (TileViewModel tvm in ViewModel.Tiles)
            {
                Logger.Info("Creating tile control for {0} (solved Position {1}", tvm.Position, tvm.SolvedPosition);
                SimpleTile tile = new SimpleTile {DataContext = tvm};
                
                Logger.Debug("Binding SimpleTile Width, Height, Top and Left properties to the TileViewModel and adding to the Puzzles Children collection");
                tile.SetBinding(WidthProperty, new Binding
                    {
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("Width")
                    });

                tile.SetBinding(HeightProperty, new Binding
                    {
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("Height")
                    });

                tile.SetBinding(LeftProperty, new Binding
                    {
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("Left")
                    });

                tile.SetBinding(TopProperty, new Binding
                    {
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("Top")
                    });

                Children.Add(tile);
                Logger.Debug("Current tile values are: Width({0}) Height({1}) Top({2}) Left5({3})", tile.ActualWidth, tile.ActualHeight, GetTop(tile), GetLeft(tile));
            }
            Logger.Debug("CreateTileControls Exit");
        }
    }
}