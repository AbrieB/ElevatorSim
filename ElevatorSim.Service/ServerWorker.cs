using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElevatorSim.Common;
using Newtonsoft.Json;

namespace ElevatorSim.Service
{
    public class ServerWorker
    {
        private Building ThisBuilding { get; set; }
        BackgroundWorker elevatorWorker = new BackgroundWorker();
        

        public ServerRespone UpdateBuilding(Building building)
        {
            try
            {
                ThisBuilding = building;
                elevatorWorker.RunWorkerAsync();
                return new ServerRespone() { Success = true, Data = DrawBuilding() };
            }
            catch(Exception ex)
            {
                return new ServerRespone() { Success = false, Data = ex.Message };
            }           
        }

        internal ServerRespone GetBuildingView()
        {
            return new ServerRespone() { Success = true, Data = DrawBuilding() };
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

        private void elevatorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //If any elevator or florr has persons loop
            while (ThisBuilding.Floors.Any(f=> f.PassengersWaiting.Count()>0) 
                    || ThisBuilding.PeopleElevators.Any(el=> el.Passangers.Count()>0))
            {
                //Logic:
                //If there are people waiting send closest elevator
                //Loop through elevators
                //     -check if elevator with passanger is moving and set status
                //     -chech if current floor has dropoffs
                //     -check if current floor has pick ups
                //Move elevators (sleep ThisBuilding.SecondsBetweenFloors 
                foreach (Floor floor in ThisBuilding.Floors)
                {
                    AssignElevator(floor);
                }
                foreach (PeopleElevator elevator in ThisBuilding.PeopleElevators)
                {
                    if (elevator.Passangers.Count>0 && elevator.Status == Enums.ElevatorStatus.Stationary)
                    {
                        if (elevator.Passangers.Count(x=> x.DestinationFloor> elevator.CurrentFloor)> elevator.Passangers.Count(x => x.DestinationFloor < elevator.CurrentFloor))
                        {
                            elevator.Status = Enums.ElevatorStatus.MovingUp;
                            elevator.Destination = elevator.Passangers.Max(x => x.DestinationFloor);
                        }
                        else
                        {
                            elevator.Status = Enums.ElevatorStatus.MovingDown;
                            elevator.Destination = elevator.Passangers.Min(x => x.DestinationFloor);
                        }                        
                    }
                    else
                    {
                        DropOffPeople(elevator.CurrentFloor, elevator);
                        PickUpPeople(elevator.CurrentFloor, elevator);
                    }
                }
                MoveElevators();
            }
        }

        private void PickUpPeople(int floorNumber, PeopleElevator elevator)
        {
            Floor floor = ThisBuilding.Floors.FirstOrDefault(x => x.FloorNumber == floorNumber);
            if (elevator.Status==Enums.ElevatorStatus.MovingUp)
            {
                List<Passenger> goingUp = floor.PassengersWaiting.Where(x => x.DestinationFloor > floor.FloorNumber).ToList();
                if (goingUp.Count>0)
                {
                    int capacity = ThisBuilding.MaxPassengers - elevator.Passangers.Count();
                    elevator.Passangers.AddRange(goingUp.Take(capacity));
                }
            }
            if (elevator.Status == Enums.ElevatorStatus.MovingDown)
            {
                List<Passenger> goingDown = floor.PassengersWaiting.Where(x => x.DestinationFloor > floor.FloorNumber).ToList();
                if (goingDown.Count > 0)
                {
                    int capacity = ThisBuilding.MaxPassengers - elevator.Passangers.Count();
                    elevator.Passangers.AddRange(goingDown.Take(capacity));
                }
            }
        }

        private void DropOffPeople(int floorNumber, PeopleElevator elevator)
        {
            Floor floor = ThisBuilding.Floors.FirstOrDefault(x => x.FloorNumber == floorNumber);
            List<Passenger> selection = elevator.Passangers.Where(x => x.DestinationFloor == floor.FloorNumber).ToList();
            if (selection.Count>0)
            {
                foreach (Passenger passanger in selection)
                {
                    elevator.Passangers.Remove(passanger);
                }
                if (elevator.Passangers.Count==0 && elevator.Destination == floor.FloorNumber)
                {
                    elevator.Status = Enums.ElevatorStatus.Stationary;
                }
            }
        }

        private void AssignElevator(PeopleElevator elevator, int floorNumber)
        {
            elevator.Destination = floorNumber;
            if (elevator.CurrentFloor<floorNumber)
            {
                elevator.Status = Enums.ElevatorStatus.MovingUp;
                elevator.Destination = (floorNumber > elevator.Destination ? floorNumber : elevator.Destination);
            }
            else
            {
                elevator.Status = Enums.ElevatorStatus.MovingDown;
                elevator.Destination = (floorNumber > elevator.Destination ? floorNumber : elevator.Destination);
            }
        }

        private PeopleElevator GetClosestElevator(int floorNumber)
        {
            //Select all elevator moving in right direction or standing still not at capacity
            List<PeopleElevator> selection = ThisBuilding.PeopleElevators.Where(x => ((x.Status ==  Enums.ElevatorStatus.MovingDown && x.CurrentFloor> floorNumber)
                                                                            || x.Status == Enums.ElevatorStatus.Stationary) 
                                                                            && x.Passangers.Count()<ThisBuilding.MaxPassengers).ToList();
            selection.AddRange(ThisBuilding.PeopleElevators.Where(x => ((x.Status == Enums.ElevatorStatus.MovingUp && x.CurrentFloor < floorNumber)
                                                                            || x.Status == Enums.ElevatorStatus.Stationary)
                                                                            && x.Passangers.Count() < ThisBuilding.MaxPassengers).ToList());
            return selection.OrderBy(item => Math.Abs(item.CurrentFloor - floorNumber)).First();
        }

        private void MoveElevators()
        {
            for (int i = 0; i < ThisBuilding.PeopleElevators.Count(); i++)
            {
                PeopleElevator elevator = ThisBuilding.PeopleElevators[i];
                
                switch (elevator.Status)
                {
                    case Enums.ElevatorStatus.MovingUp:
                        if (elevator.CurrentFloor == elevator.Destination)
                        {
                            elevator.Status = Enums.ElevatorStatus.Stationary;
                        }
                        else
                        {
                            elevator.CurrentFloor++;
                        }
                        break;
                    case Enums.ElevatorStatus.MovingDown:
                        if (elevator.CurrentFloor == elevator.Destination)
                        {
                            elevator.Status = Enums.ElevatorStatus.Stationary;
                        }
                        else
                        {
                            elevator.CurrentFloor--;
                        }
                        break;
                    case Enums.ElevatorStatus.Stationary:
                        break;
                    case Enums.ElevatorStatus.UnderMaimtenance:
                        break;
                    default:
                        break;
                }
            }
        }

        private void AssignElevator(Floor floor)
        {
            List<PeopleElevator> goingUp= ThisBuilding.PeopleElevators.Where(x=> x.Status==Enums.ElevatorStatus.MovingUp).ToList();
            List<PeopleElevator> goingDown = ThisBuilding.PeopleElevators.Where(x => x.Status == Enums.ElevatorStatus.MovingDown).ToList();
            List<Passenger> passengersGoinUp = floor.PassengersWaiting.Where(x=> x.DestinationFloor>floor.FloorNumber).ToList();
            List<Passenger> passengersGoinDown = floor.PassengersWaiting.Where(x => x.DestinationFloor < floor.FloorNumber).ToList();
            foreach (Passenger passanger in passengersGoinDown)
            {
                if (passengersGoinDown.Min(x=> x.DestinationFloor)< goingDown.Min(x=> x.Destination) )
                {
                    PeopleElevator elevator = GetClosestElevator(floor.FloorNumber);
                    AssignElevator(elevator,floor.FloorNumber);
                }
            }
            foreach (Passenger passanger in passengersGoinUp)
            {
                if (passengersGoinUp.Max(x => x.DestinationFloor) > goingUp.Max(x => x.Destination))
                {
                    PeopleElevator elevator = GetClosestElevator(floor.FloorNumber);
                    AssignElevator(elevator, floor.FloorNumber);
                }
            }
        }
    }
}
