using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcGreeter
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Saying hello to {Name}", request.Name);
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<CheersReply> SayCheers(CheersRequest request, ServerCallContext context)
        {
            List<Result> results = new List<Result>();
            results.Add(new Result { Title = "1", Url = "www" });
            results.Add(new Result { Title = "2", Url = "www" });
            results.Add(new Result { Title = "3", Url = "www" });

            return Task.FromResult(
                new CheersReply
                {
                    Message = request.Bol ? "Cheers " + request.Name : "NOOOOOO",
                    Stringlt = { request.Stringlt },
                    Numberlt = {request.Numberlt},
                    Birthday = request.Birthday,
                    Results = { results }
                }) ;
        }
    }
}
