﻿using UnityEngine;
using System.Collections;

namespace Shipception
{
    public class SimpleMove : MonoBehaviour
    {

        // 1 - Designer variables

        /// <summary>
        /// Object speed
        /// </summary>
        public Vector2 speed = new Vector2(10, 10);

        /// <summary>
        /// Moving direction
        /// </summary>
        public Vector2 direction = new Vector2(0, -1);

        private Vector2 movement;

        private void Update()
        {
            // 2 - Movement
            movement = new Vector2(
                speed.x*direction.x,
                speed.y*direction.y);
        }

        private void FixedUpdate()
        {
            // Apply movement to the rigidbody
            rigidbody2D.velocity = movement;
        }
    }

}