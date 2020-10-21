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
    public class TaskDetailsController : Controller
    {
        private IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public TaskDetailsController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _configuration = new ConfigurationBuilder()
                .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")
                .AddJsonFile("Config.json", optional: true, reloadOnChange: true)
                .Build();
                
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public async Task<IActionResult> Index(string JobId)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:" + _configuration["BackendApiPort"]);
            var client = new Job.JobClient(channel);
            var reply = await client.GetProcessingResultAsync(new RegisterResponse { Id = JobId }); 
            return View("Task", new TaskViewModel {Rank = reply.Response, Status = reply.Status, Id = JobId}); 
        }
    }
}