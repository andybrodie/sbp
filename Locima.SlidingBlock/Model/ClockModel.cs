using System;
using System.Runtime.Serialization;

namespace Locima.SlidingBlock.Model
{
    [DataContract]
    public class ClockModel
    {
        [DataMember] private DateTime _origin;

        public TimeSpan Time
        {
            get { return DateTime.Now - _origin; }
        }

        public void Reset()
        {
            _origin = DateTime.Now;
        }

    }
}