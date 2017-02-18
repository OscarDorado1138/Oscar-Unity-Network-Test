using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class network_connection_manager : MonoBehaviour {

    public string ip_address;
    bool is_server = false;
    //bool connected_to_server = false;

    bool listening = false;
    network_structs.network_server_data network_server_data;
    network_structs.network_info network_info;

    void Start()
    {


        // Update Network Server Data (This is data for the server)
        network_server_data.is_server = false;
        network_server_data.ip_address = "";
        network_server_data.socket = -1;
        network_server_data.connection = -1;
        network_server_data.reliable_channel = -1;
        network_server_data.unreliable_channel = -1;
        network_server_data.port = -1;
        network_server_data.player_number = -1;
        network_server_data.players_in_server = -1;


        // Update Network Info (this is for the local computer)
        network_info.is_server = false;
        network_info.is_connected = false;
        network_info.player_number = -1;
        network_info.players_in_server = -1;

    }




    // Update is called once per frame
    void Update ()
    {
        if (listening) // Server is trying to connect to clients OR Client waiting for response
        {

            socket_listen();
        
        }

	}



    public void connect_to_server(network_structs.network_client_connect_request connect_request)
    {

        //network_structs.network_client_connected_response connect_response = new network_structs.network_client_connected_response();

        
                
        ip_address = connect_request.server_ip_address;

        is_server = connect_request.is_server;

        CLIENT_SERVER_set_network_topology();

        if (is_server == true)
        {
            network_info.is_connected = true;
        }
        else
        {
            CLIENT_contact_server(connect_request.server_ip_address);
        }

        listening = true;

    } 


    void CLIENT_SERVER_set_network_topology()
    {
        int socket_ID;
        int reliable_channel;
        int unreliable_channel;
        int socket_port_number = 8888;


        /// Global Config defines global paramters for network library.
        GlobalConfig global_configuration = new GlobalConfig();
        global_configuration.ReactorModel = ReactorModel.SelectReactor;
        global_configuration.ThreadAwakeTimeout = 10;

        /// Add a channel to send and recieve 
        /// Build channel configuration
        ConnectionConfig connection_configuration = new ConnectionConfig();
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
            socket_ID = NetworkTransport.AddHost(host_topology, socket_port_number);
        }
        else
        {
            socket_ID = NetworkTransport.AddHost(host_topology);
        }


        if (socket_ID < 0)
        {
            Debug.Log("Client socket creation failed!");
        }
        else
        {
            // Update Struct
            network_server_data.ip_address = ip_address;
            network_server_data.is_server = is_server;
            network_server_data.socket = socket_ID;
            network_server_data.port = socket_port_number;
            network_server_data.reliable_channel = reliable_channel;
            network_server_data.unreliable_channel = unreliable_channel;

            Debug.Log(network_server_data.socket.ToString());
            Debug.Log(socket_ID.ToString());
        }

    }


    void CLIENT_contact_server(string ip_address)
    {

        byte error;
        int connection;

        connection = NetworkTransport.Connect(network_server_data.socket, ip_address, network_server_data.port, 0, out error);
        if (error != 0)
        {
            Debug.Log("I FAILED to connect to the server");
            Debug.Log(error.ToString());
        }
        else
        {
            Debug.Log("Client Connected to server");
            network_server_data.connection = connection;
        }

    }


    void socket_listen()
    {

        byte error;
        int received_host_ID;
        int received_connection_ID;
        int received_channel_ID;
        int recieved_buffer_size;
        byte[] buffer = new byte[100];
        int buffer_read_size = 100;

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
            case NetworkEventType.DataEvent:

                // Client looking for Server response
                if (network_info.is_server == false)
                {
                    network_info.is_connected = true;
                    network_info.player_number = buffer[0];
                    network_info.players_in_server = buffer[1];
                    Debug.Log("THIS IS THE CLIENT!!!! Connected)");
                }
                break;
            case NetworkEventType.ConnectEvent:
                if(is_server == true)
                {
                    Debug.Log("Server: Found Client");
                }
                else
                {
                    Debug.Log("Client: Found Server");

                }
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnect Event");
                if (is_server == true)
                {
                    Debug.Log("Server: Disconnect Event");
                    byte error2;
                    byte[] message = new byte[100];
                    message[0] = 1;
                    message[1] = 2;
                    message[2] = 3;
                    Debug.Log(network_server_data.socket.ToString());
                    Debug.Log(network_server_data.is_server.ToString());
                    Debug.Log(network_server_data.ip_address.ToString());

                    NetworkTransport.Send(network_server_data.socket, received_connection_ID, network_server_data.reliable_channel, message, 100, out error2);

                    if (error != 0)
                    {
                        Debug.Log("Could not send");
                    }
                    else
                    {
                        Debug.Log("SENT");

                    }



                }
                else
                {
                    Debug.Log("Client: Disconnect Event");
                }
                break;
        }
    }

    public bool check_connection()
    {
        return network_info.is_connected;
    }










}
