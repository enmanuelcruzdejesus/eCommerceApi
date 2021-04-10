using ApiCore.Utils.Services;
using eCommerceApi.ServiceClient.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eCommerceApi.ServiceClient
{
    public partial class Form1 : Form
    {
        RestApiService _service;
        string _authorizationToken = string.Empty;
        string _authorizationMethod = string.Empty;
        string _baseUrl = "http://localhost:50756/api/{0}/{1}";
      

        public Form1()
        {
            InitializeComponent();

            _service = new RestApiService(new ResilienceHttpClient(_baseUrl),
                _authorizationToken, _authorizationMethod);
            
        }

        private  void btnSync_Click(object sender, EventArgs e)
        {
            Task.Run(async () => 
            {
                var r = await _service.Sync();
                if (r.IsSuccessStatusCode)
                {
                    var content = await r.Content.ReadAsStringAsync();
                    //var data = JsonConvert.DeserializeObject<string>(content);
                    Console.WriteLine("Sync was executed succesfully {0}", content);

                }
                else
                {
                    Console.WriteLine("Error!!");
                }

            });
        }
    }
}
