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
using Newtonsoft.Json;

namespace ElevatorSim
{
    public class ElevatorWorker
    {
        public Building ThisBuilding { get; private set; }        
        private Process server;
        private Process outputWindow;
        
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

        #region Call elevator
        public void CallElevator(int FloorNumber, List<Passenger> Passangers)
        {
            UpdateBuilding();
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
            if (FloorNumber < 1 || FloorNumber > ThisBuilding.Floors.Count())
            {
                throw new Exception("Not a valid floor");
            }

            if (Destination < 1 || Destination > ThisBuilding.Floors.Count())
            {
                throw new Exception("Not a valid floor");
            }

            if (FloorNumber == Destination)
            {
                return;
            }
            UpdateBuilding();
            ThisBuilding.Floors.First(x => x.FloorNumber == FloorNumber).PassengersWaiting.Add( new Passenger(Destination));
            CallServer("http://localhost:8001/CallElevator,ThisBuilding", ThisBuilding);
        }
        #endregion

        #region Stop and start sim
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
            server.Kill();
            outputWindow.Kill();
        }
        #endregion

        public List<Passenger> AddPassenger(List<int> DestinationFloor)
        {
            UpdateBuilding();
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

        #region Open windows
        public void OpenServerWindow()
        {
            if (server == null)
            {
                ProcessStartInfo psi = new ProcessStartInfo("ElevatorSim.Service.exe")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                server = Process.Start(psi);
            }
        }

        public void OpenOutputWindow()
        {
            if (outputWindow == null)
            {
                ProcessStartInfo psi = new ProcessStartInfo("ElevatorSim.Output.exe")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                outputWindow = Process.Start(psi);
            }
        }
        #endregion

        private string UpdateBuilding()
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create("http://localhost:8001/GetUpdatedBuilding/");
            webrequest.Method = "GET";

            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            ServerRespone json = JsonConvert.DeserializeObject<ServerRespone>(result);
            if (json.Success)
            {
                ThisBuilding = JsonConvert.DeserializeObject<Building>(json.Data);
            }
            else
            {
                Console.Write(json.ErrorMessage);
            }

            webresponse.Close();
            return result;
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
            var input = System.Text.Json.JsonSerializer.Serialize(building, options);

            byte[] byte1 = encoding.GetBytes(input);

            webrequest.GetRequestStream().Write(byte1, 0, byte1.Length);

            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            var json = System.Text.Json.JsonSerializer.Deserialize<ServerRespone>(result);
            if (!json.Success)
            {
                Console.Write(json.ErrorMessage);
            }            
            webresponse.Close();
            return result;
        }
    }
}
