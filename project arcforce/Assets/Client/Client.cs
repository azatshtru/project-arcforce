using System.Collections;
using System.Collections.Generic;
using System;
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
    public string playerName;
    public NetworkPlayer playerPrefab;
    public OtherPlayer otherPlayerPrefab;

    Socket socket;

    NetworkPlayer selfPlayer;

    GameObject[] networkSpawns;

    List<string> playerNamesList = new List<string>();
    List<OtherPlayer> otherPlayers = new List<OtherPlayer>();

    public delegate void PlayerSpawnDelegate();
    public static event PlayerSpawnDelegate onPlayerSpawned;

    string latestSender;
    KeyValuePair<string, Vector3> latestVector = new KeyValuePair<string, Vector3>();
    bool isNAM;
    bool isVEC;

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

    public void ConnectToServer()
    {
        IPAddress ipAddress = IPAddress.Parse(SERVER_IP_ADDRESS);
        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, PORT);

        socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(remoteEndPoint);

        Debug.Log($"Connected to {socket.RemoteEndPoint.ToString()}");

        ThreadStart job = new ThreadStart(RecieveData);
        Thread t1 = new Thread(job);
        t1.Start();

        SpawnSelfPlayer();
        onPlayerSpawned();
    }

    void SpawnSelfPlayer()
    {
        //TODO: Calculate Occupied and Unoccupied Network Spawns

        GameObject playerGO = Instantiate(playerPrefab.gameObject, networkSpawns[UnityEngine.Random.Range(0, networkSpawns.Length)].transform.position, Quaternion.identity);
        playerGO.name = playerName;
        selfPlayer = playerGO.GetComponent<NetworkPlayer>();

        playerNamesList.Add(playerName);

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

        byte[] nameBytes = GetSelfPlayerName().Concat(Encoding.ASCII.GetBytes("NAM")).ToArray();
        stream.Write(nameBytes, 0, nameBytes.Length);

        stream.Close();
    }

    public void SendVector (Vector3 vector)
    {
        NetworkStream stream = new NetworkStream(socket);

        byte[] vectorBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(vector)).ToArray();
        stream.Write(vectorBytes, 0, vectorBytes.Length);

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

    public string GetSenderName(NetworkStream stream)
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

            byte[] typeBytes = new byte[3];
            int numTypeBytes = stream.Read(typeBytes, 0, typeBytes.Length);
            string type = DataPacketConvertor.GetString(typeBytes, numTypeBytes);

            if (type == "NAM")
            {
                isNAM = true;
            }

            if (type == "VEC")
            {
                byte[] dataBytes = new byte[12];
                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);

                latestVector = new KeyValuePair<string, Vector3>(senderName, DataPacketConvertor.GetVector(dataBytes));
                isVEC = true;
            }
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
    }
}
