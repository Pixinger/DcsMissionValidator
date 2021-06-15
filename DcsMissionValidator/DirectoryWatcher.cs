using System;
using System.IO;

namespace DcsMissionValidator
{
    internal class DirectoryWatcher : IDisposable
    {
        #region Eventschema: public event EventHandler<FileChangedEventArg> FileChanged;
        #region public class FileChangedEventArg : EventArgs
        public class FileChangedEventArg : EventArgs
        {
            public readonly FileInfo FileInfo;
            public FileChangedEventArg(FileInfo fileInfo)
            {
                this.FileInfo = fileInfo;
            }
        }
        #endregion
        private EventHandler<FileChangedEventArg> _OnFileChanged;
        protected virtual void OnFileChanged(object sender, FileChangedEventArg e)
        {
            if (_OnFileChanged != null)
            {
                _OnFileChanged(sender, e);
            }
        }
        #endregion

        private FileSystemWatcher _Watcher = null;


        public DirectoryWatcher(string directory, EventHandler<FileChangedEventArg> onFileChanged)
        {
            _OnFileChanged = onFileChanged ?? throw new ArgumentNullException(nameof(onFileChanged));

            if (Directory.Exists(directory))
            {
                this._Watcher = new FileSystemWatcher(directory, "*.miz");
                this._Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                this._Watcher.Created += this.OnFileChanged;
                this._Watcher.Changed += this.OnFileChanged;
                this._Watcher.IncludeSubdirectories = true;
                this._Watcher.EnableRaisingEvents = true;
            }
        }

        #region IDisposable Member
        ~DirectoryWatcher()
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
            if (this._Watcher != null)
            {
                this._Watcher.EnableRaisingEvents = false;
                this._Watcher.Dispose();
                this._Watcher = null;
            }
            Logger.Debug("DirectoryWatcher disposed");
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.FullPath))
            {
                var fileInfo = new FileInfo(e.FullPath);
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > 0)
                    {
                        this.OnFileChanged(this, new FileChangedEventArg(fileInfo));
                    }
                }
            }
        }
    }
}
