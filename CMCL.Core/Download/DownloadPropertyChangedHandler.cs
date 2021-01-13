using System.ComponentModel;

namespace CMCL.Core.Download
{
    public class DownloadPropertyChangedHandler : INotifyPropertyChanged
    {
        private string _currentTaskGroup;

        private string _currentTaskName;

        private double _currentTaskProgress;

        private bool _taskFinished;

        /// <summary>
        ///     下载进度
        /// </summary>
        public double CurrentTaskProgress
        {
            get => _currentTaskProgress;
            set
            {
                _currentTaskProgress = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentTaskProgress)));
            }
        }

        /// <summary>
        ///     下载类型
        /// </summary>
        public string CurrentTaskGroup
        {
            get => _currentTaskGroup;
            set
            {
                _currentTaskGroup = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentTaskGroup"));
            }
        }

        /// <summary>
        ///     当前下载文件名
        /// </summary>
        public string CurrentTaskName
        {
            get => _currentTaskName;
            set
            {
                _currentTaskName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentTaskName"));
            }
        }

        /// <summary>
        ///     当前下载任务是否结束
        /// </summary>
        public bool TaskFinished
        {
            get => _taskFinished;
            set
            {
                _taskFinished = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TaskFinished"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}