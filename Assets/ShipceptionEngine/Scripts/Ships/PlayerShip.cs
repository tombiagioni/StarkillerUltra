using UnityEngine;
using System.Collections;

namespace Shipception
{
    public class PlayerShip : MonoBehaviour
    {

        public ShipComponent Wings;
        public ShipComponent Cannon;
        public ShipComponent Engine;
        public ShipComponent Cockpit;

        // Sprite references :
        public Sprite playerStand;
        public Sprite playerUp;
        public Sprite playerDown;


        /// <summary>
        /// 1 - The speed of the ship
        /// </summary>
        public Vector2 speed = new Vector2(50, 50);

        // 2 - Store the movement
        private Vector2 movement;

        private void Update()
        {
            // 3 - Retrieve axis information
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            // 4 - Movement per direction
            movement = new Vector2(
                speed.x*inputX,
                speed.y*inputY);

        }

        private void FixedUpdate()
        {
            // 5 - Move the game object
            rigidbody2D.velocity = movement;
        }

    }
}
