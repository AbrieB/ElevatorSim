using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSim.Common;
using Newtonsoft.Json;

namespace ElevatorSim.Service
{
    public class ServerWorker
    {
        private Building ThisBuilding { get; set; }
        public ServerRespone UpdateBuilding(Building building)
        {
            try
            {
                ThisBuilding = building;
                return new ServerRespone() { Success = true, Data = DrawBuilding() };
            }
            catch(Exception ex)
            {
                return new ServerRespone() { Success = false, Data = ex.Message };
            }           
        }

        private string DrawBuilding()
        {
            Console.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (Floor floor in ThisBuilding.Floors)
            {
                sb.Append($"Floor {floor.FloorNumber}  :");

                foreach (PeopleElevator elevator in ThisBuilding.PeopleElevators)
                {
                    if (elevator.CurrentFloor == floor.FloorNumber)
                    {
                        string paxCount = $"{elevator.Passangers.Count}";
                        sb.Append(paxCount.PadLeft(5-elevator.Passangers.Count().ToString().Length,'_'));
                    }
                    else
                    {
                        sb.Append("___ ");
                    }                    
                };
                sb.Append($" -  {floor.PassengersWaiting.Count} people waiting");
                sb.AppendLine("");
            }
            return sb.ToString();
        }        
    }
}
