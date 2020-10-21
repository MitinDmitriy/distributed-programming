using System;
using NATS.Client;
using Subscriber;

namespace JobLogger
{
    class Program
    {
        static void Main(string[] args)
        {
           var RankService = new RankService();


            try {
            using(IConnection connection = new ConnectionFactory().CreateConnection(ConnectionFactory.GetDefaultOptions())){
                RankService.Run(connection);
                Console.WriteLine("TextRankCalc online");
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
