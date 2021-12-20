using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using HAProxy.StreamProcessingOffload.Agent;
using HAProxy.StreamProcessingOffload.Agent.Actions;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

public class TcpConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<TcpConnectionHandler> _logger;

        public TcpConnectionHandler(ILogger<TcpConnectionHandler> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            _logger.LogInformation(connection.ConnectionId + " connected");

            var frameProcessor = new FrameProcessor()
            {
                EnableLogging = true,
                LogFunc = (msg) => _logger.LogInformation(msg)
            };

            var stream = connection.Transport.Input.AsStream();

            // Cancel stream when process terminates
            AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
            {
                await frameProcessor.CancelStreamAsync(stream);
            };

            // note: With ASP.NET, we use the HandleStreamAsync overload that takes an Input and Output stream
            await frameProcessor.HandleStreamAsync(connection.Transport.Input.AsStream(), connection.Transport.Output.AsStream(), async (notifyFrame) =>
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

                    // simulate a non-blocking API call that gets the IP score
                    // and takes 1 second
                    await Task.Delay(1000);

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
        }
    }

