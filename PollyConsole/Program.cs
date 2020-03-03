using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PollyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Wrap();
            Console.WriteLine($"[App] {DateTime.Now.ToString(CultureInfo.InvariantCulture)}: 結束發送 Request");
            Console.WriteLine("按任意建退出.......");
            Console.ReadKey();
        }

        static HttpResponseMessage ExecuteMockRequest()
        {

            Console.WriteLine($"[App] {DateTime.Now.ToString(CultureInfo.InvariantCulture)}: 開始發送 Request");
            HttpResponseMessage result;
            using (HttpClient client = new HttpClient())
            {
                //client.Timeout = TimeSpan.FromMilliseconds(6000);
                result = client.GetAsync("http://www.mocky.io/v2/5e5607bd300000550028e238?mocky-delay=5000ms").Result;
            }
            return result;
        }

        static string doMockHTTPRequest(bool check = false)
        {
            Console.WriteLine($"[App] {DateTime.Now.ToString(CultureInfo.InvariantCulture)}: 開始發送 Request");
            HttpResponseMessage result;
            using (HttpClient client = new HttpClient())
            {
                //client.Timeout = !check ? TimeSpan.FromMilliseconds(500) : TimeSpan.FromMilliseconds(5000);
                //client.Timeout = TimeSpan.FromMilliseconds(500);
                result = client.GetAsync("http://www.mocky.io/v2/5e5607bd300000550028e238?mocky-delay=2000ms").Result;
            }

            return result.Content.ReadAsStringAsync().Result;
        }

        static void Retry()
        {
            Policy
                .Handle<HttpRequestException>()
                //指定錯誤返回
                .OrResult<HttpResponseMessage>(result => result.StatusCode != HttpStatusCode.OK)
                //.WaitAndRetry(new[]
                //{
                //TimeSpan.FromSeconds(1),
                //TimeSpan.FromSeconds(5),
                //TimeSpan.FromSeconds(2)
                //}, (exception, timeSpan) =>
                //{
                //    Console.WriteLine($"[App|Polly] : 呼叫 API 異常,TimeSpan {timeSpan}, Error :{exception.Result.StatusCode}");
                //})

                //.RetryForever(onRetry: exception =>
                //{
                //    Console.WriteLine($"[App|Polly] : 呼叫 API 異常, Error :{exception.Result.StatusCode}");
                //})

                .Retry(3, onRetry: (exception, retryCount) =>
                {
                    Console.WriteLine($"[App|Polly] : 呼叫 API 異常, 進行第 {retryCount} 次重試, Error :{exception.Result.StatusCode}");
                })

                .Execute(ExecuteMockRequest);

        }

        static void Timeout()
        {
            Policy
            .Timeout(TimeSpan.FromSeconds(1),TimeoutStrategy.Pessimistic, onTimeout: (context, timespan, task) =>
            {
                Console.WriteLine($"TimeOut => {context.PolicyKey} : execution timed out after {timespan} seconds.");
            })
            .Execute(ExecuteMockRequest);

        }

        static void Wrap()
        {
            bool check = false;

            RetryPolicy waitAndRetryPolicy = Policy
            .Handle<Exception>()
            .Retry(6,
                onRetry: (exception, retryCount) =>
                {
                    Console.WriteLine($"====Retry===== : 呼叫 API 異常, 進行第 {retryCount} 次重試");
                    if (retryCount == 5)
                    {
                        check = true;
                    }
                });

            TimeoutPolicy timeoutPolicys = Policy
                .Timeout(TimeSpan.FromMilliseconds(1000),TimeoutStrategy.Pessimistic,
                    onTimeout: (context, timespan, task) =>
                    {
                        Console.WriteLine($"====TimeOut===== : execution timed out after {timespan} seconds.");
                    });


            FallbackPolicy<String> fallbackForAnyException = Policy<String>
                .Handle<Exception>()
                .Fallback(
                    fallbackAction: () => { Console.WriteLine("999999999"); return "123"; },
                    onFallback: e => { Console.WriteLine($"[Polly fallback] : 重試失敗, say goodbye"); }
                );

            CircuitBreakerPolicy circuitPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(3, TimeSpan.FromSeconds(0), (ex, ts) =>
                {
                    Console.WriteLine($"====CircuitBreaker [OnBreak]=====  ts = {ts.Seconds}s ,ex.message = {ex.Message}");
                }, () =>
                {
                    Console.WriteLine("AService OnReset");
                });
            try
            {
                PolicyWrap<String> policyWrap = fallbackForAnyException
                .Wrap(waitAndRetryPolicy)
                .Wrap(circuitPolicy)
                .Wrap(timeoutPolicys);
                policyWrap.Execute(() => doMockHTTPRequest(check));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }
    }
}

