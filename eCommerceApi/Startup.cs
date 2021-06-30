using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCore.Services;
using eCommerce.Model.Entities;
using eCommerceApi.Model;
using eCommerceApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using WooCommerceNET;

namespace eCommerceApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
            });

            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var conf = builder.Build();
            var _connectionString = conf.GetConnectionString("DefaultConnection");
            var clientid = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "clientid").ToString();
            var clientsecret = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "clientsecret").ToString();
            var baseUrl = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "baseUrl").ToString();


            // singleton
            var _dbFactory = new OrmLiteConnectionFactory(_connectionString, SqlServer2014Dialect.Provider);
            var _restApi = new RestAPI(baseUrl, clientid, clientsecret);

            services.AddSingleton<IDbConnectionFactory>(_dbFactory);
            services.AddSingleton<RestAPI>(_restApi);


            services.Add(new ServiceDescriptor(typeof(IRepository<Customers>), new ServiceStackRepository<Customers>(_dbFactory)));
            services.Add(new ServiceDescriptor(typeof(IRepository<ProductCategories>), new ServiceStackRepository<ProductCategories>(_dbFactory)));
            services.Add(new ServiceDescriptor(typeof(IRepository<Products>), new ServiceStackRepository<Products>(_dbFactory)));
            services.Add(new ServiceDescriptor(typeof(IRepository<ProductVariations>), new ServiceStackRepository<ProductVariations>(_dbFactory)));

            services.Add(new ServiceDescriptor(typeof(IRepository<Orders>), new ServiceStackRepository<Orders>(_dbFactory)));
            services.Add(new ServiceDescriptor(typeof(IRepository<TransactionSyncLog>), new ServiceStackRepository<TransactionSyncLog>(_dbFactory)));
            services.Add(new ServiceDescriptor(typeof(IRepository<SyncTables>), new ServiceStackRepository<SyncTables>(_dbFactory)));



            //services.AddSingleton<IJobFactory, SingletonJobFactory>();
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();


            //services.AddSingleton<SyncJob>();
            //services.AddSingleton(new JobSchedule(
            //  jobType: typeof(SyncJob),
            //  cronExpression: "0 0/1 * * * ?")); // run every 5 minutes


            //services.AddHostedService<QuartzHostedService>();

            services.AddApplicationInsightsTelemetry();

            var jwtSection = Configuration.GetSection("JWTSettings");
            services.Configure<JWTSettings>(jwtSection);
            var appSettings = jwtSection.Get<JWTSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.SecretKey);

            services.AddAuthentication(x => 
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => 
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //// other code remove for clarity 
            //loggerFactory.AddFile("Logs/mylog-{Date}.txt");

        }
    }
}
