using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Text;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class ImageCatalogueIsolatedStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string ImageCatalogueKey = "ImageCatalogue";

        private static Dictionary<string, string> GetImageCatalogue()
        {
            Dictionary<string, string> imageCat;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(ImageCatalogueKey, out imageCat))
            {
                Logger.Info("Creating image catalogue in isolated storage settings using key {0}", ImageCatalogueKey);
                imageCat = new Dictionary<string, string>();
                IsolatedStorageSettings.ApplicationSettings[ImageCatalogueKey] = imageCat;
            }
            else
            {
                Logger.Debug("Found image catalogue");
                if (Logger.IsDebugEnabled) Logger.Debug(ImageCatalogueToString());
            }
            return imageCat;
        }


        public static string AddToCatalogue(Uri uri)
        {
            Dictionary<string, string> imageCat = GetImageCatalogue();

            string filename;
            string key = uri.ToString();
            if (imageCat.TryGetValue(key, out filename))
            {
                Logger.Debug("Returning existing filename mapping if {0} to {1}", key, filename);
            }
            else
            {
                filename = Guid.NewGuid().ToString();
                imageCat[key] = filename;
                Logger.Debug("Added new image catalogue mapping of {0} to {1}", key, filename);
            }
            filename = string.Format("{0}{1}{2}", ImageIsolatedStorageManager.ImageDirectory, IOHelper.PathSeparator, filename);
            Logger.Debug("Returning filename {0}", filename);
            return filename;
        }


        private static string ImageCatalogueToString()
        {
            StringBuilder s = new StringBuilder("Image Catalogue Contents:");

            Dictionary<string, string> imageCat = GetImageCatalogue();

            foreach (KeyValuePair<string, string> imageCatalogueEntry in imageCat)
            {
                s.AppendFormat("\n\t{0} => {1}", imageCatalogueEntry.Key, imageCatalogueEntry.Value);
            }
            s.AppendFormat("Total {0} entries", imageCat.Count);
            return s.ToString();
        }
    }
}