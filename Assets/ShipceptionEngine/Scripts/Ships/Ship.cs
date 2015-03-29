using UnityEngine;
using System.Collections;

namespace Shipception
{

    enum ShipScaleTier
    {
        Fighter = 1,
        Destroyer,
        Carrier,
        BattleCruiser,
        Dreadnought
    }
public class Ship : MonoBehaviour {








        [HideInInspector] bool gamePause;	// Is the game paused ? (sent by "MainScript()")
        
        //base values
        public int Health;
        public int ScoreValue = 100;
        [HideInInspector] public int DamageValue = 1;
        public float Speed;

        //Object Cached components
        protected Transform _myTr;
        protected SpriteRenderer _mySpriteRdr;

        // Player exhaust cached components :
        GameObject myExhaustGo;		// Target exhaust's gameObject
        Transform myExhaustTr;		// Target exhaust's transform

        // Limits player's position relative to camera position :
        public Vector2 ScreenLimitsMin;
        public Vector2 ScreenLimitsMax;

        bool _asleep = true;

        // Sprite references :
        public Sprite GoForward;
        public Sprite GoUp;
        public Sprite GoDown;

        public Sprite MoveLeft;
        public Sprite MoveRight;

        // the base position used for Y movement
        private float _basePosY;

        //Explosion
        private ObjectPoolerScript _explosionPool;
        GameObject _explosionClone;	// value to cache spawned object

        // Coins :
        public bool GiveCoins = true;
        //var coin : GameObject;
        ObjectPoolerScript _coinPool;
        GameObject _coinClone;
        Vector3 _coinPos;
        int i;

        // Upgrade :
        public bool GiveUpgrade = false;
        ObjectPoolerScript _upgradePool;
        public Color UpgradeHolderColor = Color.yellow;


    //Break this out into the Weapon Class
        // Ship speed and weapon level
        public int SpeedLevel = 1;
        public int SpeedLevelMax = 6;
        public int WeaponLevel = 1;
        public int WeaponLevelMax = 3;



        public void OnBecameVisible()
        {

            // On became visible, wake up the object
            _asleep = false;
            _myTr.collider2D.enabled = true;

        }

        public void OnBecameInvisible()
        {

            // On became invisible, destroy object
            if (gameObject.activeInHierarchy == true) StartCoroutine(DestroyObject());

        }
        public IEnumerator DestroyObject()
        {

            yield return new WaitForSeconds(audio.clip.length); // Wait for the end of explosion audio clip

            if (gameObject.activeInHierarchy == true) Destroy(gameObject); // Kills the game object

        }

        // Called by 'MainScript'
        public virtual void OnPauseGame()
        {

            gamePause = true;

        }

        // Called by 'MainScript'
        public virtual void OnResumeGame()
        {

            gamePause = false;

        }



        public virtual void Start()
        {
            // cache object transform and renderer
            _myTr = transform;
            _mySpriteRdr = _myTr.GetComponent<SpriteRenderer>();

            _explosionPool = GameObject.Find("ObjectPool ShipExplosions").GetComponent<ObjectPoolerScript>();
            _coinPool = GameObject.Find("ObjectPool ItemCoins").GetComponent<ObjectPoolerScript>();
            _upgradePool = GameObject.Find("ObjectPool ItemUpgrades").GetComponent<ObjectPoolerScript>(); 
            



            StartCoroutine(DisableCollider());

        }

        // Disable collider some times after scene has initialised
        // Collider is disabled to be sure that nothing will reach an out of screen object
        // Disabling is delayed to let "MainScript"'s "Spawn()" function, wich use enemies colliders, operate first.
        public virtual IEnumerator DisableCollider()
        {

            yield return new WaitForSeconds(0.3f);

            _myTr.collider2D.enabled = false;

        }

        public virtual void Update()
        {
            // If game is paused abort the function (sent by "MainScript()")
            if (gamePause == true) return;

            // If our object is sleeping then abort the function
            if (_asleep == true || Health <= 0) return;
        }


        public virtual IEnumerator ApplyDamage(int damage)
        {

            // Ensure that object receiving damage is not sleeping (and therefore out of screen)
            if (_asleep == true) yield break; //return null; // FIX CS 13 01 2015 VERIFIER !!
            Health = Health - 1;

            if (Health > 0)
            {
                StartCoroutine(DamageBlink());
                // ... play an impact sound
            }

            else
            {
                audio.Play();
                _myTr.collider2D.enabled = false;
                _myTr.renderer.enabled = false;
                //REGULAR METHOD : INSTANTIATE (unoptimised)
                //explosionClone = Instantiate(explosion, Vector3 (myTr.position.x, myTr.position.y, myTr.position.z), Quaternion.identity);

                // Pooling Method : grab object from "ObjectPooler"'s gameObjects list
                _explosionClone = _explosionPool.Spawn();
                _explosionClone.transform.position = _myTr.position;

                // search for "GiveCoins" value in player preferences
                if (PlayerPrefs.HasKey("Give coins") == false || PlayerPrefs.GetInt("Give coins") == 1) GiveCoins = false; // we use the value 2 for true, and one for false
                else if (PlayerPrefs.GetInt("Give coins") == 2) GiveCoins = true;

                if (GiveCoins == true)
                {
                    _coinPos = _myTr.position;

                    for (i = 0; i < ScoreValue / 10; i++)
                    {
                        //REGULAR METHOD : INSTANTIATE (unoptimised)
                        //coinClone = Instantiate(coin, coinPos, Quaternion.identity);

                        // Pooling Method : grab object from "ObjectPooler"'s gameObjects list
                        _coinClone = _coinPool.Spawn();
                        _coinClone.transform.position = _coinPos;

                        yield return null;
                    }
                }

                if (GiveUpgrade == true)
                {
                    GameObject upgradeClone = _upgradePool.Spawn();
                    upgradeClone.transform.position = _myTr.position;
                }

                //playerScript.UpdateScore(scoreValue);
                StartCoroutine(DestroyObject());
                StartCoroutine(DestroyObject());
            }

        }

        public virtual IEnumerator DamageBlink()
        {

            _mySpriteRdr.color = new Color(_mySpriteRdr.color.r, _mySpriteRdr.color.g, _mySpriteRdr.color.b, 0.0f);

            yield return new WaitForSeconds(0.05f);

            _mySpriteRdr.color = new Color(_mySpriteRdr.color.r, _mySpriteRdr.color.g, _mySpriteRdr.color.b, 1.0f);

        }


    }
}

