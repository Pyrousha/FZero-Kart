using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerFZeroKart : NetworkManager
{
    #region Dual singleton stuff
    [Header("Parameters")]
    [SerializeField] private bool isThisServerMultiplayer;
    public override void Start()
    {
        if (((MultiServer != null) && (MultiServer != this)) || ((SoloServer != null) && (SoloServer != this)))
        {
            Debug.Log("Destroyed script type" + typeof(NetworkManagerFZeroKart) + " on gameObject" + gameObject.name);
            Destroy(gameObject);
        }

        if (isThisServerMultiplayer)
            MultiServer = this;
        else
            SoloServer = this;

        base.Start();
    }

    //Instance of singleplayer server
    public static NetworkManagerFZeroKart SoloServer { get; private set; } = null;
    //Instance of multiplayer server
    public static NetworkManagerFZeroKart MultiServer { get; private set; } = null;
    #endregion

    /// <summary>Called on server when a client requests to add the player. Adds playerPrefab by default. Can be overwritten.</summary>
    // The default implementation for this function creates a new player object from the playerPrefab.
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        SceneTransitioner.Instance.SetLocalPlayer(player);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
