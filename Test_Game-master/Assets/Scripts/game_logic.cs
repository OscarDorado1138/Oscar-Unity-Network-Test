using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class game_logic : MonoBehaviour {

    bool network_game = true;

    string server_ip = "127.00.01";

    int buffer_array_size = 0;

    public GameObject network_lobby_UI;

    public List<network_structs> network_objects = new List<network_structs>();



   

	// Use this for initialization
	void Start ()
    {


        // ADD TO THIS IF YOU WANT TO NETWORK AN OBJECT
        // *WARNING* the number of network objects must match the available amount in the scene
        add_network_component("capsule_dude", "server authoritive");
        add_network_component("capsule_dude", "server authoritive");
        add_network_component("capsule_dude", "server authoritive");
        add_network_component("capsule_dude", "server authoritive");


        // This will create the network join lobby UI, but only if the game is a networked game
        if(network_game)
        {
            Instantiate(network_lobby_UI, transform.position, Quaternion.identity);
        }


    }

    void add_network_component(string network_object, string network_authority)
    {
       // network_structs oscar = new network_structs;
       switch (network_object)
        {
            case "capsule_dude":
                //network_structs.Book n_object = new network_structs.Book();
                //network_objects.Add(n_object);
                break;
        }
       
    }

    void buffer_allocation(string network_object)
    {
        // 1 float = 4 bytes

        int bytes_to_send = 0;
        switch (network_object)
        {
            case "tank":
                // We must send x,y,z position float of tank
                // We must send x,y,z rotate float of tank
                bytes_to_send += 24;
                break;
            case "driver_lever":
                // We must send Euler X float of lever
                bytes_to_send += 4;
                break;
            case "capsule_dude":
                // We must send x,y,z position float of capsule dude
                // We must send x,y,z rotate float of capsule dude
                bytes_to_send += 24;
                break;

        }

        buffer_array_size += bytes_to_send;
    }



}
