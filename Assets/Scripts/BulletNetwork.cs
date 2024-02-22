using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletNetwork : NetworkBehaviour
{
    [SerializeField] public PlayerNetwork.TeamColor bulletColor;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
   
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger con " + collision.gameObject.name);
        if (collision.tag == "map")
            DestroyThisBulletServerRpc();
    
        else if(bulletColor != collision.GetComponentInParent<PlayerNetwork>().playerTeam.Value)
        {
            collision.GetComponentInParent<PlayerNetwork>().ReceiveDamageServerRpc();
            DestroyThisBulletServerRpc();
        }
    }
 

    [ServerRpc(RequireOwnership = false)]
    private void DestroyThisBulletServerRpc()
    {
        Destroy(gameObject);
    }
}
