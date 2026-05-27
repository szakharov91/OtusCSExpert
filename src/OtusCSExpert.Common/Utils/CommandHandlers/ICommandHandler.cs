using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtusCSExpert.Common.Types;

namespace OtusCSExpert.Common.Utils.CommandHandlers;

public interface ICommandHandler
{
    void Execute(ParsedCommand command);
}
