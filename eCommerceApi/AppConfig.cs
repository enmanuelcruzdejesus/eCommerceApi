using System;
using System.Configuration;
using System.IO;
using eCommerceApi.DAL.Services;
using Microsoft.Extensions.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using WooCommerceNET;

namespace ApiCore
{
    public class AppConfig : IDisposable
    {
        #region FIELDS
        private static AppConfig _instance;
        private static string _connectionString;
        private static IDbConnectionFactory _dbFactory;
        private Database _db = null;

        private RestAPI _restApi = null;

        //public static string code;
        //public static string realmId;
        //public static TokenResponse Token;

        private static string clientid;
        private static string clientsecret;
        private static string redirectUrl;
        private static string environment;
        private static string baseUrl;


        //private static OAuth2Client _auth2Client = null;

        //public  OAuth2Client Auth2Client 
        //{
        //    get
        //    {
        //        return _auth2Client;
        //    }
        //}


        #endregion

        #region CTOR
        private AppConfig()
        {
            var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
          

            var conf = builder.Build();
            _connectionString = conf.GetConnectionString("DefaultConnection");
            clientid = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "clientid").ToString();
            clientsecret = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "clientsecret").ToString();          
            baseUrl = conf.GetSection("WooCommerceSettings").GetValue(typeof(string), "baseUrl").ToString();
          



            _instance = null;
            _dbFactory = new OrmLiteConnectionFactory(_connectionString, SqlServer2014Dialect.Provider);
        }
        #endregion


        #region GETTERS AND SETTERS
        public static AppConfig Instance()
        {
            if (_instance == null)
                _instance = new AppConfig();

            return _instance;
        }
        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
            }
        }

        public Database Db
        {
            get
            {
                if (_db == null)
                    _db = new Database(ConnectionString);

                return _db;
            }
        }

        public RestAPI Service
        {
            get
            {
                if (_restApi == null) 
                {
                    _restApi = new RestAPI(baseUrl, clientid, clientsecret);
                   
                }
                

                return _restApi;
            }
        }



        public IDbConnectionFactory DbFactory { get { return _dbFactory; } }




        #endregion

        #region PUBLIC METHODS
        public void Dispose()
        {
            _instance = null;
            _connectionString = null;
            //_dbFactory = null;
        }

        #endregion



    }
}
