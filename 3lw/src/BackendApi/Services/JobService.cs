using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            
            string id = Guid.NewGuid().ToString();
            saveToDB(id, request.Description);
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
    }
}
