using System;
using System.Threading.Tasks;
using System.Windows;

namespace CMCL.Client.Window
{
    public partial class DownloadFrm : System.Windows.Window
    {
        public DownloadFrm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="loadingText">加载文字</param>
        /// <param name="actions">要执行的任务数组</param>
        public void DoWork(params Action[] actions)
        {

            var currentTaskIndex = 1;
            this.Show();
            var taskFactory = new TaskFactory();

            var taskArray = new Task[actions.Length];
            for (var i = 0; i < actions.Length; i++)
            {
                taskArray[i] = taskFactory.StartNew(actions[i]);
            }
            taskFactory.ContinueWhenAny(taskArray, result =>
            {
                currentTaskIndex++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
                    TbCurrentTaskDetail.Text = currentTaskIndex.ToString();
                }));
            });
            taskFactory.ContinueWhenAll(taskArray, result => { this.Dispatcher.BeginInvoke(new Action(this.Close)); });
        }
    }
}