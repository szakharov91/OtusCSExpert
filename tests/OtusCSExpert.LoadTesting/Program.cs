using System.Text;
using NBomber.Contracts;
using NBomber.CSharp;
using OtusCSExpert.Common.Utils.Server;
using OtusCSExpert.LoadTesting.Utils;

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
                var randomInt = _rand.Next(_randomOptions.MinValue, _randomOptions.MaxValue);

                string key = GenerateRandomString(_randomOptions.KeyLength);
                byte[] value = Encoding.UTF8.GetBytes(GenerateRandomString(_randomOptions.ValueLength));

                try
                {
                    // Создаём клиента внутри шага
                    using ISimpleTcpClient client = new SimpleTcpClient("127.0.0.1", 8080);
                    await client.ConnectAsync();

                    byte[] response = randomInt % 2 == 0 ? await client.SetAsync(key, value) : await client.GetAsync(key);

                    if (response.Length == _randomOptions.ValueLength)
                        return Response.Ok(); // GET вернул сохранённое значение

                    if (response.Length == 0)
                        return Response.Fail(); // Всегда ошибка

                    string encodedMessage = Encoding.UTF8.GetString(response);
                    return encodedMessage switch
                    {
                        ErrorResponses.AsString.OkResponse => Response.Ok(),
                        ErrorResponses.AsString.NilResponse => Response.Ok(),
                        ErrorResponses.AsString.UnknownCommandResponse => Response.Fail(),
                        _ => Response.Fail(), // всё остальное — ошибка
                    };
                }
                catch (Exception)
                {
                    // на случай, если у нас происходит какой-то сетевой сбой
                    return Response.Fail();
                }
            });

            return response;
        })
        .WithLoadSimulations(
            // разогрев (10 р/с, 5 сек)
            Simulation.Inject(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)),

            // основная нагрузка
            Simulation.Inject(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30))
        );

        string reportsFolder = Path.Combine(
            VisualStudioProvider.GetPathToPrerequisites(VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName),
            "NBomber_Reports"
            );

        var nbomber = NBomberRunner.RegisterScenarios(scenario);

        if (Directory.Exists(reportsFolder))
        {
            nbomber = nbomber.WithReportFolder(Path.Combine(reportsFolder, DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")));
        }

        nbomber.Run();

        Console.ReadLine();
    }
}
