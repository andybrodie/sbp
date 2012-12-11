using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.Persistence;
using Locima.SlidingBlock.ViewModel;
using NLog;

namespace Locima.SlidingBlock.Controls
{

    /// <summary>
    /// This control represents the entire sliding block puzzle as it appears on <see cref="GamePage"/>.  It contains only instances of <see cref="TileControl"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is a specialisation of <see cref="Canvas"/>, which may or may not be a smart way to do this.  I used it because then the tiles could be easily animated based on the
    /// <see cref="Canvas.TopProperty"/> and <see cref="Canvas.LeftProperty"/> attached properties of each tile.</para>
    /// <para>
    /// This control shows off:
    /// <list type="number">
    /// <item><description>Creating my own dependency properties, <see cref="PauseScreenProperty"/> and <see cref="StartScreenProperty"/> and <see cref="GameStateProperty"/> which are then data bound</description></item>
    /// <item><description>Creating a XAML-based property of the control, i.e. the pause screen, <see cref="PauseScreenProperty"/></description></item>
    /// <item><description>Dynamically creating sub-controls defined in XAML (<see cref="TileControl"/>) and adding them to the hierarchy</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class Puzzle : Canvas
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The pause screen is overlaid on top of the puzzle when <see cref="GameStateProperty"/> is set to <see cref="GameStates.Paused"/>
        /// </summary>
        public static readonly DependencyProperty PauseScreenProperty = DependencyProperty.Register("PauseScreen",
                                                                                                    typeof(FrameworkElement),
                                                                                                    typeof(Puzzle),
                                                                                                    null);

        /// <summary>
        /// The complete screen is overlaid on top of the puzzle when the level is completed
        /// </summary>
        public static readonly DependencyProperty StartScreenProperty = DependencyProperty.Register("StartScreen",
                                                                                                    typeof(FrameworkElement),
                                                                                                    typeof(Puzzle),
                                                                                                    null);

        /// <summary>
        /// The current state of the game, as determined by the set of valid values for <see cref="GameStates"/>
        /// </summary>
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register
            ("GameState",
             typeof (GameStates),
             typeof (Puzzle),
             new PropertyMetadata(GameStates.NotStarted, GameStatePropertyChangeCallback));
        
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
        /// The set of controls overlaid on the puzzle when the level is complete
        /// </summary>
        public FrameworkElement StartScreen { get; set; }

        /// <summary>
        ///   Whether the puzzle has been started, is running, is paused or is completed
        /// </summary>
        public GameStates GameState
        {
            get { return (GameStates) GetValue(GameStateProperty); }
            set { SetValue(GameStateProperty, value); }
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
            ViewModel.Initialise(_game);

            // Wipe any existing tiles, reset the pause and start screens then recalculate and re-add all the tiles
            Children.Clear();
            Children.Add(PauseScreen);
            Children.Add(StartScreen);
            PauseScreen.Visibility = GameState == GameStates.Paused ? Visibility.Visible : Visibility.Collapsed;
            StartScreen.Visibility = GameState == GameStates.NotStarted ? Visibility.Visible : Visibility.Collapsed;
            
            // Ensure that the pause and start panels sits over the top of everything else
            PauseScreen.SetValue(ZIndexProperty, 100);
            StartScreen.SetValue(ZIndexProperty, 100);

            // Create the tile instances dynamically, binding all their properties to their viewmodel and add them to the puzzle canvas
            CreateTileControls();
        }


        /// <summary>
        /// Invoked when the dependency property <see cref="GameState"/> is set.  This handler then sets the <see cref="Visibility"/> property on the <see cref="PauseScreen"/>
        /// and <see cref="StartScreen"/> controls.
        /// </summary>
        /// <param name="dependencyObject">The puzzle (required because this method must be static to be used with the static declaration of <see cref="GameStateProperty"/></param>
        /// <param name="dependencyPropertyChangedEventArgs"><see cref="DependencyPropertyChangedEventArgs.NewValue"/> used to detect which state we're in</param>
        private static void GameStatePropertyChangeCallback(DependencyObject dependencyObject,
                                                         DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            GameStates newState = (GameStates) dependencyPropertyChangedEventArgs.NewValue;
            Logger.Info("New game state: {0}", newState);
            Puzzle puzzle = (Puzzle) dependencyObject;

            puzzle.PauseScreen.Visibility = newState==GameStates.Paused ? Visibility.Visible : Visibility.Collapsed;
            puzzle.StartScreen.Visibility = newState==GameStates.NotStarted ? Visibility.Visible : Visibility.Collapsed;
        }


        /// <summary>
        /// Event handler for the <see cref="FrameworkElement.SizeChanged"/> event on this control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Logger.Info("OnSizeChanged fired: {0} to {1}, setting ViewModel.PuzzleArea", e.PreviousSize, e.NewSize);
            ReconfigureScreens(new Size(ActualWidth, ActualHeight));
        }


        /// <summary>
        /// Ensure the <see cref="StartScreen"/> and <see cref="PauseScreen"/> panels are the right size and will appear on top of everything else
        /// </summary>
        /// <param name="newSize"></param>
        /// <remarks>
        /// Used when the puzzle is resized (<see cref="OnSizeChanged"/> and <see cref="OnLayoutUpdated"/></remarks>
        private void ReconfigureScreens(Size newSize)
        {
            Logger.Debug("Updating puzzle area from {0} to {1}", ViewModel.PuzzleArea, newSize);
            ViewModel.PuzzleArea = newSize;

            // Set the pause and start screen sizes to fill the puzzle area
            PauseScreen.Width = ActualWidth;
            PauseScreen.Height = ActualHeight;
            StartScreen.Width = ActualWidth;
            StartScreen.Height = ActualHeight;

            // Removed ZIndex set and put in the tile control reset
        }


        /// <summary>
        /// Event handler for the <see cref="FrameworkElement.LayoutUpdated"/> event on this control.
        /// </summary>
        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
//            Logger.Debug("OnLayoutUpdated fired");
            Size currentSize = ViewModel.PuzzleArea;

            // Only take action is the size of the puzzle area has changed by more than 1 pixel
            if ((Math.Abs(currentSize.Width - ActualWidth) > 1) || (Math.Abs(currentSize.Height - ActualHeight) > 1))
            {
                Size newSize = new Size(ActualWidth, ActualHeight);
                Logger.Info("Grid layout changed from {0} to {1}, notifying puzzle viewmodel", currentSize, newSize);
                ReconfigureScreens(newSize);

            }
        }


        /// <summary>
        /// Captures a <see cref="UIElement.Tap"/> event on the puzzle and, provided that the game isn't paused, passes the location of the tap to the view model (<see cref="PuzzleViewModel.MoveTileBasedOnTap"/>
        /// </summary>
        private void PuzzleTap(object sender, GestureEventArgs e)
        {
            switch (GameState)
            {
                case GameStates.Completed:
                    // TODO Fill this in
                    break;
                case GameStates.NotStarted:
                    Logger.Info("Starting the level");
                    ViewModel.StartGameCommand.Execute(this);
                    break;
                case GameStates.Running:
                    ViewModel.MoveTileBasedOnTap(e.GetPosition(this));
                    break;
                case GameStates.Paused:
                    Logger.Debug("Ignoring tap on the puzzle as the game state is {0}", GameState);
                    break;
                default:
                    throw new InvalidStateException("Unexpeccted GameState value {0}", GameState);
            }
            e.Handled = true;
        }


        /// <summary>
        /// Hook up events and tells the <see cref="ViewModel"/> that its ready to calculate its maximum size.
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

            ViewModel.PuzzleModelPuzzleResized(true);
            Logger.Info("Puzzle control initialise exit");
        }


        /// <summary>
        ///   Creates all the child controls of this canvas specialisation, namely the tiles that will slide around
        /// </summary>
        /// <see cref="TileViewModel"/>
        /// <see cref="TileControl"/>
        private void CreateTileControls()
        {
            Logger.Debug(
                "Dynamically creating {0} TileControl controls and binding them to the TileViewModel instances in the PuzzleViewModel",
                ViewModel.Tiles.Count);
            foreach (TileViewModel tvm in ViewModel.Tiles)
            {
                Logger.Info("Creating tile control for {0} (solved Position ({1},{2}))", tvm.Position, tvm.SolvedPosition.X, tvm.SolvedPosition.Y);
                TileControl tile = new TileControl {DataContext = tvm};
                
                Logger.Debug("Binding TileControl Width, Height, Top and Left properties to the TileViewModel and adding to the Puzzles Children collection");
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
                Logger.Debug("Current tile values are: Width({0}) Height({1}) Top({2}) Left({3})", tile.ActualWidth, tile.ActualHeight, GetTop(tile), GetLeft(tile));
            }
            Logger.Debug("CreateTileControls Exit");
        }
    }
}