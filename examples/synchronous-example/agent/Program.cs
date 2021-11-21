using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HAProxy.StreamProcessingOffload.Agent;
using HAProxy.StreamProcessingOffload.Agent.Actions;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            int port = 12345;
            var listener = new TcpListener(address, port);
            var frameProcessor = new FrameProcessor()
            {
                EnableLogging = true,
                LogFunc = (msg) => Console.WriteLine(msg)
            };

            listener.Start();
            Console.WriteLine("Listening on {0}:{1}", address, port);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Task.Run(() =>
                {
                    NetworkStream stream = client.GetStream();

                    // Cancel stream when process terminates
                    System.AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        frameProcessor.CancelStream(stream);
                    };

                    frameProcessor.HandleStream(stream, (notifyFrame) =>
                    {
                        // NOTIFY frames contain HAProxy messages to the agent.
                        // The agent can send back "actions" to HAProxy via ACK frames.
                        var messages = ((ListOfMessagesPayload)notifyFrame.Payload).Messages;
                        var responseActions = new List<SpoeAction>();

                        if (messages.Any(msg => msg.Name == "my-message-name"))
                        {
                            var myMessage = messages.First(msg => msg.Name == "my-message-name");

                            // Each message may contain a collection of arguments, which hold the data.
                            TypedData myArg = myMessage.Args.First(arg => arg.Key == "ip").Value; 

                            // simulate a blocking API call that gets the IP score
                            // and takes 1 second
                            // Thread.Sleep(1000);
                            
                            int ip_score = 10;

                            if ((string)myArg.Value == "192.168.50.1")
                            {
                                ip_score = 20;
                            }

                            // You can send actions back to HAProxy, such as setting a variable.
                            SpoeAction setVar = 
                                new SetVariableAction(
                                    VariableScope.Session, 
                                    "ip_score", 
                                    new TypedData(DataType.Int32, ip_score));

                            responseActions.Add(setVar);
                        }

                        return responseActions;
                    });
                });
            }
        }
    }
}
