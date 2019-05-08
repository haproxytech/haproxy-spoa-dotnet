# HAProxy Stream Processing Offload Agent (SPOA) for .NET Core

The HAProxy [Stream Processing Offload Protocol](https://github.com/haproxy/haproxy/blob/master/doc/SPOE.txt) (SPOP) allows traffic data to be streamed to an external agent. This library allows you to write that agent using [.NET Core](https://dotnet.microsoft.com/learn/dotnet/what-is-dotnet).

For more information, read the blog post [Extending HAProxy with the Stream Processing Offload Engine](https://www.haproxy.com/blog/extending-haproxy-with-the-stream-processing-offload-engine/).

## Supported SPOP Capabilities

| Capability    | Supported |
|---------------|-----------|
| fragmentation | yes       |
| async         | no        |
| pipelining    | no        |

## Installation

Add a reference to the NuGet package using the .NET CLI:

```
dotnet add package HAProxy.StreamProcessingOffload.Agent --version 1.0.0 
```

Or use the NuGet Package Manager:

```
Install-Package HAProxy.StreamProcessingOffload.Agent -Version 1.0.0 
```

## Usage

1. Download and install the [.NET Core SDK](https://dotnet.microsoft.com/download).
2. Create a new folder and `cd` into it.
3. Create a new .NET Core project:
```
dotnet new console
```
4. Add a reference to the package:
```
dotnet add package HAProxy.StreamProcessingOffload.Agent --version 1.0.0
```
5. Edit **Program.cs** to start a service listening on an IP and port. Create an instance of `FrameProcessor` and call `HandleStream` to process SPOP messages received on new network connections. This method accepts a `NetworkStream` and a callback function that takes a `NotifyFrame` and returns `List<SpoeAction>`.

## Example 

**Program.cs**:

```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HAProxy.StreamProcessingOffload.Agent;
using HAProxy.StreamProcessingOffload.Agent.Actions;
using HAProxy.StreamProcessingOffload.Agent.Payloads;

namespace spoa_dotnet_example
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
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
```

## How it Works

All communication is handled by the `FrameProcessor` class. Its `HandleStream` method takes a `NetworkStream` object and a lambda function as parameters. The lambda function is called when a NOTIFY frame is received. From there, you can return a list of `SpoeAction` objects, which tell HAProxy to perform action(s).

Actions:

* `SetVariableAction` - set a variable in HAProxy
* `UnsetVariableAction` - unset a variable

When setting a variable, you must pass in the variable scope (`Process`, `Session`, `Transaction`, `Request`, or `Response`). Then, set a name for the variable and its value as a `TypedData` instance.

```C#
SpoeAction setVar = 
   new SetVariableAction(
       VariableScope.Session, // scope
       "ip_score", // name
       new TypedData(DataType.Int32, ip_score)); // value
```

Both actions and arguments received in messages from HAProxy are represented as `TypedData`. This is simply a container for different types of data. `DataType` can be:

* Null
* Boolean
* Int32
* Uint32
* Int64
* Uint64
* Ipv4
* Ipv6
* String
* Binary

For message arguments, the type is set for you by HAProxy depending on the fetch method used. For example, the `src` fetch method is `Ipv4` or `Ipv6`.

There can be multiple *messages* within a NOTIFY frame, so you should check for a message name to find the right one:

```C#
var messages = ((ListOfMessagesPayload)notifyFrame.Payload).Messages;
if (messages.Any(msg => msg.Name == "my-message-name"))
{
   // perform logic
}
```

Here is an example **spoe.conf** file that defines the messages and arguments HAProxy will send to the agent:

```
[my-spoe]
spoe-agent my-agent
   messages           my-message-name
   option var-prefix  myspoe
   log                global
   use-backend        be_agents
   timeout hello      10s
   timeout idle       10s
   timeout processing 10s

spoe-message my-message-name
   args ip=src anotherarg=str(abc)
   event on-frontend-http-request if { path / }
```

In your **haproxy.cfg*, the SPOE can be initialized by adding a `filter spoe` line:

```
frontend web
   bind :80
   filter spoe engine my-spoe config /etc/haproxy/spoe.conf
   http-request set-header "ip_score" %[var(sess.myspoe.ip_score)]
   default_backend servers

backend servers
   balance roundrobin
   server s1 127.0.0.1:8080 check maxconn 30

backend be_agents
   mode tcp
   balance roundrobin
   option spop-check
   server agent1 127.0.0.1:12345 check  inter 10s  maxconn 30
```

## Building and Testing

To build and run the unit tests, use the following commands

```bash
dotnet clean
dotnet build
dotnet test
```