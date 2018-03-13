using EasyNetQ;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hi there client! Please insert a sentance you would like suggestions for.");
            HandleUserInput();
        }
        //Handles all user input and sends messages to the load balancer.
        private static void HandleUserInput()
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                bus.Receive<SuggestionMessage>("loadbalancer.result", suggestion => HandleSuggestion(suggestion));

                var input = "";
                while ((input = Console.ReadLine()) != "Quit")
                {
                    bus.Send("loadbalancer.request", new SuggestionMessage
                    {
                        Word = input
                    });
                    Console.WriteLine("Nice word, pal!");

                }
            }
        }

        //Displays recieved suggestions to the client.
        private static void HandleSuggestion(SuggestionMessage suggestion)
        {
            Console.WriteLine("Suggestion recieved: " + suggestion.Word);
        }
    }
}
