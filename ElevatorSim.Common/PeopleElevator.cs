using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSim.Common
{
    public class PeopleElevator:ElevatorBase
    {
        public List<Passenger> Passangers { get; set; } = new List<Passenger>();
    }
}
