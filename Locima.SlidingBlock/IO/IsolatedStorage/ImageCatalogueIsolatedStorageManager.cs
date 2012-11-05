using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Text;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{

    /// <summary>
    /// Manages the catalogue of images (mapping of Url source to local filename) in <see cref="IsolatedStorageSettings.ApplicationSettings"/>
    /// </summary>
    public class ImageCatalogueIsolatedStorageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The key in the <see cref="IsolatedStorageSettings.ApplicationSettings"/> used to store the catalogue 
        /// </summary>
        public static string ImageCatalogueKey = "ImageCatalogue";

        /// <summary>
        /// Gets the image catalogue from <see cref="IsolatedStorageSettings.ApplicationSettings"/>, or create a new one if one doesn't exist
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Adds a new image to the catalogue, returning the key to use to get it back later
        /// </summary>
        /// <remarks></remarks>
        /// If the <paramref name="uri"/> already exists in the catalogue, this call is ignored
        /// <param name="uri">The Uri to store in the catalogue, must not be null</param>
        /// <returns>The filename in isolated storage to retrieve the contents of the Uri from in future</returns>
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


        /// <summary>
        /// Serialises the image catalogue contents as a string
        /// </summary>
        /// <remarks>Used for debugging</remarks>
        /// <returns>A multi-line string</returns>
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