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

        private double _currentDownloadProgress;

        /// <summary>
        /// 下载进度
        /// </summary>
        public double CurrentDownloadProgress
        {
            get => _currentDownloadProgress;
            set
            {
                _currentDownloadProgress = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentDownloadProgress"));
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

        private string _currentDownloadGroup;

        /// <summary>
        /// 下载类型
        /// </summary>
        public string CurrentDownloadGroup
        {
            get => _currentDownloadGroup;
            set
            {
                _currentDownloadGroup = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentDownloadGroup"));
            }
        }

        private string _currentDownloadFile;

        /// <summary>
        /// 当前下载文件名
        /// </summary>
        public string CurrentDownloadFile
        {
            get => _currentDownloadFile;
            set
            {
                _currentDownloadFile = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentDownloadFile"));
            }
        }

        private bool _downloadFinished;

        /// <summary>
        /// 当前下载任务是否结束
        /// </summary>
        public bool DownloadFinished
        {
            get => _downloadFinished;
            set
            {
                _downloadFinished = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DownloadFinished"));
            }
        }
    }
}