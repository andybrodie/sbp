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
    /// <summary>
    ///   Various helper utility for IO operations on Isolated Storage (the local storage area on the phone)
    /// </summary>
    public class IOHelper
    {
        /// <summary>
        ///   The strings that separates directories within a path
        /// </summary>
        public const string PathSeparator = "\\";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   Downloads the file located at <paramref name="uri" /> and saves it to the <paramref name="isolatedStoragePath" />.  Any existing file is overwritten.
        /// </summary>
        /// <remarks>
        ///   Delegates to <see cref="DownloadStream(System.Uri,Stream)" />
        /// </remarks>
        /// <param name="uri"> Download source, must not be null </param>
        /// <param name="isolatedStoragePath"> Download destination, must not be null </param>
        /// <returns> Response from <see cref="DownloadStream(System.Uri,Stream)" /> </returns>
        public IAsyncResult DownloadStream(Uri uri, string isolatedStoragePath)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (string.IsNullOrEmpty(isolatedStoragePath)) throw new ArgumentNullException("isolatedStoragePath");
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream streamToWriteTo = new IsolatedStorageFileStream(isolatedStoragePath,
                                                                                      FileMode.Create, store);
            return DownloadStream(uri, streamToWriteTo);
        }


        /// <summary>
        ///   Downloads the stream from <paramref name="uri" /> and writes it to <paramref name="streamToWriteTo" />
        /// </summary>
        /// <remarks>
        ///   Uses <see cref="HttpWebRequest" /> to download the file.  See <see cref="DownloadStreamWorker" />.  For parameters of download, see <see
        ///    cref="DownloadRequest" />
        /// </remarks>
        /// <param name="uri"> Download source, must not be null </param>
        /// <param name="streamToWriteTo"> The stream to write the downloaded data to, must not be null </param>
        /// <returns> Response object from <see cref="HttpWebRequest.BeginGetResponse" /> </returns>
        public static IAsyncResult DownloadStream(Uri uri, Stream streamToWriteTo)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.AllowReadStreamBuffering = true;
            IAsyncResult result = request.BeginGetResponse(DownloadStreamWorker,
                                                           new DownloadRequest
                                                               {
                                                                   SourceUri = uri,
                                                                   Request = request,
                                                                   OutputStream = streamToWriteTo,
                                                                   Buffersize = 256,
                                                                   Callback =
                                                                       (dlReq, totalDownloaded, totalSize) =>
                                                                       Logger.Debug(
                                                                           "Downloaded {0} / {1} bytes of {2}",
                                                                           totalDownloaded, totalSize, dlReq.SourceUri)
                                                               });
            return result;
        }


        /// <summary>
        ///   Used with <see cref="DownloadStream(System.Uri,Stream)" /> to download a file to isolated storage, using <see
        ///    cref="HttpWebRequest.BeginGetResponse" />
        /// </summary>
        /// <param name="result"> The object encapsulating the request to download </param>
        private static void DownloadStreamWorker(IAsyncResult result)
        {
            DownloadRequest req = (DownloadRequest) result.AsyncState;
            HttpWebRequest request = req.Request;
            Stream streamToWriteTo = req.OutputStream;
            try
            {
                HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result);
                Stream inputStream = response.GetResponseStream();

                byte[] data = new byte[req.Buffersize*1024];
                int bytesRead;

                long totalValue = response.ContentLength;
                long totalBytesRead = 0;
                while ((bytesRead = inputStream.Read(data, 0, data.Length)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (streamToWriteTo.Length != 0)
                        Logger.Info((int) ((streamToWriteTo.Length*100)/totalValue));
                    streamToWriteTo.Write(data, 0, bytesRead);
                    if (req.Callback != null) req.Callback(req, totalBytesRead, totalValue);
                }
            }
            finally
            {
                streamToWriteTo.Close();
            }
            Logger.Info("Completed download of {0}", req.SourceUri);
        }


        /// <summary>
        /// Saves the stream of data in <paramref name="stream"/>to the file named <paramref name="filename"/>
        /// </summary>
        /// <param name="filename">The file to save the stream <paramref name="stream"/> to</param>
        /// <param name="stream">The stream to save to <paramref name="filename"/></param>
        public static void Save(string filename, Stream stream)
        {
            int totalBytesWritten = 0;
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream s = store.CreateFile(filename);

                int count;
                byte[] buffer = new byte[512000];
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    s.Write(buffer, 0, count);
                    totalBytesWritten += count;
                } while (count > 0);
            }
            Logger.Info("Written {0} bytes to {1}", totalBytesWritten, filename);
        }


        /// <summary>
        ///   Copies the data from <paramref name="inStream" /> to <paramref name="outStream" />
        /// </summary>
        /// <param name="inStream"> The stream to copy data from </param>
        /// <param name="outStream"> The stream to copy data to </param>
        public static void CopyStream(Stream inStream, Stream outStream)
        {
            int count;
            byte[] buffer = new byte[512*1024];
            do
            {
                count = inStream.Read(buffer, 0, buffer.Length);
                outStream.Write(buffer, 0, count);
            } while (count > 0);
        }

        /// <summary>
        ///   Creates the directory named within isolated storage
        /// </summary>
        /// <param name="directoryName"> The name of the directory to create, must be a valid directory name </param>
        /// <param name="store"> </param>
        /// <returns> True if the directory was created, false if it already existed </returns>
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
        ///   Creates a directory if it doesn't exist
        /// </summary>
        /// <param name="directoryName"> The name of the directory to create </param>
        /// <returns> <c>true</c> is the directory had to be created, <c>false</c> if it already existed </returns>
        public static bool EnsureDirectory(string directoryName)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return EnsureDirectory(directoryName, store);
            }
        }


        /// <summary>
        ///   Returns the full path names of files within the direcctory specified by <paramref name="directoryName" />
        /// </summary>
        /// <param name="directoryName"> The name of the directory to search within </param>
        /// <param name="store"> The store to search </param>
        /// <returns> A (potentially empty) list of files </returns>
        public static List<string> GetFileNames(string directoryName, IsolatedStorageFile store)
        {
            string[] fileNames = store.GetFileNames(string.Format("{0}{1}*", directoryName, PathSeparator));
            List<string> fullFileNames = new List<string>(fileNames.Length);
            fullFileNames.AddRange(
                fileNames.Select(fileName => string.Format("{0}{1}{2}", directoryName, PathSeparator, fileName)));
            return fullFileNames;
        }

        /// <summary>
        ///   Saves an object to isolated storage, creating or overwriting if necessary, using the default <see
        ///    cref="DataContractSerializer" />
        /// </summary>
        /// <typeparam name="T"> The type of object to be saved </typeparam>
        /// <param name="obj"> The object to save </param>
        public static void SaveObject<T>(T obj) where T : class, IPersistedObject
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SaveObject(obj, store);
            }
        }


        /// <summary>
        ///   Saves an object to isolated storage, creating or overwriting if necessary, using the default <see
        ///    cref="DataContractSerializer" />
        /// </summary>
        /// <typeparam name="T"> The type of object to be saved </typeparam>
        /// <param name="obj"> The object to save </param>
        /// <param name="store"> The store to save data to </param>
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
        ///   Loads an object from Isolated Storage
        /// </summary>
        /// <remarks>
        ///   Uses <See cref="DataContractSerializer" /> to load data from isolated storage and sets <see
        ///    cref="IPersistedObject.LastUpdate" /> 
        ///   and <see cref="IPersistedObject.Id" /> to <paramref name="filename" />
        /// </remarks>
        /// <typeparam name="T"> The type of object to deserialise to </typeparam>
        /// <param name="store"> The isolated storage file to read from </param>
        /// <param name="filename"> The filename to load from (must not be null) </param>
        /// <returns> The loaded object </returns>
        public static T LoadObject<T>(IsolatedStorageFile store, string filename) where T : class, IPersistedObject
        {
            Logger.Info("Loading file {0} and deserialising to instance of {1}", filename, typeof (T).FullName);

            if (store == null) throw new ArgumentNullException("store");
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException("filename");
            if (!store.FileExists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            using (
                IsolatedStorageFileStream fileStream = store.OpenFile(filename, FileMode.Open, FileAccess.Read,
                                                                      FileShare.None))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof (T));
                T deserialisedObject = (T) serializer.ReadObject(fileStream);

                // Set the non-persisted filename property
                deserialisedObject.Id = filename;
                deserialisedObject.LastUpdate = store.GetLastWriteTime(filename);
                Logger.Info("Successfully read {0}, deserialised to {1} and set last update {2}", filename,
                            deserialisedObject, deserialisedObject.LastUpdate);

                return deserialisedObject;
            }
        }


        /// <summary>
        ///   Loads an object from Isolated Storage
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="filename"> The filename to load from (must not be null) </param>
        /// <returns> The loaded object </returns>
        public static T LoadObject<T>(string filename) where T : class, IPersistedObject
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return LoadObject<T>(store, filename);
            }
        }


        /// <summary>
        /// Deletes a file, delegates to <see cref="DeleteFile(string, bool)"/> passing <c>false</c> as the <c>suppressException</c> parameter
        /// </summary>
        /// <param name="filename">The name the file to delete</param>
        /// <returns><c>True</c> if the file was found and deleted, <c>false</c> otherwise</returns>
        public static bool DeleteFile(string filename)
        {
            return DeleteFile(filename, false);
        }


        /// <summary>
        ///   Deletes a file from Isolated Storage
        /// </summary>
        /// <param name="filename"> The name of the file to delete </param>
        /// <param name="suppressException"> If true, all exceptions will be swallowed (but logged) </param>
        /// <returns> <c>true</c> if the file was successfully deleted, <c>false</c> otherwise </returns>
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

        #region Nested type: DownloadRequest

        /// <summary>
        ///   Represents a download request, used with <see cref="IOHelper.DownloadStream(System.Uri,string)" />, <see
        ///    cref="IOHelper.DownloadStream(System.Uri,String)" />
        ///   and <see cref="IOHelper.DownloadStreamWorker" />
        /// </summary>
        public struct DownloadRequest
        {
            /// <summary>
            ///   The Uri to download data from
            /// </summary>
            public Uri SourceUri { get; set; }

            /// <summary>
            ///   The request object that's doing the downloading
            /// </summary>
            public HttpWebRequest Request { get; set; }

            /// <summary>
            ///   Where to write the data to
            /// </summary>
            public Stream OutputStream { get; set; }

            /// <summary>
            ///   Data will be downloading chunks of this size
            /// </summary>
            public int Buffersize { get; set; }

            /// <summary>
            ///   Callback after each chunk is downloaded.  Second parameter is the number of bytes downloaded so far, third is the total number of bytes expected.
            /// </summary>
            public Action<DownloadRequest, long, long> Callback { get; set; }
        }

        #endregion


        /// <summary>
        /// Determines whether or not a file exists in isolated storage
        /// </summary>
        /// <param name="filename">The name of the file to check for the existance of, must be a valid filename</param>
        /// <returns><c>true</c> if the file exists, <c>false</c> otherwise</returns>
        public static bool FileExists(string filename)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool fileExists = store.FileExists(filename);
                Logger.Info("Determined that {0} file does{1} exist", filename, fileExists ? string.Empty : "n't");
                return fileExists;
            }
        }

        /// <summary>
        /// Joins all the parmaters passed in <paramref name="pathElements"/> together using the <see cref="PathSeparator"/>
        /// </summary>
        /// <param name="pathElements">The elements that make up the path</param>
        /// <returns>A single string</returns>
        public static string CreatePath(params string[] pathElements)
        {
            return string.Join(PathSeparator, pathElements);
        }



        /// <summary>
        /// Searches a directory for a file which, when deserialised, matches the <paramref name="appId"/> with the <see cref="IPersistedObject.AppId"/> member.
        /// </summary>
        /// <typeparam name="T">The type of object that each file should deserialise to</typeparam>
        /// <param name="directory">The directory to search (non-recursively)</param>
        /// <param name="appId">The <see cref="IPersistedObject.AppId"/> to search for</param>
        /// <returns>The object of type <typeparamref name="T"/> that has an <see cref="IPersistedObject.AppId"/> matching <paramref name="appId"/></returns>
        public static T LoadFileByAppId<T>(string directory, string appId) where T : class, IPersistedObject
        {
            if (string.IsNullOrEmpty(appId)) throw new ArgumentException("appId must not be null", "appId");
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                List<string> filenames = GetFileNames(directory, store);
                Logger.Info("Searching {0} files within {1} for one with an AppId of {2}", filenames.Count, directory,
                            appId);
                foreach (string filename in filenames)
                {
                    T obj = LoadObject<T>(store, filename);
                    if (obj.AppId == appId)
                    {
                        Logger.Debug("Loaded file {0} and found matching AppId {1}", filename, obj.AppId);
                        return obj;
                    }
                    Logger.Debug("Loaded file {0} and found unmatching AppId \"{1}\"", filename, obj.AppId);
                }
            }
            Logger.Info("No match found in {0} for AppId {1}", directory, appId);
            return default(T);
        }

        /// <summary>
        /// Downloads the full contents of a stream and returns it as a byte array
        /// </summary>
        /// <param name="stream">The stream to download</param>
        /// <returns>The stream as a byte array</returns>
        public static byte[] DownloadStream(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            CopyStream(stream, ms);
            ms.Close();
            return ms.ToArray();

        }

        /// <summary>
        /// Saves the data passed in <paramref name="data"/> to a file named <paramref name="filename"/>
        /// </summary>
        /// <param name="filename">The filename to save <paramref name="data"/> to</param>
        /// <param name="data">The data to save to <paramref name="filename"/></param>
        public static int Save(string filename, byte[] data)
        {
            Logger.Info("Writing {0} bytes to {1}", data.Length, filename);
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(filename))
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return data.Length;
        }

        /// <summary>
        /// Delete all the files in a directory hierarchy recursively
        /// </summary>
        /// <param name="directoryName">The root directory to start deleting at</param>
        /// <returns>The total number of files removed</returns>
        public static int DeleteFiles(string directoryName)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return DeleteFiles(directoryName, store);
            }
        }


        /// <summary>
        /// Delete all the files in a directory hierarchy recursively
        /// </summary>
        /// <param name="directoryName">The root directory to start deleting at</param>
        /// <param name="store">The store to delete files from</param>
        /// <returns>The total number of files removed</returns>
        public static int DeleteFiles(string directoryName, IsolatedStorageFile store)
        {
            int total = 0;
            if (store.DirectoryExists(directoryName))
            {
                string[] filenames = store.GetFileNames(directoryName);
                Logger.Info("Deleting {0} files in {1}", filenames.Length, directoryName);
                total += filenames.Length;
                foreach (string filename in filenames)
                {
                    DeleteFile(filename);
                }
                string[] dirNames = store.GetDirectoryNames(Path.Combine(directoryName,"*"));
                Logger.Info("Deleting {0} sub-directories of {1}", dirNames.Length, directoryName);
                foreach (string dirName in dirNames)
                {
                    total += DeleteFiles(dirName);
                }
            }
            else
            {
                Logger.Info("Ignoring call to DeleteFile as directory {0} does not exist", directoryName);
            }
            return total;
        }


    }
}
