using UnityEngine;
using System.Collections;

namespace Shipception
{
    enum MyEnum
    {
        Engine = 1,
        Wings,
        Cannon,
        Cockpit,
        Shield,
        Orbital,
        Missile,
        Bomb
    }
    public class ShipComponent : Item
    {
        public float AttackBuff;
        public float SpeedBuff;
        public float DefenseBuff;
        public float VitalityBuff;

        // Use this for initialization
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }
    }
}
