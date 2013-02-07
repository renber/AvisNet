using Avis.IO.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Avis.IO.Net
{
    /// <summary>
    /// Monitors message traffic
    /// and decides if the connection is still up
    /// </summary>
    public class LivenessMonitor
    {
        /// <summary>
        /// The connection state
        /// </summary>
        public bool IsUp { get; private set; }

        System.Timers.Timer timer;

        IConnector connector;
        ManualResetEvent responseEvent = new ManualResetEvent(false);
        
        public int LivenessTimeout
        {
            get
            {
                return (int)timer.Interval;
            }
            set
            {
                if (value < 0)                
                    throw new ArgumentOutOfRangeException("LivenessTimeout", "LivenessTimeout must be greater that or equal to zero.");

                timer.Interval = value;
                if (timer.Enabled)
                {
                    // reset the timer
                    timer.Enabled = false;
                    timer.Enabled = true;                    
                }
            }
        }

        public int ReceiveTimeout { get; set; }

        public LivenessMonitor(IConnector connector, int livenessTimeout = 60000, int receiveTimeout = 10000)
        {
            IsUp = true;
            this.connector = connector;
            connector.MessageReceived += connector_MessageReceived;
            ReceiveTimeout = receiveTimeout;

            timer = new System.Timers.Timer(livenessTimeout);
            timer.Elapsed += timer_Elapsed;                       
        }        

        /// <summary>
        /// Enables liveness monitoring
        /// </summary>
        public void Enable()
        {
            timer.Enabled = true;
        }

        /// <summary>
        /// Disables liveness monitoring
        /// </summary>
        public void Disable()
        {
            timer.Enabled = false;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;            

            // did not receive a message during the lifeness timeout
            // -> send a test message
            if (connector != null && connector.Connected)
            {
                connector.Send(TestConn.Instance);
                responseEvent.Reset();

                // wait for ConfConn
                Task.Run(() =>
                {
                    // wait for signal from connector_MessageReceived
                    if (!responseEvent.WaitOne(ReceiveTimeout))
                    {
                        // timed out
                        IsUp = false;
                        OnConnectionDied();
                    }
                    else
                    {
                        // continue monitoring
                        timer.Enabled = true;
                    }
                });
            }
            else
            {
                // the connection is down
                IsUp = false;
                OnConnectionDied();
            }
        }

        void connector_MessageReceived(object sender, MessageEvent e)
        {
            // reset the timer
            timer.Enabled = false;
            timer.Enabled = true;

            if (e.Message.TypeId == ConfConn.ID)
            {
                responseEvent.Set();
                // Console.WriteLine("Liveness confirmed: received ConfConn");
            }
        }

        #region Events

        /// <summary>
        /// Fired when the liveness check fails and the connection
        /// is considered dead; forther liveness monitoring is then automatically disabled
        /// </summary>
        public event EventHandler ConnectionDied;

        /// <summary>
        /// Fires the ConnectionDied event
        /// </summary>
        protected void OnConnectionDied()
        {
            if (ConnectionDied != null)
                ConnectionDied(this, EventArgs.Empty);
        }

        #endregion
    }
}
