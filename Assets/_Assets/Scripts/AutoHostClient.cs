using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoHostClient : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;


    // Start is called before the first frame update
    void Start()
    {
        networkManager.StartHost();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
