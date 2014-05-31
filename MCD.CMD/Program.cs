using ManyConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MCD.CMD
{
    public class Program
    {
        public static int Main(String[] args)
        {
            IEnumerable<ConsoleCommand> commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
            int result = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            return result;
        }
    }
}
