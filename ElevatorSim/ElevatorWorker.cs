using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSim.Common;
using System.Net;
using System.Web;
using System.Text.Json;

namespace ElevatorSim
{
    public class ElevatorWorker
    {
        public Building ThisBuilding { get; private set; }
        
        private Process server;
        
        #region AddBuilding
        public void AddBuilding(int PeopleElevators, int FreightElevaors, int Floors, int MaxpassangersPerElevator = 10)
        {
            ValidateBuilding(PeopleElevators, Floors);

            this.ThisBuilding = new Building()
            {
                Floors = AddFloor(Floors),
                FreightElevators = AddFreightElevator(FreightElevaors),
                MaxPassengers = MaxpassangersPerElevator,
                PeopleElevators = AddPeopleElevator(PeopleElevators)
            };            
        }

        private List<PeopleElevator> AddPeopleElevator(int count)
        {
            List<PeopleElevator> retVal = new List<PeopleElevator>();
            for (int i = 1; i <= count; i++)
            {
                retVal.Add(new PeopleElevator());
            }
            return retVal;
        }

        private List<FreightElevator> AddFreightElevator(int count)
        {
            List<FreightElevator> retVal = new List<FreightElevator>();
            for (int i = 1; i <= count; i++)
            {
                retVal.Add(new FreightElevator());
            }
            return retVal;
        }

        private List<Floor> AddFloor(int count)
        {
            List<Floor> retVal = new List<Floor>();
            for (int i = 1; i <= count; i++)
            {
               
                retVal.Add(new Floor(i));
            }
            return retVal;
        }

        
        private void ValidateBuilding(int PeopleElevators, int Floors)
        {
            if (ThisBuilding != null)
            {
                throw new Exception("Please remove old building first");
            }

            if (PeopleElevators < 1)
            {
                throw new Exception("Please add at least one elevator");
            }

            if (Floors < 3)
            {
                throw new Exception("Please add at least 3 floors");
            }
        }
        #endregion

        public void CallElevator(int FloorNumber, List<Passenger> Passangers)
        {
            if (Passangers.Count()<1)
            {
                throw new Exception("No passangers on floor");
            }

            if (FloorNumber < 0 || FloorNumber > ThisBuilding.Floors.Count())
            {
                throw new Exception("Not a valid floor");
            }

            if (Passangers.Any(x=> x.DestinationFloor == FloorNumber))
            {
                throw new Exception("One of the passangers have an invalid floor number (Same as current floor)");
            }

            ThisBuilding.Floors.First(x => x.FloorNumber == FloorNumber).PassengersWaiting.AddRange(Passangers);
            CallServer("http://localhost:8001/CallElevator,ThisBuilding",ThisBuilding);
        }

        public void CallElevator(int FloorNumber, int Destination)
        {
            ThisBuilding.Floors.First(x => x.FloorNumber == FloorNumber).PassengersWaiting.Add( new Passenger(Destination));
            CallServer("http://localhost:8001/CallElevator,ThisBuilding", ThisBuilding);
        }

        public void StartSim()
        {
            if (this.ThisBuilding== null)
            {
                throw new Exception("Please add a building first");
            }
            CallServer("http://localhost:8001/AddBuilding", ThisBuilding);
        }

        public void StopSim()
        {
            ThisBuilding = null;
            server.Close();
        }

        public List<Passenger> AddPassenger(List<int> DestinationFloor)
        {
            if (DestinationFloor.Any(x=> x<0 || x>ThisBuilding.Floors.Count()))
            {
                throw new Exception("Invalid floor number in list");
            }
            List<Passenger> retVal = new List<Passenger>();
            foreach (var item in DestinationFloor)
            {
                retVal.Add(
                    new Passenger() { DestinationFloor = item }
                );
            }
            return retVal;
        }

        public void OpenServerWindow()
        {
            if (server == null)
            {
                ProcessStartInfo psi = new ProcessStartInfo("ElevatorSim.Service.exe")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                server = Process.Start(psi);
            }
        }

        private string CallServer(string url, Building building)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/json";
  
            ASCIIEncoding encoding = new ASCIIEncoding();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var input = JsonSerializer.Serialize(building, options);

            byte[] byte1 = encoding.GetBytes(input);

            webrequest.GetRequestStream().Write(byte1, 0, byte1.Length);

            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            var json = JsonSerializer.Deserialize<ServerRespone>(result);
            if (json.Success)
            {
                Console.Write(json.Data);
            }
            else
            {
                Console.Write(json.ErrorMessage);
            }
            
            webresponse.Close();
            return result;
        }
    }
}
