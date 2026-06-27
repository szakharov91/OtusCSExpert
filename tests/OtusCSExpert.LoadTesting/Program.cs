using System.Text;
using NBomber.Contracts;
using NBomber.CSharp;
using OtusCSExpert.Common.Utils.Server;

namespace OtusCSExpert.LoadTesting;

public record RandomOptions(int MinValue, int MaxValue, int KeyLength, int ValueLength);

public class Program
{
    private static readonly RandomOptions _randomOptions = new RandomOptions(0, 1000, 6, 100);
    private static readonly Random _rand = Random.Shared;
    private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(int length)
    {
        return new string(Enumerable.Repeat(_chars, length)
                                    .Select(s => s[_rand.Next(s.Length)])
                                    .ToArray());
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, NBomber!");

        await Task.Delay(TimeSpan.FromSeconds(3)); // ждем запуска основного приложения

        var scenario = Scenario.Create("tcp_server_load_test", async context =>
        {
            // Внутри сценария определяем шаг с помощью Step.Run
            var response = await Step.Run("client_step", context, async () =>
            {
                // Создаём клиента внутри шага
                using ISimpleTcpClient client = new SimpleTcpClient("127.0.0.1", 8080);
                await client.ConnectAsync();

                var randomInt = _rand.Next(_randomOptions.MinValue, _randomOptions.MaxValue);

                string key = GenerateRandomString(_randomOptions.KeyLength);
                byte[] value = Encoding.UTF8.GetBytes(GenerateRandomString(_randomOptions.ValueLength));

                byte[] response = randomInt % 2 == 0 ? await client.SetAsync(key, value) : await client.GetAsync(key);

                string encodedMessage = Encoding.UTF8.GetString(response);

                return encodedMessage switch
                {
                    ErrorResponses.AsString.OkResponse => Response.Ok(),
                    ErrorResponses.AsString.NilResponse => Response.Fail(),
                    ErrorResponses.AsString.UnknownCommandResponse => Response.Fail(),
                    _ => Response.Ok(),
                };
            });

            return response;
        })
        .WithLoadSimulations(
            // разогрев (10 р/с, 5 сек)
            Simulation.Inject(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)),

            // основная нагрузка
            Simulation.Inject(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Console.ReadLine();
    }
}
