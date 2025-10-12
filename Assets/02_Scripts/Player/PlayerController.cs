using Rewired;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Player player;
    private IMovement movement;

    void Start()
    {
        player = ReInput.players.GetPlayer(0);
        movement = GetComponent<IMovement>();
    }

    void Update()
    {
        float horizontal = player.GetAxis("Move Horizontal");
        float vertical = player.GetAxis("Move Vertical");
        bool isRunning = player.GetButton("Run");

        movement.Move(new Vector2(horizontal, vertical), isRunning);
    }
}
