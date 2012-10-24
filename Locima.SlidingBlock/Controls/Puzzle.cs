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

    /// <summary>
    /// This control represents the entire sliding block puzzle as it appears on <see cref="GamePage"/>.  It contains only instances of <see cref="SimpleTile"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is a specialisation of <see cref="Canvas"/>, which may or may not be a smart way to do this.  I used it because then the tiles could be easily animated based on the
    /// <see cref="Canvas.TopProperty"/> and <see cref="Canvas.LeftProperty"/> attached properties of each tile.</para>
    /// <para>
    /// This control shows off:
    /// <list type="number">
    /// <item><description>Creating a my own dependency properties, <see cref="PauseScreenProperty"/> and <see cref="PausedProperty"/> which are then data bound</description></item>
    /// <item><description>Creating a XAML-based property of the control, i.e. the pause screen, <see cref="PauseScreenProperty"/></description></item>
    /// <item><description>Dynamically creating sub-controls defined in XAML (<see cref="SimpleTile"/>) and adding them to the hierarchy</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class Puzzle : Canvas
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The pause screen is overlaid on top of the puzzle when <see cref="PausedProperty"/> is set to true
        /// </summary>
        public static readonly DependencyProperty PauseScreenProperty = DependencyProperty.Register("PauseScreen",
                                                                                                    typeof (FrameworkElement),
                                                                                                    typeof (Puzzle),
                                                                                                    null);

        /// <summary>
        /// If <c>true</c> then the pause screen is displayed and all taps are ignored
        /// </summary>
        public static readonly DependencyProperty PausedProperty = DependencyProperty.Register("Paused",
                                                                                               typeof (Boolean),
                                                                                               typeof (Puzzle),
                                                                                               new PropertyMetadata(
                                                                                                   false,
                                                                                                   PausedPropertyChangeCallback));
        
        /// <summary>
        /// The save game that the puzzle is currently playing.  I don't like this, the View essentially has a reference to the model, which is not right at all.
        /// </summary>
        /// <remarks>
        /// Backing field for <see cref="SaveGame"/>
        /// </remarks>
        private SaveGame _game;

        /// <summary>
        /// The set of pause screen controls
        /// </summary>
        /// <see cref="PauseScreen"/>
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


        /// <summary>
        /// The current game that's being represented by this control.  When this is set the whole control reconfigures itself.
        /// </summary>
        /// <remarks>
        /// TODO The puzzle shouldn't need the whole SaveGame instance, instead create individual DependencyProperties and bind them to the view model
        /// </remarks>
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


        /// <summary>
        /// Captures a <see cref="UIElement.Tap"/> event on the puzzle and, provided that the game isn't paused, passes the location of the tap to the view model (<see cref="PuzzleViewModel.MoveTileBasedOnTap"/>
        /// </summary>
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
        /// <see cref="TileViewModel"/>
        /// <see cref="SimpleTile"/>
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