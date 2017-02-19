using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class network_connection_manager : MonoBehaviour {



    bool listening = false;
    network_structs.network_server_data network_server_data;
    network_structs.network_info network_info;


    // Update Network Server Data (This is data for the server)
    public string ip_address = "";
    int socket = -1;
    int connection = -1;
    int reliable_channel = -1;
    int unreliable_channel = -1;
    int port = -1;
    int player_number = -1;
    int players_in_server = -1;

    // Update Network Info (this is for the local computer)
    bool is_server = false;
    bool is_connected = false;

    int recieved_con;

    int count = 0;


    void Start()
    {


    }




    // Update is called once per frame
    void Update ()
    {
        if (listening) // Server is trying to connect to clients OR Client waiting for response
        {
            socket_listen();

            if (count == 2)
            {

                if (is_server)
                {
                    relay_network_info();
                }
                if (is_server == false)
                {
                    client_relay();
                }

                count = 0;
            }
            count++;
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
            is_connected = true;
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
        //int reliable_channel;
        //int unreliable_channel;
        int socket_port_number = 8888;


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
            //ip_address = ip_address;
            //is_server = is_server;
            socket = socket_ID;
            port = socket_port_number;
        
            Debug.Log(network_server_data.socket.ToString());
            Debug.Log(socket_ID.ToString());
        }

    }


    void CLIENT_contact_server(string ip_address)
    {

        byte error;
        int connection;

        connection = NetworkTransport.Connect(socket, ip_address, port, 0, out error);
        if (error != 0)
        {
            Debug.Log("I FAILED to connect to the server");
            Debug.Log(error.ToString());
        }
        else
        {
            Debug.Log("Client Connected to server");
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
                if (is_server == false)
                {
                    Debug.Log("Client recieved data");
                }
                else
                {
                    Debug.Log("Server recieved data");
                }
                break;
            case NetworkEventType.ConnectEvent:
                if(is_server == true)
                {
                    recieved_con = received_connection_ID;

                    Debug.Log("Server: Found Client");
                    byte error2;
                    byte[] message = new byte[100];
                    message[0] = 1;
                    message[1] = 2;
                    message[2] = 3;
                    Debug.Log(socket.ToString());
                    Debug.Log(is_server.ToString());
                    Debug.Log(ip_address.ToString());

                    NetworkTransport.Send(socket, received_connection_ID, unreliable_channel, message, 100, out error2);

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
                    recieved_con = received_connection_ID;
                    Debug.Log("Client: Found Server");

                }
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnect Event");
                Debug.Log("Why DISCONNECT");
                Debug.Log(error.ToString());
                Debug.Log("^ DISCONNECT");

                if (is_server == true)
                {
                    Debug.Log("Server: Disconnect Event");
                    byte error2;
                    byte[] message = new byte[100];
                    message[0] = 1;
                    message[1] = 2;
                    message[2] = 3;
                    Debug.Log(socket.ToString());
                    Debug.Log(is_server.ToString());
                    Debug.Log(ip_address.ToString());

                    NetworkTransport.Send(socket, received_connection_ID, unreliable_channel, message, 100, out error2);
                    /*
                    if (error != 0)
                    {
                        Debug.Log("Could not send");
                    }
                    else
                    {
                        Debug.Log("SENT");

                    }
                    */


                }
                else
                {
                    Debug.Log("Client: Disconnect Event");
                }
                break;
        }
    }

    void relay_network_info()
    {
        Debug.Log("Server Relay");
        byte error;
        byte[] message = new byte[100];
        message[0] = 1;
        message[1] = 2;
        message[2] = 3;
        Debug.Log(socket.ToString());
        Debug.Log(is_server.ToString());
        Debug.Log(ip_address.ToString());

        NetworkTransport.Send(socket, recieved_con, unreliable_channel, message, 100, out error);

        if (error != 0)
        {
            Debug.Log("Could not send");
        }
        else
        {
            Debug.Log("SENT");

        }
    }

    void client_relay()
    {
        Debug.Log("Client Relay");
        byte error;
        byte[] message = new byte[100];
        message[0] = 1;
        message[1] = 2;
        message[2] = 3;

        NetworkTransport.Send(socket, recieved_con, unreliable_channel, message, 100, out error);

        if (error != 0)
        {
            Debug.Log("Could not send");
            Debug.Log(error.ToString());
        }
        else
        {
            Debug.Log("SENT");

        }
    }

    public bool check_connection()
    {
        return is_connected;
    }










}
