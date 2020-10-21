using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using NATS.Client;

namespace BackendApi.Services
{
    public class JobService : Job.JobBase
    {
        private readonly static Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private readonly ILogger<JobService> _logger;
        private const int DELAY = 500;
        private const int REQUEST_REPEATS = 5;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            saveToDB(id + "_description", request.Description);
            saveToDB(id + "_data", request.Data);
            publish(id);
            var resp = new RegisterResponse
            {
                Id = id
            };
            _jobs[id] = request.Description;
            return Task.FromResult(resp);
        }

        public void publish(string id) 
        {
            IConnection connection = new ConnectionFactory().CreateConnection(ConnectionFactory.GetDefaultOptions());
            byte[] payload = Encoding.Default.GetBytes(id);
            connection.Publish("events", payload);
        }

        public void saveToDB(string id, string value)
        {
            var config = new ConfigurationBuilder()  
                        .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")  
                        .AddJsonFile("config.json", optional: false)  
                        .Build(); 
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:{config.GetValue<int>("RedisPort")}");
            IDatabase db = redis.GetDatabase();
            db.StringSet(id, value);
        }

        private string getFromDB(string id)
        {
            var config = new ConfigurationBuilder()  
                .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")  
                .AddJsonFile("config.json", optional: false)  
                .Build();
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:{config.GetValue<int>("RedisPort")}");
            IDatabase db = redis.GetDatabase();
            return db.StringGet(id);
        }
        public override Task<ProcessingResult> GetProcessingResult(RegisterResponse registerResponse, ServerCallContext context)
        {
            var config = new ConfigurationBuilder()  
                .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")  
                .AddJsonFile("config.json", optional: false)  
                .Build();
            string idResult = registerResponse.Id + "_rating";
            var result = new ProcessingResult {Status = "Processing", Response = "no response"};
            
            for (int i = 0; i < REQUEST_REPEATS; i++)
            {
                string response = getFromDB(idResult);
                if (response != null) {
                    result.Status = "Completed";
                    result.Response = response;
                    break;
                }
                Thread.Sleep(DELAY);
            }
            return Task.FromResult(result);
        }
    }
}
