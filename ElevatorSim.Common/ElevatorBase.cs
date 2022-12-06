using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSim.Common
{
    public class ElevatorBase
    {
        public Enums.ElevatorStatus Status { get; set; } = Enums.ElevatorStatus.Stationary;
        public int CurrentFloor { get; set; } = 1;
        public int Destination { get; set; }
    }
}
