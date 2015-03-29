using UnityEngine;
using System.Collections;

namespace Shipception
{
    public class PlayerShip : Ship
    {

        public ShipComponent Wings;
        public ShipComponent Cannon;
        public ShipComponent Engine;
        public ShipComponent Cockpit;

        public float AttackBase;
        public float SpeedBase;
        public float DefenseBase;
        public float VitalityBase;

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