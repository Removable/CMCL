using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CMCL.LauncherCore.Download;
using CMCL.LauncherCore.Launch;
using CMCL.LauncherCore.Utilities;
using ComponentUtil.Common.Data;
using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;

namespace CMCL.ConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("欢迎使用CMCL启动器!");
            Console.WriteLine("正在初始化...");
            await GameHelper.ApplicationInit();
            Console.WriteLine("初始化完毕！");
            Console.WriteLine("输入start启动游戏");
            Console.WriteLine("输入exit退出程序");
            Console.WriteLine("输入help查看更多指令");

            while (true)
            {
                var input = Console.ReadLine()?.ToLower();
                if (string.IsNullOrWhiteSpace(input)) return;
                switch (input)
                {
                    case "exit":
                        Console.WriteLine("正在退出...");
                        Environment.Exit(0);
                        return;
                    case "start":
                        await StartGame();
                        break;
                    case "help":
                        MoreCommand();
                        break;
                    case "config":
                        PrintConfig();
                        break;
                    case "versions":
                        await PrintVersionList();
                        break;
                }
            }
        }

        /// <summary>
        /// 打印版本列表
        /// </summary>
        private static async Task PrintVersionList()
        {
            var mirror = MirrorManager.GetCurrentMirror();
            var versionList = await mirror.Version.LoadGameVersionList(Utils.HttpClientFactory.CreateClient());
            var table = new ConsoleTable("版本", "发布时间","类型");
            
            foreach (var vi in versionList.Versions)
            {
                table.AddRow(vi.Id, vi.ReleaseTime, vi.Type);
            }
            table.Write(Format.Alternative);
        }

        /// <summary>
        /// 打印配置信息
        /// </summary>
        private static void PrintConfig()
        {
            var table = new ConsoleTable("配置项", "字段名","当前内容");
            var config = AppConfig.GetAppConfig();
            foreach (var propertyInfo in config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                table.AddRow(propertyInfo.GetDescription(), propertyInfo.Name, config.GetPropertyByName(propertyInfo.Name));
            }
            table.Write(Format.Alternative);
        }

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <returns></returns>
        private static async Task StartGame()
        {
            var launcher = new Launcher();

            #region 事件注册

            launcher.OnLaunchError += (l, err) => { Console.WriteLine($"{err.Message}，启动器内核版本：{l.Version}"); };
            launcher.BeforeGameLaunch += (sender, status, info) =>
            {
                Console.WriteLine($"正在启动，游戏版本：{info.Id}，启动器内核版本：{((Launcher) sender).Version}");
            };

            #endregion

            await launcher.Start();
        }

        private static void MoreCommand()
        {
            Console.WriteLine("输入start启动游戏");
            Console.WriteLine("输入exit退出程序");
            Console.WriteLine("输入config查看配置");
            Console.WriteLine("输入versions获取版本列表");
            Console.WriteLine("输入help查看更多指令");
        }
    }
}