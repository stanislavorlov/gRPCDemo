using Grpc.Net.Client;
using SimpleGrpcService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var greeterClient = new Greeter.GreeterClient(channel);

            Console.WriteLine("Please enter name: ");
            string name = Console.ReadLine();

            var reply = await greeterClient.SayHelloAsync(new HelloRequest { Name = name });

            Console.WriteLine($"Server response: {reply.Message}");

            var customerClient = new Customer.CustomerClient(channel);

            var clientRequested = new CustomerLookupModel { UserId = 1 };

            var customer = await customerClient.GetCustomerInfoAsync(clientRequested);

            Console.WriteLine($"{customer.FirstName} {customer.LastName}");
            Console.WriteLine();
            Console.WriteLine("New Customer List");
            Console.WriteLine();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            using var call = customerClient.GetNewCustomers(new NewCustomerRequest { },
                cancellationToken: token);

            while (await call.ResponseStream.MoveNext(token))
            {
                var currentCust = call.ResponseStream.Current;

                Console.WriteLine($"{currentCust.FirstName} {currentCust.LastName}: {currentCust.Email}");
            }

            Console.ReadKey();
        }
    }
}
