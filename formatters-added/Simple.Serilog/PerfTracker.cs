using System;
using System.Diagnostics;
using Serilog;

namespace Simple.Serilog
{
    public class PerfTracker
    {
        private readonly string _whatsBeingTracked;
        private readonly Stopwatch _tracker;
        private readonly string _infoName;
        private readonly string _infoValue;

        /// <inheritdoc />
        public PerfTracker(string whatsBeingTracked) : this(whatsBeingTracked, null, null)
        {            
        }

        /// <summary>
        /// Creates a new PerfTracker object to track performance.  The constructor starts the
        /// clock ticking.
        /// </summary>
        /// <param name="whatsBeingTracked">The name of the thing you're tracking performance for --
        /// like API method name, procname, or whatever.</param>
        /// <param name="infoName">The name of an additional value you want to capture</param>
        /// <param name="infoValue">The value of the additional info you're capturing (like parameters for a method)</param>
        public PerfTracker(string whatsBeingTracked, string infoName, string infoValue)
        {
            _infoName = infoName;
            _infoValue = infoValue;
            if (string.IsNullOrEmpty(infoValue) && !string.IsNullOrEmpty(infoName) ||
                !string.IsNullOrEmpty(infoValue) && string.IsNullOrEmpty(infoName))
            {
                throw new ArgumentException("Either both infoName and infoValue must be provided" +
                                            "or neither should be provided.");
            }
            _whatsBeingTracked = whatsBeingTracked;
            _tracker = new Stopwatch();
            _tracker.Start();
        }

        public void Stop()
        {
            if (_tracker == null) return;

            _tracker.Stop();

            if (string.IsNullOrEmpty(_infoValue))
            {
                Log.Information("{PerfItem} took {ElapsedMilliseconds} milliseconds",
                    _whatsBeingTracked, _tracker.ElapsedMilliseconds);
            }
            else
            {
                Log.Information("{PerfItem} took {ElapsedMilliseconds} milliseconds " +
                                "with {MoreName} of {MoreValues}",
                    _whatsBeingTracked, _tracker.ElapsedMilliseconds, _infoName, _infoValue);
            }
        }
    }
}

