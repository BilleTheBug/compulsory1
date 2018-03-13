using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messages;

namespace DidYouMeanService
{
    public class MessageHandler
    {
        private static IBus iBus;
        private int id;
        private float serverLoad = 0;
        public MessageHandler(int id)
        {
            this.id = id;
            InitializeMessaging(id);
        }
        //Creates a new MessageHandler on a new thread.
        private void CreateNewService(CreateServiceMessage request)
        {
            Task.Factory.StartNew(() => new MessageHandler(request.ServiceId));
            Console.WriteLine("Service " + request.ServiceId + " created.");
        }
        //Handles messages recieved from the loadbalancer. Serverload is increased by 10 for each request recieved.
        private void HandleSuggestionRequest(SuggestionMessage request)
        {
            serverLoad += 10;
            Console.WriteLine("Service " + id + " recieved request with word:");
            Console.WriteLine(request.Word);

            String result = ServiceProgram.CheckForSuggestions(request.Word);
            if (result != null)
                Console.WriteLine("Suggested word returned to client: " + result);
            else
                Console.WriteLine("No result or perfect match found.");
            iBus.Send<SuggestionReplyMessage>("suggestion.reply", new SuggestionReplyMessage() { Word = result, ServerLoad = serverLoad, ServiceID = id });
        }
        //Initializes messaging and subscribes to the needed queues
        private void InitializeMessaging(int id)
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                iBus = bus;
                bus.Receive<CreateServiceMessage>("service.create", request => CreateNewService(request));
                bus.Receive<SuggestionMessage>("service.request" + id, request => HandleSuggestionRequest(request));
                Console.ReadLine();
                
            }
        }
    }
}
