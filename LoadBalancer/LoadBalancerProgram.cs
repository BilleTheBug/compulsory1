using EasyNetQ;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class LoadBalancerProgram
    {
        private static int serviceId;
        private readonly static float MAX_SERVER_LOAD = 50;
        private static IBus iBus;
        private static Dictionary<int, float> serviceLoadMap;
        private static List<int> activeServices;

        static void Main(string[] args)
        {
            InitializeServiceData();
            UserInputHandler();
        }

        //Handles requests from the client and sends requests to the least loaded service.
        private static void HandleSuggestionRequest(SuggestionMessage suggestion)
        {
            int bestAvailableService = 0;
            foreach (var service in serviceLoadMap)
            {
                if (service.Value < serviceLoadMap[bestAvailableService])
                    bestAvailableService = service.Key;
            }
            iBus.Send<SuggestionMessage>("service.request" + bestAvailableService, suggestion);
            activeServices.Add(bestAvailableService);
            Console.WriteLine("Suggestion request sent to service " + bestAvailableService);
        }

        //Handles replies from services and sends replies to the client, if a suggestion is found.
        private static void HandleSuggestionReply(SuggestionReplyMessage suggestion)
        {
            if (suggestion.Word != null)
            {
                Console.WriteLine("Suggestion recieved from service " + suggestion.ServiceID + ". Serverload: " + suggestion.ServerLoad + "%");
                iBus.Send<SuggestionMessage>("loadbalancer.result", new SuggestionMessage() { Word = suggestion.Word });
            }
            else
                Console.WriteLine("No or perfect match found in service " + suggestion.ServiceID + ". Serverload: " + suggestion.ServerLoad + "%");
            serviceLoadMap[suggestion.ServiceID] = suggestion.ServerLoad;
            activeServices.Remove(suggestion.ServiceID);
            RemoveUnusedServices();
            CheckServerLoad();
        }

        //
        private static void CheckServerLoad()
        {
            float totalLoad = 0;
            foreach (var service in serviceLoadMap)
            {
                totalLoad += service.Value;
            }
            if (totalLoad / serviceLoadMap.Count >= MAX_SERVER_LOAD)
            {
                createService();
            }
        }

        //Removes the server with least load, if it isn´t active, and all other services are not overloaded.
        private static void RemoveUnusedServices()
        {
            if (serviceLoadMap.Count > 1)
            {
                int overloadedServices = 0;
                int leastLoadedService = 0;
                foreach (var service in serviceLoadMap)
                {
                    if (service.Value >= MAX_SERVER_LOAD)
                        overloadedServices++;
                    if (service.Value < serviceLoadMap[leastLoadedService])
                        leastLoadedService = service.Key;
                }
                if (!activeServices.Contains(leastLoadedService) && serviceLoadMap.Count > overloadedServices + 1)
                {
                    Console.WriteLine("Service" + leastLoadedService + " removed due to inactivity.");
                    serviceLoadMap.Remove(leastLoadedService);
                }
            }
        }

        //Creates a new service.
        private static void createService()
        {
            int id = ++serviceId;
            iBus.Send("service.create", new CreateServiceMessage() { ServiceId = id });
            serviceLoadMap.Add(id, 0);
            Console.WriteLine("New service created with ID: " + id);
        }

        //Removes an existing service.
        private static void removeService(int toBeRemoved)
        {
            if (serviceLoadMap.ContainsKey(toBeRemoved))
            {
                serviceLoadMap.Remove(toBeRemoved);
                Console.WriteLine("Service " + toBeRemoved + " was successfully removed.");
            }
            else
                Console.WriteLine("No service with ID " + toBeRemoved + " exists.");
        }

        //Displays the load on all services, and if they are active.
        private static void ServiceStatus()
        {
            foreach (var service in serviceLoadMap)
            {
                Console.WriteLine("Service " + service.Key + ", load: " + service.Value + ", active: " + activeServices.Contains(service.Key).ToString());
            }
        }

        //Handles user input in the console and initializes messaging. 
        private static void UserInputHandler()
        {
            Console.WriteLine("Enter 'create service' to create a new service. A new service is automaticly created if their serverload exceeds 50%.");
            Console.WriteLine("Enter 'remove service x' to remove an existing service.");
            Console.WriteLine("Enter 'service status' to show active services and their current load.");
            Console.WriteLine("Enter 'Quit' to quit.");
            Console.WriteLine("New service created with ID: 0");
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                iBus = bus;
                bus.Receive<SuggestionReplyMessage>("suggestion.reply", suggestion => HandleSuggestionReply(suggestion));
                bus.Receive<SuggestionMessage>("loadbalancer.request", suggestion => HandleSuggestionRequest(suggestion));
            
            var input = "";
            while ((input = Console.ReadLine()) != "Quit")
            {
                if (input.ToLower() == "create service")
                {
                    createService();
                }
                else if (input.Length > 15 && input.ToLower().Remove(15, 1) == "remove service ")
                {
                    int toBeRemoved;
                    if (int.TryParse(input.Remove(0, 15), out toBeRemoved))
                        removeService(toBeRemoved);
                    else
                        Console.WriteLine("Please enter a numeric value.");
                }
                else if (input.ToLower() == "service status")
                {
                    ServiceStatus();
                }

            }
            }
        }
        //Initializes all data connected to the services.
        private static void InitializeServiceData()
        {
            serviceLoadMap = new Dictionary<int, float>();
            serviceLoadMap.Add(serviceId, 0);
            activeServices = new List<int>();
        }
    }
}
