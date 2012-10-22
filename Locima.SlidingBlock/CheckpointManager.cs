using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Locima.SlidingBlock
{
    /// <summary>
    ///   Checkpoint Manager instances are used to manage the serialisation of model data to session state or isolated storage. Objects implementing <see
    ///    cref="ICheckpointable" /> and <see cref="INotifyPropertyChanged" /> may register with Checkpoint Managers and will be informed periodically when to save their state and to which type of storage.
    /// </summary>
    /// <remarks>
    ///   <para>This class exists to help with the scenario that applications may be terminated without warning or the usual events being fired,
    ///     for example due to power failure or incoming phone call.  Losing state like this in a game can be frustrating for the user, therefore
    ///     this class provides the facility to set timed "checkpoints" or moments when registered model classes are instructed to save their state.</para>
    ///   <para>When a checkpoint is required (either through a timed event or explicit call to <see cref="Checkpoint(CheckpointType)" />) any objects registered
    ///   with the checkpoint manager that have notified the checkpoint manager that a property has changed (via the <see cref="INotifyPropertyChanged" />.</para>
    ///   <para>
    ///   This class is currently unused and therefore excluded from the project definition.  I still think it might come in handy at some point...</para>
    /// </remarks>
    public class CheckpointManager : ICheckpointable
    {
        private readonly object _lock = new object();

        private Dictionary<ICheckpointable, ICollection<string>> _objectsChangedSinceLastCheckpoint;

        public CheckpointManager()
        {
            _objectsChangedSinceLastCheckpoint = new Dictionary<ICheckpointable, ICollection<string>>();
        }

        public int CheckpointInterval { get; set; }

        /// <summary>
        ///   If set to true then timed checkpoints
        /// </summary>
        public bool EnableTimer { get; set; }

        #region ICheckpointable Members

        public void Checkpoint(CheckpointType cpType)
        {
            lock (_lock)
            {
                foreach (ICheckpointable o in _objectsChangedSinceLastCheckpoint.Values)
                {
                    o.Checkpoint(cpType);
                }
                _objectsChangedSinceLastCheckpoint = new Dictionary<ICheckpointable, ICollection<string>>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// </summary>
        public void CheckpointOnXnaEvents()
        {
        }

        public void CheckpointOnSilverlightEvents()
        {
        }

        public void Register(ICheckpointable objectToInclude, bool deepScan)
        {
            objectToInclude.PropertyChanged += Notify;
        }


        public void Deregister(ICheckpointable objectToExclude, bool deepScan)
        {
            objectToExclude.PropertyChanged -= Notify;
        }


        private void Notify(object senderObject, PropertyChangedEventArgs e)
        {
            ICheckpointable sender = senderObject as ICheckpointable;
            if (sender == null) throw new ArgumentException("sender"); // TODO: Fix crudity here
            lock (_lock)
            {
                ICollection<string> values;
                if (!_objectsChangedSinceLastCheckpoint.TryGetValue(sender, out values))
                {
                    values = new List<string>();
                    _objectsChangedSinceLastCheckpoint.Add(sender, values);
                }
                values.Add(e.PropertyName);
            }
        }
    }

    public interface ICheckpointable : INotifyPropertyChanged
    {
        void Checkpoint(CheckpointType checkpointType);
    }


    public enum CheckpointType : byte
    {
        State = 1,
        IsolatedStorage = 2,
        Both = 3
    }


    public interface ICheckpointEventArgs
    {
        CheckpointType Type { get; }
        long Interval { get; }
    }

    public class CheckpointEventArgs : ICheckpointEventArgs
    {
        #region ICheckpointEventArgs Members

        public CheckpointType Type { get; set; }
        public long Interval { get; set; }

        #endregion
    }
}