using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Locima.SlidingBlock.Common;
using NLog;

namespace Locima.SlidingBlock.IO.IsolatedStorage
{
    public class IOHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public const string PathSeparator = "\\";

        public void DownloadStream(Uri uri, string isolatedStoragePath)
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream streamToWriteTo = new IsolatedStorageFileStream(isolatedStoragePath,
                                                                                      FileMode.Create, store);
            DownloadStream(uri, streamToWriteTo);
        }


        public static IAsyncResult DownloadStream(Uri uri, Stream streamToWriteTo)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.AllowReadStreamBuffering = true;
            IAsyncResult result = request.BeginGetResponse(DownloadStreamWorker,
                                                           new DownloadRequest
                                                               {
                                                                   SourceUri = uri,
                                                                   Request = request,
                                                                   OutputStream = streamToWriteTo
                                                               });
            return result;
        }


        private static void DownloadStreamWorker(IAsyncResult result)
        {
            // TODO Tidy this up, allow callbacks on incremental increases in download progress
            DownloadRequest req = (DownloadRequest) result.AsyncState;
            HttpWebRequest request = req.Request;
            Stream streamToWriteTo = req.OutputStream;
            try
            {
                HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result);
                Stream inputStream = response.GetResponseStream();

                byte[] data = new byte[256*1024];
                int bytesRead;

                long totalValue = response.ContentLength;
                while ((bytesRead = inputStream.Read(data, 0, data.Length)) > 0)
                {
                    if (streamToWriteTo.Length != 0)
                        Logger.Info((int) ((streamToWriteTo.Length*100)/totalValue));
                    streamToWriteTo.Write(data, 0, bytesRead);
                }
            }
            finally
            {
                streamToWriteTo.Close();
            }
            Logger.Info("Completed download of {0}", req.SourceUri);
        }


        public void SaveStream(string filename, Stream stream)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream s = store.CreateFile(filename);

                int count;
                byte[] buffer = new byte[512000];
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    s.Write(buffer, 0, count);
                } while (count > 0);
            }
        }

        public static void CopyStream(Stream inStream, Stream outStream)
        {
            int count;
            byte[] buffer = new byte[512000];
            do
            {
                count = inStream.Read(buffer, 0, buffer.Length);
                outStream.Write(buffer, 0, count);
            } while (count > 0);
        }


        #region Nested type: DownloadRequest

        private struct DownloadRequest
        {
            public Uri SourceUri { get; set; }
            public HttpWebRequest Request { get; set; }
            public Stream OutputStream { get; set; }
        }

        #endregion


        /// <summary>
        /// Creates the directory named within isolated storage
        /// </summary>
        /// <param name="directoryName">The name of the directory to create, must be a valid directory name</param>
        /// <param name="store"></param>
        /// <returns>True if the directory was created, false if it already existed</returns>
        public static bool EnsureDirectory(string directoryName, IsolatedStorageFile store)
        {
            bool created;
            if (store.GetDirectoryNames(directoryName).Length == 0)
            {
                Logger.Info("Creating new directory {0}", directoryName);
                store.CreateDirectory(directoryName);
                created = true;
            }
            else
            {
                Logger.Debug("Confirmed {0} directory already exists", directoryName);
                created = false;
            }
            return created;
        }


        /// <summary>
        /// Creates a directory if it doesn't exist
        /// </summary>
        /// <param name="directoryName">The name of the directory to create</param>
        /// <returns><c>true</c> is the directory had to be created, <c>false</c> if it already existed</returns>
        public static bool EnsureDirectory(string directoryName)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return EnsureDirectory(directoryName, store);
            }
        }


        /// <summary>
        /// Returns the full path names of files within the direcctory specified by <paramref name="directoryName"/>
        /// </summary>
        /// <param name="directoryName">The name of the directory to search within</param>
        /// <param name="store">The store to search</param>
        /// <returns>A (potentially empty) list of files</returns>
        public static List<string> GetFileNames(string directoryName, IsolatedStorageFile store)
        {
            string[] fileNames = store.GetFileNames(string.Format("{0}{1}*", directoryName, PathSeparator));
            List<string> fullFileNames = new List<string>(fileNames.Length);
            fullFileNames.AddRange(
                fileNames.Select(fileName => string.Format("{0}{1}{2}", directoryName, PathSeparator, fileName)));
            return fullFileNames;
        }

                /// <summary>
        /// Saves an object to isolated storage, creating or overwriting if necessary, using the default <see cref="DataContractSerializer"/>
        /// </summary>
        /// <typeparam name="T">The type of object to be saved</typeparam>
        /// <param name="obj">The object to save</param>
        public static void SaveObject<T>(T obj) where T : class, IPersistedObject
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        SaveObject(obj, store);
                    }
                }





        /// <summary>
        /// Saves an object to isolated storage, creating or overwriting if necessary, using the default <see cref="DataContractSerializer"/>
        /// </summary>
        /// <typeparam name="T">The type of object to be saved</typeparam>
        /// <param name="obj">The object to save</param>
        /// <param name="store">The store to save data to</param>
        public static void SaveObject<T>(T obj, IsolatedStorageFile store) where T : class, IPersistedObject
        {
            if (obj == null) throw new ArgumentException("obj");
            if (string.IsNullOrEmpty(obj.Id))
                throw new InvalidStateException("Object passed to SaveObject has no Filename value set: {0}", obj);
            if (Logger.IsInfoEnabled)
                Logger.Info("{0} file {1}",
                            store.FileExists(obj.Id) ? "Overwriting existing" : "Creating new",
                            obj.Id);

            using (
                IsolatedStorageFileStream fileStream = store.OpenFile(obj.Id, FileMode.Create, FileAccess.Write))
            {
                Logger.Debug("Opened file for writing, now writing data for {0}", obj.Id);

                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(fileStream, obj);
                Logger.Debug("Wrote data successfully for {0}", obj.Id);

                // Update puzzle.SaveDateTime with the time the file was written
                obj.LastUpdate = store.GetLastWriteTime(obj.Id);
            }
        }


        /// <summary>
        /// Loads an object from Isolated Storage
        /// </summary>
        /// <remarks>
        /// Uses <See cref="DataContractSerializer"/> to load data from isolated storage and sets <see cref="IPersistedObject.LastUpdate"/> 
        /// and <see cref="IPersistedObject.Id"/> to <paramref name="filename"/></remarks>
        /// <typeparam name="T">The type of object to deserialise to</typeparam>
        /// <param name="store">The isolated storage file to read from</param>
        /// <param name="filename">The filename to load from (must not be null)</param>
        /// <returns>The loaded object</returns>
        public static T LoadObject<T>(IsolatedStorageFile store, string filename) where T : class, IPersistedObject
        {
            Logger.Info("Loading file {0} and deserialising to instance of {1}", filename, typeof(T).FullName);

            if (store == null) throw new ArgumentNullException("store");
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException("filename");
            if (!store.FileExists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            using (IsolatedStorageFileStream fileStream = store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                T deserialisedObject = (T)serializer.ReadObject(fileStream);

                // Set the non-persisted filename property
                deserialisedObject.Id = filename;
                deserialisedObject.LastUpdate = store.GetLastWriteTime(filename);
                Logger.Info("Successfully read {0}, deserialised to {1} and set last update {2}", filename, deserialisedObject, deserialisedObject.LastUpdate);

                return deserialisedObject;
            }
        }


        /// <summary>
        /// Loads an object from Isolated Storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename to load from (must not be null)</param>
        /// <returns>The loaded object</returns>
        public static T LoadObject<T>(string filename) where T : class, IPersistedObject
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return LoadObject<T>(store, filename);
            }
        }


        public static bool DeleteFile(string filename)
        {
            return DeleteFile(filename, false);
        }


        /// <summary>
        /// Deletes a file from Isolated Storage
        /// </summary>
        /// <param name="filename">The name of the file to delete</param>
        /// <param name="suppressException">If true, all exceptions will be swallowed (but logged)</param>
        /// <returns><c>true</c> if the file was successfully deleted, <c>false</c> otherwise</returns>
        public static bool DeleteFile(string filename, bool suppressException)
        {
            bool result;
            try
            {
                Logger.Debug("Deleting IsolatedStorageFile {0}", filename);
                if (string.IsNullOrEmpty(filename))
                {
                    Logger.Error("Null or empty filename passed to DeleteFile.  Returning immediately");
                    result = false;
                }
                else
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(filename))
                        {
                            store.DeleteFile(filename);
                            Logger.Debug("File {0} deleted successfully", filename);
                            result = true;
                        }
                        else
                        {
                            Logger.Error("Call to DeleteGame for file {0} invoked, but no such file exists", filename);
                            result = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("Exception deleting file", e);
                if (!suppressException) throw;
                result = true;
            }
            return result;
        }

    }
}