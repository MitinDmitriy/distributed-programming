using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FrontendTask.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using BackendApi;
using System.IO;

namespace FrontendTask.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "/config")
                .AddJsonFile("frontConfig.json", optional: true, reloadOnChange: true)
                .Build();
                
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HandleFormSubmit(String description) {
            
            if (description == null) {
                return View("Error", new ErrorViewModel {RequestId = "Description can't be empty"});
            }
            try {
            using var channel = GrpcChannel.ForAddress("http://localhost:" + _configuration["port"]);
            var client = new Job.JobClient(channel);
            var reply = await client.RegisterAsync(new RegisterRequest { Description = description });
            return View("Task", new TaskViewModel { Id = reply.Id });
            } catch(Grpc.Core.RpcException ex) {
                return View("Error", new ErrorViewModel {RequestId = ex.Message});
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
