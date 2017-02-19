using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class network_connection_manager : MonoBehaviour {


    // Client or Server poll socket
    bool listening = false;

    // Network Info (this is for the local computer)
    bool is_server = false;
    bool is_connected = false;
    string ip_address = "";
    int player_number = -1;
    int players_in_server = 1;


    // Update Network Server Data (This is data for the server)
    int socket_ID = -1;
    int connection_ID = -1;
    int reliable_channel = -1;
    int unreliable_channel = -1;
    int port = -1;


    // Client data
    int CLIENT_server_connection = -1;

    // Host data
    List<network_structs.player_struct> SERVER_client_connections = new List<network_structs.player_struct>();




    int count = 0;


    void Start()
    {


    }




    // Update is called once per frame
    void Update ()
    {
        if (listening) // Server is trying to connect to clients OR Client waiting for response
        {
            CLIENT_SERVER_socket_listen();

            if (count == 2)
            {

                if (is_server)
                {
                    relay_network_info();
                }
                if (is_server == false)
                {
                   
                }



                count = 0;
            }
            count++;
        }

	}


    // Network Input/Output functions

    // Function that controls connection set up
    // Client: Will attempt to connect to server and wait for server confirmation
    // Server: Open socket and listen for client connections
    public void connect_to_server(network_structs.network_client_connect_request connect_request)
    {
                
        ip_address = connect_request.server_ip_address;

        is_server = connect_request.is_server;

        CLIENT_SERVER_set_network_topology();

        if (is_server == true)
        {
            is_connected = true;
        }
        else
        {
            CLIENT_contact_server(ip_address);
        }

        listening = true;

    } 

    // Function that updates outside script with the connection status of the network
    public network_structs.network_info network_connection_update()
    {
        network_structs.network_info network_info = new network_structs.network_info();
        network_info.is_server = is_server;
        network_info.is_connected = is_connected;
        network_info.ip_address = ip_address;
        network_info.player_number = player_number;
        network_info.players_in_server = players_in_server;
    
        return network_info;
    }





    // Client/Server function that initializes the network topology
    void CLIENT_SERVER_set_network_topology()
    {
        
        port = 8888;


        /// Global Config defines global paramters for network library.
        GlobalConfig global_configuration = new GlobalConfig();
        global_configuration.ReactorModel = ReactorModel.SelectReactor;
        global_configuration.ThreadAwakeTimeout = 10;

        /// Add a channel to send and recieve 
        /// Build channel configuration
        ConnectionConfig connection_configuration = new ConnectionConfig();
        connection_configuration.PingTimeout = 50000;
        connection_configuration.DisconnectTimeout = 50000;
        unreliable_channel = connection_configuration.AddChannel(QosType.UnreliableSequenced);
        reliable_channel = connection_configuration.AddChannel(QosType.ReliableSequenced);

        /// Create Network Topology for host configuration
        /// This topology defines: 
        /// (1) how many connection with default config will be supported/
        /// (2) what will be special connections (connections with config different from default).
        HostTopology host_topology;
        if (is_server == true )
        {
            int max_connections = 10;
            host_topology = new HostTopology(connection_configuration, max_connections);
        }
        else
        {
            host_topology = new HostTopology(connection_configuration, 1);
        }

        /// Initializes the NetworkTransport. 
        /// Should be called before any other operations on the NetworkTransport are done.
        NetworkTransport.Init();

        // Open sockets for server and client
        if (is_server == true)
        {
            socket_ID = NetworkTransport.AddHost(host_topology, port);
        }
        else
        {
            socket_ID = NetworkTransport.AddHost(host_topology);
        }


        if (socket_ID < 0)
        {
            Debug.Log("Socket creation failed!");
        }


    }

    // Client Function, Client attempts to contact the server
    void CLIENT_contact_server(string ip_address)
    {

        byte error;
        int connection;

        connection = NetworkTransport.Connect(socket_ID, ip_address, port, 0, out error);
        if (error != 0)
        {
            Debug.Log("Client failed to make request to server");
            Debug.Log("Error number:");
            Debug.Log(error.ToString());
        }
        else
        {
           // Debug.Log("Client Connected to server");
        }

    }


    void CLIENT_SERVER_socket_listen()
    {

        byte error;
        int received_host_ID;
        int received_connection_ID;
        int received_channel_ID;
        int recieved_buffer_size;
        byte[] buffer = new byte[2];
        int buffer_read_size = 2;

        NetworkEventType network_event = NetworkEventType.DataEvent;

        network_event = NetworkTransport.Receive(out received_host_ID,
                                                out received_connection_ID,
                                                out received_channel_ID,
                                                buffer,
                                                buffer_read_size,
                                                out recieved_buffer_size,
                                                out error
                                                );

        switch (network_event)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                // Client connected
                if (is_server == false)
                {
                    //is_connected = true;
                    //CLIENT_server_connection = received_connection_ID;
                }

                if (is_server == true)
                {
                    players_in_server++;
                    network_structs.player_struct player = new network_structs.player_struct();
                    player.player_number = players_in_server;
                    player.connection_ID = received_connection_ID;
                }

                break;

            case NetworkEventType.DisconnectEvent:
                if (is_server == false)
                {
                    Debug.Log("Client: Disconnect Event");
                }
                else
                {
                    Debug.Log("Server: Disconnect Event");
                }

                break;

            case NetworkEventType.DataEvent:
                if (is_server == false)
                {
                    Debug.Log("Client recieved data");
                    player_number = buffer[0];
                    players_in_server = buffer[1];
                    is_connected = true;
                    CLIENT_server_connection = received_connection_ID;
                }
                else
                {
                    // Server does not need to recieve data from the client
                    Debug.Log("Server recieved data");
                }
                break;
        }
    }





    void relay_network_info()
    {
        byte error;
        byte[] message = new byte[2];
        int message_size = 2;
        // Message[0] = player
        // Message[1] = number of players in the server

        foreach (var player in SERVER_client_connections)
        {
            message[0] = (byte) player.player_number;
            message[1] = (byte)players_in_server;
            NetworkTransport.Send(socket_ID, player.connection_ID, unreliable_channel, message, message_size, out error);
            if (error != 0)
            {
                Debug.Log("Could not send");
            }
        }

    }












}
