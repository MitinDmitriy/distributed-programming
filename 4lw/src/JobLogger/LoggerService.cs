using System;
using System.IO;
using System.Text;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Subscriber
{
    public class LoggerService
    {
        public string getDescriptionInDB(string id)
        {
            var config = new ConfigurationBuilder()  
                        .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")  
                        .AddJsonFile("Config.json", optional: false)  
                        .Build(); 
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:{config.GetValue<int>("RedisPort")}");
            IDatabase db = redis.GetDatabase();
            return db.StringGet(id);
        }

        public void logToConsole(string id)
        {
            string redisValue = getDescriptionInDB(id + "_description");
            Console.WriteLine(id + " : " + redisValue);
        }

        public void Run(IConnection connection)
        {
            var greetings = connection.Observe("events")
                    .Select(m => Encoding.Default.GetString(m.Data));

            greetings.Subscribe(msg => logToConsole(msg));
        }    
    }
}