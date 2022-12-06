using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSim.Common
{
    public class Floor
    {
        public int FloorNumber { get; set; }
        public List<Passenger> PassengersWaiting { get; set; } = new List<Passenger>();

        public Floor(int id)
        {
            FloorNumber = id;
        }
    }
}
