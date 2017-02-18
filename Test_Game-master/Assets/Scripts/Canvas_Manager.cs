// The Canvas Manager has one job. At the very end of its logic it MUST have a filled out
// Network_Player.Network_Info struct for all clients AND the server
// The UI for the network lobby can look anywway it does but it must have the information filled



using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Canvas_Manager : MonoBehaviour {
    // UI's
    public GameObject UI_create_join;
    public GameObject UI_server_join;
    public GameObject UI_client_join;
    public GameObject UI_client_waiting;
    public GameObject network_connection_manager_prefab;
    public GameObject network_connection_manager;
    public GameObject server;






    public spawner_manager spawner;
    static bool is_a_host = false;
    static string ip_address;
    static string inserted_ip = "";
    public GameObject client_lobby;

    bool client_connected = false;
    bool request = false;

    // Network Contracts
    network_structs.network_client_connect_request network_client_connect_request;
    network_structs.network_info network_info;

    void Start()
    {
        Instantiate(UI_create_join, transform.position, Quaternion.identity);
    }

    void Update()
    {
        if (request == true)
        {

            client_connected = network_connection_manager.GetComponent<network_connection_manager>().check_connection();

            if (client_connected == true)
            {
                client_connected_UI();
            }
        }

    }

    public void change_to_host(GameObject server_client)
    {
        is_a_host = true;
        ip_address = Network.player.ipAddress;
        Debug.Log(ip_address);

        // NETWORK STRUCT UPDATE
        network_client_connect_request.is_server = true;
        network_client_connect_request.server_ip_address = ip_address;

        Destroy(server_client);

        GameObject s_lobby = Instantiate(UI_server_join, transform.position, Quaternion.identity);
        //GameObject s_lobby = GameObject.Find("Server Lobby(Clone)");
        GameObject s_lobby_button = s_lobby.transform.Find("Button").gameObject;
        GameObject s_lobby_button_text = s_lobby_button.transform.Find("Text").gameObject;
        Text ip_to_display = s_lobby_button_text.GetComponent<Text>();
        ip_to_display.text = "IP Address: " + ip_address;

        //Instantiate(custom_network_manager, transform.position, Quaternion.identity);



        network_connection_manager = Instantiate(network_connection_manager_prefab, transform.position, Quaternion.identity);

        network_connection_manager.GetComponent<network_connection_manager>().connect_to_server(network_client_connect_request);

    }

    public void change_to_join(GameObject server_client)
    {
        // NETWORK STRUCT UPDATE
        network_client_connect_request.is_server = false;

        Destroy(server_client);
        Instantiate(UI_client_join, transform.position, Quaternion.identity);
    }

    public void insert_ip(GameObject button)
    {
        client_lobby = button;
        GameObject panel = client_lobby.transform.Find("Panel").gameObject;
        GameObject input_field = panel.transform.Find("InputField").gameObject;
        GameObject text = input_field.transform.Find("Text").gameObject;
        Text give_ip = text.GetComponent<Text>();
        inserted_ip = give_ip.text;

        // NETWORK STRUCT UPDATE
        network_client_connect_request.server_ip_address = inserted_ip;


        if (request == true)
        {
            Destroy(network_connection_manager);
        }

        
        network_connection_manager = Instantiate(network_connection_manager_prefab, transform.position, Quaternion.identity);

        network_connection_manager.GetComponent<network_connection_manager>().connect_to_server(network_client_connect_request);
        request = true;

    }


    public void client_connected_UI()
    {
        
        Destroy(client_lobby);

        waiting_in_lobby(network_info.player_number);
    }

    public void waiting_in_lobby(int players)
    {
        
        GameObject wait = Instantiate(UI_client_waiting, transform.position, Quaternion.identity);
        GameObject panel = wait.transform.Find("Panel").gameObject;
        GameObject wait_panal = panel.transform.Find("Wait").gameObject;
        GameObject wait_text = wait_panal.transform.Find("Text").gameObject;
        Text give_ip = wait_text.GetComponent<Text>();
        give_ip.text = "Player " + players.ToString() + " in Lobby...";


        //GameObject n_manager = GameObject.Find("Custom Network Manager(Clone)");
        //network_manager n_manager_script = n_manager.GetComponent<network_manager>();
        //n_manager_script.started = true;

    }

    public void start_the_game()
    {
        // The game has started!!!
        Debug.Log("THE GAME HAS STARTED!");
        GameObject s_lobby = GameObject.Find("Server Lobby(Clone)");
        Destroy(s_lobby);

        GameObject n_manager = GameObject.Find("Custom Network Manager(Clone)");
        network_manager n_manager_script = n_manager.GetComponent<network_manager>();
        //n_manager_script.game_ready = true;
        
        if (!n_manager_script.is_the_host())
        {
            GameObject wait = GameObject.Find("Client Waiting(Clone)");
            Destroy(wait);
        }

        spawner.spawn_four_players(1,2,3,4);

        //n_manager_script.game_ready = true;
    }

    

















    public string get_address()
    {
        return ip_address;
    }


    public bool get_host_status()
    {
        return is_a_host;
    }

    public string get_inserted_ip()
    {
        return inserted_ip;
    }


}
