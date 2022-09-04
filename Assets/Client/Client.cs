using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Linq;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;

    public string SERVER_IP_ADDRESS;
    public int PORT;
    public int UDP_PORT;
    public string playerName;
    public NetworkPlayer playerPrefab;
    public OtherPlayer otherPlayerPrefab;

    Socket socket;
    int SERVER_INT;
    UdpClient udpClient;
    IPEndPoint udpEndPoint;

    NetworkPlayer selfPlayer;

    GameObject[] networkSpawns;

    List<string> playerNamesList = new List<string>();
    List<OtherPlayer> otherPlayers = new List<OtherPlayer>();
    Dictionary<int, string> playerNamesDictionary = new Dictionary<int, string>();

    public delegate void PlayerSpawnDelegate();
    public static event PlayerSpawnDelegate onPlayerSpawned;

    string latestSender;
    KeyValuePair<string, Vector3> latestVector = new KeyValuePair<string, Vector3>();
    KeyValuePair<string, Ray> latestRay = new KeyValuePair<string, Ray>();
    KeyValuePair<string, float> latestDecimal = new KeyValuePair<string, float>();
    bool isNAM;
    bool isVEC;
    bool isRAY;
    bool isDEC;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    private void Start()
    {
        networkSpawns = GameObject.FindGameObjectsWithTag("NetworkSpawn");
    }

    int GetServerInt()
    {
        byte[] serverIntBytes = new byte[7];
        int serverIntByteLength = socket.Receive(serverIntBytes);

        Stream stream = new MemoryStream(serverIntBytes);
        byte[] nameBytes = new byte[6];
        int numNameBytes = stream.Read(nameBytes, 0, nameBytes.Length);
        string s_name = DataPacketConvertor.GetString(nameBytes, numNameBytes);

        if(s_name == "server")
        {
            byte[] intBytes = new byte[1];
            int numIntBytes = stream.Read(intBytes, 0, intBytes.Length);
            stream.Close();
            return int.Parse(DataPacketConvertor.GetString(intBytes, numIntBytes));
        }

        stream.Close();
        return 0;
    }

    public void ConnectToServer()
    {
        IPAddress ipAddress = IPAddress.Parse(SERVER_IP_ADDRESS);
        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, PORT);

        socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(remoteEndPoint);

        Debug.Log($"Connected to {socket.RemoteEndPoint.ToString()}");

        udpClient = new UdpClient();
        udpEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP_ADDRESS), UDP_PORT);
        udpClient.Connect(udpEndPoint);

        SERVER_INT = GetServerInt();

        ThreadStart job = new ThreadStart(RecieveData);
        Thread t1 = new Thread(job);
        t1.Start();

        ThreadStart udpJob = new ThreadStart(RecieveDataUDP);
        Thread t2 = new Thread(udpJob);
        t2.Start();

        SpawnSelfPlayer();
        onPlayerSpawned();
    }

    public void HandleDisconnection ()
    {

    }

    void SpawnSelfPlayer()
    {
        GameObject playerGO = Instantiate(playerPrefab.gameObject, networkSpawns[SERVER_INT].transform.position, Quaternion.identity);
        playerGO.name = playerName;
        selfPlayer = playerGO.GetComponent<NetworkPlayer>();

        playerNamesList.Add(playerName);
        playerNamesDictionary.Add(SERVER_INT, playerName);

        SendSelfPlayer();
    }

    public void SetName(string name)
    {
        playerName = name;
    }

    private byte[] GetSelfPlayerName()
    {
        byte[] playerNameLength = BitConverter.GetBytes(playerName.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(playerNameLength);
        }
        return playerNameLength.Concat(Encoding.ASCII.GetBytes(playerName)).ToArray();
    }

    public void SendSelfPlayer()
    {
        NetworkStream stream = new NetworkStream(socket);

        byte[] b_serverint = BitConverter.GetBytes(SERVER_INT);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b_serverint);
        }

        byte[] nameBytes = GetSelfPlayerName().Concat(Encoding.ASCII.GetBytes("NAM")).Concat(b_serverint).ToArray();
        stream.Write(nameBytes, 0, nameBytes.Length);

        stream.Close();
    }

    public void SendVector (Vector3 vector)
    {
        /*
        NetworkStream stream = new NetworkStream(socket);

        byte[] vectorBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(vector)).ToArray();
        stream.Write(vectorBytes, 0, vectorBytes.Length);

        stream.Close();
        */

        byte[] vectorBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(vector)).ToArray();
        udpClient.Send(vectorBytes, vectorBytes.Length);
    }

    public void SendRay (Ray ray)
    {
        NetworkStream stream = new NetworkStream(socket);

        byte[] rayBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(ray)).ToArray();
        stream.Write(rayBytes, 0, rayBytes.Length);

        stream.Close();
    }

    public void SendDecimal (float number)
    {
        NetworkStream stream = new NetworkStream(socket);

        byte[] decimalBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(number)).ToArray();
        stream.Write(decimalBytes, 0, decimalBytes.Length);

        stream.Close();
    }

    void InstantiateIncommingPlayer(string _name)
    {
        if (playerNamesList.Contains(_name))
        {
            return;
        }

        GameObject otherPlayer = Instantiate(otherPlayerPrefab.gameObject, otherPlayerPrefab.transform.position, Quaternion.identity);
        otherPlayer.name = _name;
        otherPlayer.GetComponent<OtherPlayer>().SetOtherName(_name);

        otherPlayers.Add(otherPlayer.GetComponent<OtherPlayer>());
        playerNamesList.Add(_name);

        SendSelfPlayer();
    }

    void SetOtherPlayerPosition(KeyValuePair<string, Vector3> vec)
    {
        foreach(OtherPlayer p in otherPlayers)
        {
            if(p.GetOtherName() == vec.Key)
            {
                p.SetOtherPosition(vec.Value);
            }
        }
    }

    void SetOtherPlayerRay(KeyValuePair<string, Ray> ray)
    {
        foreach (OtherPlayer p in otherPlayers)
        {
            if (p.GetOtherName() == ray.Key)
            {
                p.SetOtherRay(ray.Value);
            }
        }
    }

    void SetOtherPlayerDecimal(KeyValuePair<string, float> dec)
    {
        foreach (OtherPlayer p in otherPlayers)
        {
            if (p.GetOtherName() == dec.Key)
            {
                p.SetOtherHealth(dec.Value);
            }
        }
    }

    public string GetSenderName(NetworkStream stream)
    {
        byte[] nameLengthBytes = new byte[4];
        int numNameLengthBytes = stream.Read(nameLengthBytes, 0, nameLengthBytes.Length);

        byte[] nameBytes = new byte[DataPacketConvertor.GetInt(nameLengthBytes)];
        int numNameBytes = stream.Read(nameBytes, 0, nameBytes.Length);
        return DataPacketConvertor.GetString(nameBytes, numNameBytes);
    }

    public string GetSenderName(Stream stream)
    {
        byte[] nameLengthBytes = new byte[4];
        int numNameLengthBytes = stream.Read(nameLengthBytes, 0, nameLengthBytes.Length);

        byte[] nameBytes = new byte[DataPacketConvertor.GetInt(nameLengthBytes)];
        int numNameBytes = stream.Read(nameBytes, 0, nameBytes.Length);
        return DataPacketConvertor.GetString(nameBytes, numNameBytes);
    }

    void RecieveData()
    {
        NetworkStream stream = new NetworkStream(socket);

        while (true)
        {
            Thread.Sleep(30);

            string senderName = GetSenderName(stream);

            latestSender = senderName;

            if(latestSender == playerName)
            {
                //continue;
            }

            byte[] typeBytes = new byte[3];
            int numTypeBytes = stream.Read(typeBytes, 0, typeBytes.Length);
            string type = DataPacketConvertor.GetString(typeBytes, numTypeBytes);

            if (type == "NAM")
            {
                byte[] dataBytes = new byte[4];
                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);
                int otherServerInt = DataPacketConvertor.GetInt(dataBytes);

                if (!playerNamesDictionary.Values.Contains(senderName))
                {
                    playerNamesDictionary.Add(otherServerInt, senderName);
                }

                isNAM = true;
            }

            if (type == "RAY")
            {
                byte[] dataBytes = new byte[24];
                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);

                latestRay = new KeyValuePair<string, Ray>(senderName, DataPacketConvertor.GetRay(dataBytes));
                isRAY = true;
            }

            if (type == "DEC")
            {
                byte[] dataBytes = new byte[4];
                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);

                latestDecimal = new KeyValuePair<string, float>(senderName, DataPacketConvertor.GetFloat(dataBytes));
                isDEC = true;
            }
        }
    }

    void RecieveDataUDP()
    {
        
        while (true)
        {
            //Thread.Sleep(30);

            byte[] recievedBytes = udpClient.Receive(ref udpEndPoint);
            Stream stream = new MemoryStream(recievedBytes);

            string senderName = GetSenderName(stream);
            latestSender = senderName;
            if (latestSender == playerName)
            {
                //continue;
            }

            byte[] typeBytes = new byte[3];
            int numTypeBytes = stream.Read(typeBytes, 0, typeBytes.Length);
            string type = DataPacketConvertor.GetString(typeBytes, numTypeBytes);

            if (type == "VEC")
            {
                byte[] dataBytes = new byte[12];
                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);

                latestVector = new KeyValuePair<string, Vector3>(senderName, DataPacketConvertor.GetVector(dataBytes));
                isVEC = true;
            }

            stream.Flush();
            stream.Dispose();
            stream.Close();
        }
        
    }

    private void Update()
    {
        if (isNAM)
        {
            InstantiateIncommingPlayer(latestSender);
            isNAM = false;
        }

        if (isVEC)
        {
            SetOtherPlayerPosition(latestVector);
            isVEC = false;
        }

        if (isRAY)
        {
            SetOtherPlayerRay(latestRay);
            isRAY = false;
        }

        if (isDEC)
        {
            SetOtherPlayerDecimal(latestDecimal);
            isDEC = false;
        }
    }
}
