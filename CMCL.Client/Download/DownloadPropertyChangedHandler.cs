using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CMCL.Client.Download
{
    public class DownloadPropertyChangedHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private double _currentTaskProgress;

        /// <summary>
        /// 下载进度
        /// </summary>
        public double CurrentTaskProgress
        {
            get => _currentTaskProgress;
            set
            {
                _currentTaskProgress = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentTaskProgress"));
            }
        }

        private double _currentDownloadSpeed;

        /// <summary>
        /// 下载速度
        /// </summary>
        public double CurrentDownloadSpeed
        {
            get => _currentDownloadSpeed;
            set
            {
                _currentDownloadSpeed = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentDownloadSpeed"));
            }
        }

        private string _currentTaskGroup;

        /// <summary>
        /// 下载类型
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

        private string _currentTaskName;

        /// <summary>
        /// 当前下载文件名
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

        private bool _taskFinished;

        /// <summary>
        /// 当前下载任务是否结束
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
    }
}