using Mirror;
using UnityEngine;

public class SimpleEnemyMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;
    private Transform player;

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindPlayer();
    }

    [Server]
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    [Server]
    private void Update()
    {
        if (player == null) 
        {
            FindPlayer();
            return;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }
}