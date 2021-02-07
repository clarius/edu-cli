using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clarius.Edu.CLI
{
    public interface ICommand
    {
        List<string> GetSupportedCommands();
        Task<bool> Run(string[] args, bool verbose);
        bool CanHandle(string commandName);
        string Name { get; }
        string Description { get; }
        bool UseCache { get; }
        bool UseAppPermissions { get; }
        bool RequiresUser { get; }
    }
}
