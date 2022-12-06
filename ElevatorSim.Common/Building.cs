using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSim.Common
{
    public class Building
    {
        public int MaxPassengers { get; set; } = 10;
        public List<FreightElevator> FreightElevators { get; set; } = new List<FreightElevator>();
        public List<PeopleElevator> PeopleElevators { get; set; } = new List<PeopleElevator>();
        public List<Floor> Floors { get; set; } = new List<Floor>();
        public int SecondsBetweenFloors { get; set; } = 3;
        public int SecondsCollectingPassengers { get; set; } = 5;
        public bool AllowPartialFloorPickup { get; set; } = true;
    }
}
