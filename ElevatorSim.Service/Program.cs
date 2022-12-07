using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ElevatorSim.Common;
using Newtonsoft;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.ComponentModel;

namespace ElevatorSim.Service
{
    class Program
    {
        #region Listener and responder
        static void Main(string[] args)
        {
            
            using (var listener = new HttpListener())
            {
                try
                {
                    listener.Prefixes.Add("http://localhost:8001/");

                    listener.Start();

                    Console.WriteLine("Listening on port 8001...");
                    bool running = true;
                    ServerWorker worker = new ServerWorker();
                    while (running)
                    {
                        HttpListenerContext ctx = listener.GetContext();
                        using (HttpListenerResponse resp = ctx.Response)
                        {
                            HttpListenerRequest request = ctx.Request;
                            ASCIIEncoding encoding = new ASCIIEncoding();
                            var options = new Newtonsoft.Json.JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented
                            };
                                                
                            var input = JsonConvert.SerializeObject(ProcessRequest(request, worker), options);
                            byte[] byte1 = encoding.GetBytes(input);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(input);                     

                            resp.StatusCode = (int)HttpStatusCode.OK;
                            resp.StatusDescription = "Status OK";
                            HttpListenerResponse response = ctx.Response;
                            // Construct a response.
                            response.ContentLength64 = buffer.Length;
                            System.IO.Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            // You must close the output stream.
                            output.Close();
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("There is already an server running on this port");
                    Console.ReadKey();
                }
            }
        }

        private static ServerRespone ProcessRequest(HttpListenerRequest request,ServerWorker worker)
        {
            try
            {
                Console.WriteLine(request.UserHostAddress);
                Console.WriteLine(request.RawUrl);
                string response = string.Empty;
                ServerRespone rsp = new ServerRespone();
                
                if (request.RawUrl.ToLower().Contains("getupdatedbuilding"))
                {
                    rsp = worker.GetUpdatedBuilding();
                    return rsp;
                }
                if (request.RawUrl.ToLower().Contains("getbuildingview"))
                {
                    rsp = worker.GetBuildingView();
                    return rsp;
                }
                if (request.ContentLength64 > 0)
                {
                    var body = request.InputStream;
                    var encoding = request.ContentEncoding;
                    var reader = new StreamReader(body, encoding);

                    Console.WriteLine("Client data content type {0}", request.ContentType);
                    Console.WriteLine("Client data content length {0}", request.ContentLength64);
                    Console.WriteLine("Start of data:");
                    response = reader.ReadToEnd();
                    
                    if (request.RawUrl.ToLower().Contains("addbuilding")|| request.RawUrl.ToLower().Contains("callelevator"))
                    {
                        Building tmp = JsonConvert.DeserializeObject<Building>(response);
                        rsp = worker.UpdateBuilding(tmp);
                    }

                    Console.WriteLine(rsp.Data);
                    return rsp;
                }
                else
                {
                    return new ServerRespone() { Success = true };
                }                
            }
            catch (Exception ex)
            {
                return new ServerRespone() { Success = false, ErrorMessage = ex.Message };
            }
        }
        #endregion


    }
}
