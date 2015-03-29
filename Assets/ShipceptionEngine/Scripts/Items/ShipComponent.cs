using UnityEngine;
using System.Collections;

namespace Shipception
{
    public enum ComponentType
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

    public enum ComponentTier
    {
        Common = 1,
        Uncommon,
        Rare,
        Unique
    }

    public enum ComponentScaleTier
    {
        Fighter = 1,
        Destroyer,
        Carrier,
        BattleCruiser,
        Dreadnought
    }
    public class ShipComponent : Item
    {
        public float AttackBuff;
        public float SpeedBuff;
        public float DefenseBuff;
        public float VitalityBuff;


        public ComponentType Type;
        public ComponentTier Tier;
        public ComponentScaleTier ScaleTier;

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
        }

    }
}
