using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Locima.SlidingBlock.Common;
using Locima.SlidingBlock.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using NLog;

namespace Locima.SlidingBlock
{

    /// <summary>
    /// Creates a crash report on a file in isolated storage when an uncaught exception is thrown; then when the app is started again the report can be mailed
    /// to me for analysis.
    /// </summary>
    /// <remarks>
    /// Credit to Andy Pennell for this, see <a href="http://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx"/>
    /// </remarks>
    public class LittleWatson
    {
        const string CrashReportFilename = "LittleWatson.txt";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    IOHelper.DeleteFile(CrashReportFilename, true);
                    Logger.Info("Creating \"{0}\" LittleWatson exception file", CrashReportFilename);
                    using (TextWriter output = new StreamWriter(store.CreateFile(CrashReportFilename)))
                    {
                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("Error writing LittleWatson report exception", e);
            }
        }


        internal static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(CrashReportFilename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(CrashReportFilename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        IOHelper.DeleteFile(CrashReportFilename, true);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show(LocalizationHelper.GetString("LittleWatson_Message"), 
                        LocalizationHelper.GetString("LittleWatson_Title"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask
                            {
                                To = "slidingblockcrash@locima.co.uk",
                                Subject = "SlidingBlock Crash Details",
                                Body = contents
                            };
                        IOHelper.DeleteFile(CrashReportFilename, true);
                        email.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("Error ocurred in LittleWatson", e);
            }
            finally
            {
                IOHelper.DeleteFile(CrashReportFilename, true);
            }
        }

        
    }
}
