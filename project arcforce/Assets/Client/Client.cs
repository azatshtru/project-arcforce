using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;

    public string SERVER_IP_ADDRESS;
    public int PORT;
    public NetworkPlayer playerPrefab;

    Socket socket;

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

        SpawnPlayer();
        onPlayerSpawned();
    }

    void SpawnPlayer()
    {
        //Calculate Occupied and Unoccupied Network Spawns
        Instantiate(playerPrefab.gameObject, networkSpawns[Random.Range(0, networkSpawns.Length)].transform.position, Quaternion.identity);
    }

    public void SetIPAddress(string _ipAddress)
    {
        SERVER_IP_ADDRESS = _ipAddress;
    }

    void RecieveData()
    {
        while (true)
        {
            Thread.Sleep(250);

            byte[] recievedBytes = new byte[1024];
            int numBytes = socket.Receive(recievedBytes);
            string recievedString = Encoding.ASCII.GetString(recievedBytes, 0, numBytes);
            print(recievedString);
        }
    }

    void Update()
    {
        
    }
}
