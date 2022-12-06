using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSim.Common;

namespace ElevatorSim
{
    class Program
    {
        static void Main(string[] args)
        {
            ElevatorWorker worker = new ElevatorWorker();
            try
            {
                worker.AddBuilding(3, 3, 5);                              
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            worker.OpenServerWindow();
            worker.StartSim();
            worker.CallElevator(1, worker.AddPassenger(new List<int>() { 4, 5, 2 }));
            ConsoleKeyInfo key = Console.ReadKey();
            while (key.KeyChar !='q' && key.KeyChar != 'Q')
            {
                key = Console.ReadKey();
            }
            worker.StopSim();
        }
    }
}
