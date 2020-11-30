using System;
using System.Threading.Tasks;

namespace CMCL.Client.Window
{
    public partial class LoadingFrm : System.Windows.Window
    {
        public LoadingFrm()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// 执行任务方法的委托
        ///// </summary>
        ///// <param name="param"></param>
        //public delegate void DoWorkDelegate(object param);

        /// <summary>
        /// 打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="loadingText">加载文字</param>
        /// <param name="actions">要执行的任务数组</param>
        public void DoWork(string loadingText, params Action[] actions)
        {
            LoadingControl.LoadingTip = loadingText;
            this.Show();
            var taskFactory = new TaskFactory();
            var taskArray = new Task[actions.Length];
            for (var i = 0; i < actions.Length; i++)
            {
                taskArray[i] = taskFactory.StartNew(actions[i]);
            }
            taskFactory.ContinueWhenAll(taskArray, result =>
            {
                this.Dispatcher.BeginInvoke(new Action(this.Close));
            });
        }
    }
}