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

    Socket socket;

    NetworkPlayer selfPlayer;

    GameObject[] networkSpawns;

    public delegate void PlayerSpawnDelegate();
    public static event PlayerSpawnDelegate onPlayerSpawned;

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
        //Calculate Occupied and Unoccupied Network Spawns
        GameObject playerGO = Instantiate(playerPrefab.gameObject, networkSpawns[UnityEngine.Random.Range(0, networkSpawns.Length)].transform.position, Quaternion.identity);
        playerGO.name = playerName;
        selfPlayer = playerGO.GetComponent<NetworkPlayer>();
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

    public void SendVector (Vector3 vector)
    {
        NetworkStream stream = new NetworkStream(socket);

        byte[] vectorBytes = GetSelfPlayerName().Concat(DataPacketConvertor.GetBytes(vector)).ToArray();
        stream.Write(vectorBytes, 0, vectorBytes.Length);
    }

    void RecieveData()
    {
        NetworkStream stream = new NetworkStream(socket);

        while (true)
        {
            Thread.Sleep(250);

            byte[] nameLengthBytes = new byte[4];
            int numNameLengthBytes = stream.Read(nameLengthBytes, 0, nameLengthBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nameLengthBytes);
            }
            int nameLength = BitConverter.ToInt32(nameLengthBytes, 0);

            byte[] nameBytes = new byte[nameLength];
            int numNameBytes = stream.Read(nameBytes, 0, nameBytes.Length);

            string incommingPlayerName = Encoding.ASCII.GetString(nameBytes, 0, numNameBytes);

            byte[] typeBytes = new byte[3];
            int numTypeBytes = stream.Read(typeBytes, 0, typeBytes.Length);

            float[] vectorfloats = new float[3];

            if(Encoding.ASCII.GetString(typeBytes, 0, numTypeBytes) == "VEC")
            {
                for (int i = 0; i < 3; i++)
                {
                    byte[] dataBytes = new byte[4];
                    int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(dataBytes);
                    }
                    vectorfloats[i] = BitConverter.ToSingle(dataBytes, 0);
                }
                Vector3 recievedVector = new Vector3(vectorfloats[0], vectorfloats[1], vectorfloats[2]);
                print(incommingPlayerName + recievedVector.ToString());
            }
        }
    }

    void Update()
    {
        
    }
}
