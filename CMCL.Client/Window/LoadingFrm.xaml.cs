using System;
using System.Threading.Tasks;

namespace CMCL.Client.Window
{
    public partial class LoadingFrm : System.Windows.Window
    {
        private static LoadingFrm _loadingFrm;

        private LoadingFrm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     单例：获取窗口
        /// </summary>
        /// <returns></returns>
        public static LoadingFrm GetInstance()
        {
            return _loadingFrm ??= new LoadingFrm();
        }

        /// <summary>
        ///     打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="loadingText">加载文字</param>
        /// <param name="closeWhenFinish">任务结束后是否关闭</param>
        /// <param name="actions">要执行的任务数组</param>
        public void DoWork(string loadingText, bool closeWhenFinish, params Action[] actions)
        {
            var currentTaskIndex = 1;
            LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
            
            Show();
            var taskFactory = new TaskFactory();

            var taskArray = new Task[actions.Length];
            for (var i = 0; i < actions.Length; i++) taskArray[i] = taskFactory.StartNew(actions[i]);
            taskFactory.ContinueWhenAny(taskArray, result =>
            {
                currentTaskIndex++;
                LoadingControl.LoadingTip =
                    loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
            });
            taskFactory.ContinueWhenAll(taskArray, result => { if (closeWhenFinish) Close(); });
        }
    }
}