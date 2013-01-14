using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using NLog;

namespace Locima.SlidingBlock.Common
{
    /// <summary>
    /// Contains methods that help with respect to reflection and dynamic code invocation
    /// </summary>
    public class ReflectionHelper
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates instances of all classes found within the calling assembly that implement <typeparamref name="T"/> using their default constructor
        /// </summary>
        /// <remarks>Searches the calling assembly (identified by <see cref="Assembly.GetCallingAssembly"/>) searching for all instances of <typeparamref name="T"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>An unordered list containing a single instance of each class found in the calling assembly compatible with <typeparamref name="T"/></returns>
        public static IEnumerable<T> CreateInstancesOf<T>() where T : class
        {
            Type matchType = typeof(T);
            Logger.Info("Searching for all instances of {0}", matchType.FullName);
            Assembly asm = Assembly.GetCallingAssembly();
            Type[] defaultConsArgs = new Type[0];
            const BindingFlags publicConsBindingFlags = BindingFlags.Public | BindingFlags.Instance;
            List<T> instances = new List<T>();
            Type[] types = asm.GetTypes();
            foreach (Type type in types.Where(type => !type.IsAbstract
                                                      && matchType.IsAssignableFrom(type)
                                                      &&
                                                      type.GetConstructor(publicConsBindingFlags, null, defaultConsArgs, null) != null))
            {
                Logger.Info("Attempting to instantiate matching class with public no-op constructor: {0}", type.FullName);
                instances.Add((T) Activator.CreateInstance(type));
            }
            Logger.Info("Found {0} classes that are compatible with {1} out of {2}", instances.Count, matchType.FullName, types.Length);
            return instances;
        }

    }
}