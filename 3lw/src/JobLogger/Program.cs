using System;
using NATS.Client;
using Subscriber;

namespace JobLogger
{
    class Program
    {
        static void Main(string[] args)
        {
           var loggerservice = new LoggerService();

            try {
            using(IConnection connection = new ConnectionFactory().CreateConnection(ConnectionFactory.GetDefaultOptions())){
                loggerservice.Run(connection);
                Console.WriteLine("JobLogger online");
                Console.ReadKey();
            }
            } catch(NATS.Client.NATSNoServersException ex) {
                Console.WriteLine("No connection to NATS");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
