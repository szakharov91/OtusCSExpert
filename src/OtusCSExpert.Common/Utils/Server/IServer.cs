using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusCSExpert.Common.Utils.Server;

public interface IServer : IDisposable
{
    Task StartAsync(CancellationToken cancellationToken = default);
    void Stop();
}
