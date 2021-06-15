using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DcsMissionValidator
{
    internal class ValidationPool : IDisposable
    {
        #region private class Container
        private class Container
        {
            public FileInfo FileInfo;
            public DateTime TimeToExecute;
        }
        #endregion

        private bool _Simulate;
        private bool _CreateTextfile;
        private Configuration _Configuration;

        private Thread _Thread = null;
        private ManualResetEvent _ManualResetEvent = new ManualResetEvent(false);

        private object _SyncObject = new object();
        private List<Container> _Containers = new List<Container>();


        public ValidationPool(Configuration configuration, bool simulate, bool createTextfile)
        {
            this._Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._Simulate = simulate;
            this._CreateTextfile = createTextfile;

            this._Thread = new Thread(this.ThreadProc);
            this._Thread.Name = "ValidationPool";
            this._Thread.Priority = ThreadPriority.BelowNormal;
            this._Thread.Start();
        }
        #region IDisposable Member
        ~ValidationPool()
        {
            this.Dispose(false);
        }
        public bool Disposed { get; private set; }
        public bool Disposing { get; private set; }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            this.Disposing = true;
            this.OnDispose(disposing);
            this.Disposing = false;
            this.Disposed = true;
        }
        private void ExceptionIfDisposed()
        {
            if (this.Disposed)
                throw new ObjectDisposedException(this.ToString());
        }
        #endregion
        protected virtual void OnDispose(bool disposing)
        {
            if (this._Thread != null)
            {
                this._ManualResetEvent.Set();
                if (!this._Thread.Join(1000)) // Try it low and slow...
                {
                    Logger.Debug("ValidationPool-Thread seem to be unwilling to exit. Aborting it now.");
                    this._Thread.Abort(); // This should throw an exception in the thread.
                    if (!this._Thread.Join(500)) // Give it some more time....
                    {
                        Logger.Debug("ValidationPool-Thread seem to be unwilling to exit, even after ABORT call.");
                    }
                }
                this._Thread = null;
            }
            Logger.Debug("ValidationPool disposed");
        }

        public void Add(FileInfo fileInfo)
        {
            Debug.Assert(fileInfo.Exists);
            Debug.Assert(fileInfo.Length > 0);

            lock (this._SyncObject)
            {
                var timeToExecute = DateTime.Now + TimeSpan.FromSeconds(this._Configuration.DelayAfterModified_s);

                // If there already exists a container with this name, we only update the execute time.
                foreach (var container in this._Containers)
                {
                    if (container.FileInfo.FullName == fileInfo.FullName)
                    {
                        container.TimeToExecute = timeToExecute;
                        return;
                    }
                }

                // Add it as a new Container.
                this._Containers.Add(new Container()
                {
                    FileInfo = fileInfo,
                    TimeToExecute = timeToExecute,
                });
                Logger.Debug($"Added entry to ValidationPool ({fileInfo.Name}).");
            }
        }

        private Container PopEllapsedContainer()
        {
            lock (this._SyncObject)
            {
                var now = DateTime.Now;
                for (int i = 0; i < this._Containers.Count; i++)
                {
                    if (this._Containers[i].TimeToExecute <= now)
                    {
                        var result = this._Containers[i];
                        this._Containers.RemoveAt(i);
                        return result;
                    }
                }

                return null;
            }
        }

        private void ThreadProc(Object stateInfo)
        {
            try
            {
                Logger.Debug("ValidationPool-Thread started.");
                while (!this._ManualResetEvent.WaitOne(250))
                {
                    Container container = this.PopEllapsedContainer(); // SyncLocked...
                    if (container != null)
                    {
                        Logger.Debug($"Processing container ({container.FileInfo.Name}) now.");
                        MissionValidator.Validate(this._Configuration, container.FileInfo, this._Simulate, this._CreateTextfile);
                    }
                }
                Logger.Debug("ValidationPool-Thread finished.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Exception in ValidationPool-Thread.");
            }
        }
    }
}
