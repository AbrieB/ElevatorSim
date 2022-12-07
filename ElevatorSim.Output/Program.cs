using ElevatorSim.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorSim.Output
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Timer _timer = new Timer(TimerCallback, null, 0, 1000);
            Console.ReadLine();
        }

        private static void TimerCallback(Object o)
        {
            try
            {
                CallServer("http://localhost:8001/GetBuildingView/");
            }
            catch 
            {
                Console.WriteLine("Server not reachable");
            }            
        }

        private static string CallServer(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";;

            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            var json = JsonConvert.DeserializeObject<ServerRespone>(result);
            if (json.Success)
            {
                Console.Clear();
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
