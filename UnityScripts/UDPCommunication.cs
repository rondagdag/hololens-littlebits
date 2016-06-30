using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using HoloToolkit.Unity;
using System.Collections.Generic;
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;

#endif
public class UDPCommunication : Singleton<UDPCommunication>
{
    public string port = "12345";
    public string externalIP = "192.168.17.110";
    public string externalPort = "12346";

    const string FAN_OFF_MESSAGE = "fan-off";
    const string FAN_ON_MESSAGE = "fan-on";
    const string LIGHT_ON_MESSAGE = "light-on";
    const string LIGHT_OFF_MESSAGE = "light-off";
    const string TEMPERATURE_MESSAGE = "temperature";

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();


#if !UNITY_EDITOR
    DatagramSocket socket;
#endif
    // use this for initialization
#if !UNITY_EDITOR
    async void Start()
    {
        Debug.Log("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, port);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        var message = "hello from " + IP;
        await SendMessage(message);
        await SendMessage("hello");

        Debug.Log("exit start");
    }

    private async System.Threading.Tasks.Task SendMessage(string message)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                var data = Encoding.UTF8.GetBytes(message);

                writer.WriteBytes(data);
                await writer.StoreAsync();
                Debug.Log("Sent: " + message);
            }
        }
    }

    public async void TurnOnFan()
    {
        await SendMessage(FAN_ON_MESSAGE);
    }

    public async void TurnOffFan()
    {
        await SendMessage(FAN_OFF_MESSAGE);
    }

    public async void TurnOnLight()
    {
        await SendMessage(LIGHT_ON_MESSAGE);
    }

    public async void TurnOffLight()
    {
        await SendMessage(LIGHT_OFF_MESSAGE);
    }

#else
    void Start()
    {

    }
#endif

    // Update is called once per frame
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

#if !UNITY_EDITOR
    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Debug.Log("GOT MESSAGE: ");
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("MESSAGE: " + message);

        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                Thermostat.Temperature = float.Parse(message);
            });
        }
    }
#endif
}
