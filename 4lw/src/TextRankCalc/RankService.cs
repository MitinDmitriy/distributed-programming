using System;
using System.IO;
using System.Text;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace Subscriber
{
    public class RankService
    {
        
        private IConfigurationRoot config;

        public string getValueFromDB(string id)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:{config.GetValue<int>("RedisPort")}");
            IDatabase db = redis.GetDatabase();
            return db.StringGet(id);
        }

        public void setValueToDB(string id, string value)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:{config.GetValue<int>("RedisPort")}");
            IDatabase db = redis.GetDatabase();
            db.StringSet(id, value);
        }

        private float calcMessageRating(string message)
        {
            int countVowels = Regex.Matches(message, @"[aeiouаёеуоыиэюя]", RegexOptions.IgnoreCase).Count;
            int countConsonants = Regex.Matches(message, @"[bcdfghjklmnpqrstvwxyzбвгджзйклмнпрстфхцчшщ]", RegexOptions.IgnoreCase).Count;
            int result = countConsonants > 0 ? countVowels / countConsonants : 0;
            return result;
        }

        public void RankTask(string id)
        {
            string dataFromDB = getValueFromDB(id + "_data");
            float rating = calcMessageRating(dataFromDB);
            id += "_rating";
            setValueToDB(id, rating.ToString());
        }

        public void Run(IConnection connection)
        {
            config = new ConfigurationBuilder()  
                .SetBasePath(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName + "/config")  
                .AddJsonFile("config.json", optional: false)  
                .Build();
            var greetings = connection.Observe("events")
                    .Select(m => Encoding.Default.GetString(m.Data));

            greetings.Subscribe(msg => RankTask(msg));
        }    
    }
}