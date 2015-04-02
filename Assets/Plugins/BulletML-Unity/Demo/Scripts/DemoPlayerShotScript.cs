// Copyright © 2014 Pixelnest Studio
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.
using UnityEngine;
using System.Collections;

namespace Pixelnest.BulletML.Demo
{
  public class DemoPlayerShotScript : MonoBehaviour
  {
    public Vector2 speed = Vector2.zero;

    void Update()
    {
      // Destroy when outside the screen
      if (renderer != null && renderer.isVisible == false)
      {
        Destroy(this.gameObject);
      }
    }

    void FixedUpdate()
    {
      rigidbody2D.velocity = speed;
    }
  }
}