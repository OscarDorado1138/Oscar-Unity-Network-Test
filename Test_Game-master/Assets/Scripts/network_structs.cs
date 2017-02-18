using UnityEngine;
using System.Collections;

public class network_structs {


    public struct network_client_connect_request
    {
        public bool is_server;
        public string server_ip_address;
    }


    public struct network_client_connected_response
    {
        public bool is_connected;
        public bool player_number;
    }

    public struct network_server_data
    {
        public bool is_server;
        public string ip_address;
        public int socket;
        public int connection;
        public int reliable_channel;
        public int unreliable_channel;
        public int port;
        public int player_number;
        public int players_in_server;
    }

    public struct network_info
    {
        public bool is_server;
        public bool is_connected;
        public int player_number;
        public int players_in_server;
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
