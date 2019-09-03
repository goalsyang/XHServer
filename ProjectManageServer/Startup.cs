using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using ProjectManageServer.Bussiness;
using ProjectManageServer.Common;
using ProjectManageServer.Common.Filter;
using ProjectManageServer.Interface;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManageServer
{
    public class Startup
    {

        #region 字段属性

        /// <summary>
        /// 日志接口
        /// </summary>
        public static ILoggerRepository repository { get; set; }

        /// <summary>
        /// 配置接口
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        #region 启动项配置

        /// <summary>
        /// 启动项配置
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(environment.ContentRootPath, "Configs"))
                .AddJsonFile("appsettings.json", true, true) 
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// 运行时添加服务
        /// 运行时调用此方法。使用此方法向容器添加服务
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigDatabase(services);

            ConfigTokenForJWT(services);

            ConfigLog();

            ConfigIOC(services);

            //services.AddResponseCaching();//注册缓存

            services.AddMvc(action =>
            {
                action.Filters.Add(typeof(CustomerActionFilter)); // by type

                action.Filters.Add(new CustomerActionFilter()); // an instance

                action.Filters.Add<GlobalExceptionFilter>();
            });

            services.AddMvc().AddJsonOptions(action =>
            {
               
                action.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                action.SerializerSettings.ContractResolver = new DefaultContractResolver();

                action.SerializerSettings.DateFormatString = "yyyy-MM-dd";
            });
        }

        /// <summary>
        /// 运行时调用，配置HTTP管道
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ///配置跨域
            app.UseCors(config =>
            { 
                config.AllowAnyHeader();

                config.AllowAnyMethod();

                config.AllowAnyOrigin();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } 
             
            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseMvc();
        }

        #endregion

        #region 方法函数

        /// <summary>
        /// 获取配置文件中的数据库地址
        /// </summary>
        /// <param name="services"></param>
        private void ConfigDatabase(IServiceCollection services)
        {
            var appsSettings = Configuration.GetSection("AppSettings");

            AppDataBase.ConnStrings = Configuration["AppSettings:ConnStrings"];

            AppDataBase.LogConnString = Configuration["AppSettings:LogConnString"];
        }

        /// <summary>
        /// 获取配置文件中的日志配置
        /// </summary>
        private void ConfigLog()
        {
            repository = LogManager.CreateRepository("NETCoreRepository");

            XmlConfigurator.Configure(repository, new FileInfo("Configs/log4net.config"));

            LogProperties.LogRepository = repository;
        }

        /// <summary>
        /// 配置JWT
        /// </summary>
        /// <param name="services"></param>
        private void ConfigTokenForJWT(IServiceCollection services)
        {
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));

            JwtSettings jwt = new JwtSettings();

            Configuration.Bind("JwtSettings", jwt);

            services.AddAuthentication(scheme =>
            {
                scheme.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                scheme.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
                config.SecurityTokenValidators.Clear();

                config.SecurityTokenValidators.Add(new CustomerTokenValidate());

                config.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["token"];

                        context.Token = token.FirstOrDefault();

                        return Task.CompletedTask;
                    }
                };
            });
        }

        /// <summary>
        /// 配置IOC容器
        /// </summary>
        /// <param name="services"></param>
        private void ConfigIOC(IServiceCollection services)
        {
            services.AddTransient<IAuthorization,AuthorizationBussiness>();
            services.AddTransient<ILogin, LoginBussiness>();
            services.AddTransient<IObjectCreate, ObjectCreateBussiness>();


        }

        #endregion
         
    }
}
