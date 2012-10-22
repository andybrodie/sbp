﻿using System;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Locima.SlidingBlock.Controls;
using Locima.SlidingBlock.IO;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NLog;

namespace Locima.SlidingBlock
{
    
    public partial class App : Application
    {
        private static Logger Logger;

        public const string SuppressBackQueryParameterName = "SuppressBack";

        public bool IsLightTheme
        {
            get
            {
                Visibility vis = (Visibility) Current.Resources["PhoneDarkThemeVisibility"];
                return vis != Visibility.Visible;
            }
        }

        public Color ThemeColor
        {
            get { return (Color) Current.Resources["PhoneAccentColor"]; }
        }


        /// <summary>
        ///   Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                MetroGridHelper.IsVisible = true;
            }
        }

        /// <summary>
        ///   Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns> The root frame of the Phone Application. </returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            Logger.Info("Lauching application");
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            Logger.Info("Activating Application from {0} state", e.IsApplicationInstancePreserved ? "dormant" : "tombstoned");
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Logger.Info("Deactiving Application");     
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Logger.Info("Closing Application");
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Logger.FatalException(string.Format("Attempted navigation to {0} failed.  Application crashing out.", e.Uri),e.Exception);
            LittleWatson.ReportException(e.Exception, e.Uri == null ? String.Empty : e.Uri.ToString());

            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Logger.FatalException("Unhandled exception encountered.  Application crashing out.", e.ExceptionObject);
            LittleWatson.ReportException(e.ExceptionObject, String.Empty);
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool _phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
            {
                Logger.Debug("Aborting repeated call of InitializePhoneApplication");
                return;
            }

            Logger = LogManager.GetCurrentClassLogger();

            // Make sure all required directories are created
            SaveGameStorageManager.Instance.Initialise();
            PlayerStorageManager.Instance.Initialise();
            ImageStorageManager.Instance.Initialise();
            HighScoresStorageManager.Instance.Initialise();

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();

            RootFrame.Navigated += RootFrameOnNavigated;

            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;            

            // Ensure we don't initialize again
            _phoneApplicationInitialized = true;

        }


        private void RootFrameOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            Logger.Info("Navigated to {0} in mode {1}", navigationEventArgs.Uri, navigationEventArgs.NavigationMode);

            // Edit the backstack if the Uri includes a request to eliminate the previous page from the backstack
            if (navigationEventArgs.NavigationMode == NavigationMode.New)
            {
                HandleBackstackSuppression(navigationEventArgs.Uri);
            }
            else
            {
                Logger.Debug("Not checking for backstack suppression because navigation mode is {0}",
                             navigationEventArgs.NavigationMode);
            }

            if (Logger.IsDebugEnabled)
            {
                StringBuilder backStackTrace = new StringBuilder("Backstack now stands as follows:\n");
                foreach (JournalEntry je in RootFrame.BackStack)
                {
                    backStackTrace.AppendFormat("\t{0}\n", je.Source);
                }
                backStackTrace.AppendFormat("Total {0} entries", RootFrame.BackStack.Count());
                Logger.Debug(backStackTrace);
            }

        }


        /// <summary>
        /// Invoked when a new page is navigated to to suppress the most recent <see cref="PhoneApplicationFrame.BackStack"/> entry if 
        /// a query parameter <see cref="SuppressBackQueryParameterName"/> is specified with the value <c>True</c>.
        /// </summary>
        /// <remarks>
        /// By adding this method to the Application XAML code-behind, we can have this as a feature available to all pages within the application,
        /// rather than adding it page at a time.</remarks>
        /// <param name="uri">The URI of the page navigated to</param>
        /// <returns>True if a back entry was removed</returns>
        private bool HandleBackstackSuppression(Uri uri)
        {
            Boolean suppressed;
            Regex expression = new Regex(string.Format(@".*{0}=(\w+).*$", SuppressBackQueryParameterName));
            Match match = expression.Match(uri.ToString());
            string suppressBackValue = match.Groups[1].Value;

            if (Boolean.TrueString.Equals(suppressBackValue))
            {
                JournalEntry topEntry = RootFrame.BackStack.FirstOrDefault();
                Logger.Info("Suppressing top entry in backstack: {0}", topEntry == null ? "None" : topEntry.Source.ToString());
                RootFrame.RemoveBackEntry();
                suppressed = true;
            } else
            {
                Logger.Debug("No {0} parameter found, defaulting to false", SuppressBackQueryParameterName);
                suppressed = false;
            }
            return suppressed;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // If we don't have a current player, then redirect

            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}