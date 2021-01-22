using System;
using System.Windows;
using CMCL.LauncherCore.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMCL.Wpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await GameHelper.ApplicationInit();

                //配置全局服务容器
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                ServiceProvider = serviceCollection.BuildServiceProvider();

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception exception)
            {
                await LogHelper.LogExceptionAsync(exception);
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("mc", client =>
            {
                //暂无
            });

            // 向全局容器中注册一个视图
            services.AddTransient(typeof(MainWindow));
        }
    }
}