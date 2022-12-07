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
                worker.AddBuilding(3, 3, 9);      //Test data                        
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            worker.OpenServerWindow();
            worker.OpenOutputWindow();
            worker.StartSim();
            worker.CallElevator(1, worker.AddPassenger(new List<int>() { 4, 5, 2 }));//Test data

            bool quit = false;
            Console.WriteLine("Press Q to quit");
            while (!quit)
            {
                Console.WriteLine("Call elevator to floor number:");
                ConsoleKeyInfo key = Console.ReadKey();
                if(key.KeyChar == 'q' || key.KeyChar == 'Q') quit= true;
                Int32 currrentFloor, destination;
                if (Int32.TryParse(key.KeyChar.ToString(), out currrentFloor))
                {
                    Console.WriteLine();
                    Console.WriteLine("Which floor do you want to go?");
                    key = Console.ReadKey();
                    if (Int32.TryParse(key.KeyChar.ToString(), out destination))
                    {
                        worker.CallElevator(currrentFloor, destination);
                    }
                    Console.Clear();
                }
            }
            worker.StopSim();
        }

    }
}
