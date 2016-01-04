using System.Threading.Tasks;

namespace Takenet.VirtualAssistant
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static Task MainAsync(string[] args)
        {            
            return Task.FromResult(0);
        }
    }
}
