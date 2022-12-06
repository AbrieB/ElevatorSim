using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSim.Common
{
    public class Passenger
    {
        public int DestinationFloor { get; set; }

        public Passenger()
        {

        }

        public Passenger(int Destination)
        {
            DestinationFloor = Destination;
        }
    }
}
