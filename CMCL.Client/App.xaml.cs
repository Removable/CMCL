using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMCL.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
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
                LogHelper.WriteLog(exception);
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

            // 在全局容器中配置 AppSettings
            // services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

            // 在全局容器中注册自定义服务
            // services.AddScoped<ISampleService, SampleService>();
        }
    }
}