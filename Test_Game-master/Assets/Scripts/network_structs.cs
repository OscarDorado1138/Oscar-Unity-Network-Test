using UnityEngine;
using System.Collections;

public class network_structs {



    // Network_Connection_Manager I/O structs

    // Request struct to make a network connection
    public struct network_client_connect_request
    {
        public bool is_server;
        public string server_ip_address;
    }

    // Response on the current network status
    // Whether the client is connected to the server
    // The number of players connected to the server
    public struct network_info
    {
        public bool is_server;
        public bool is_connected;
        public string ip_address;
        public int player_number;
        public int players_in_server;
    }





    // Internal Struct to pass to the server to Start the server with pre decided connections
    public struct network_server_data
    {
        public bool is_server;
        public string ip_address;
        public int socket_ID;
        public int connection_ID;
        public int reliable_channel;
        public int unreliable_channel;
        public int port;
        public int player_number;
        public int players_in_server;
    }


    // Struct for the host to keep track of players
    public struct player_struct
    {
        public int player_number;
        public int connection_ID;
    }









    public struct capsule_dude
    {
        public float position_x;
        public float position_y;
        public float position_z;
        public float euler_x;
        public float euler_y;
        public float euler_z;
    }

    public struct block_dude
    {
        public float position_x;
        public float position_y;
        public float position_z;
        public float euler_x;
        public float euler_y;
        public float euler_z;
    }




}
