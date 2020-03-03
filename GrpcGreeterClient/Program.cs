using System;
using System.Net.Http;
using System.Threading.Tasks;
using GrpcGreeter;
using Grpc.Net.Client;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GrpcGreeterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

            var creply = await client.SayCheersAsync(new CheersRequest
            {
                Bol = true,
                Name = "HUGO",
                Stringlt = { new List<string> { "A", "B", "C", "D", "E" } },
                Numberlt = { new List<int>() { 1, 2, 3, 4, 5 } },
                Birthday = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            });
            Console.WriteLine("Cheers: " + creply.Message);
            Console.WriteLine("Cheers String List: " + creply.Stringlt);
            Console.WriteLine("Cheers Number List: " + creply.Numberlt);
            Console.WriteLine("Cheers TimeSpan: " + creply.Birthday);
            Console.WriteLine("Cheers Objects: " + creply.Results);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public static class ByteStringUtility
    {
        public static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
    }
}