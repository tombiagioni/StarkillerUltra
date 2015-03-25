using UnityEngine;
using System.Collections;


namespace Shipception
{
    public class EnemyShip : Ship
    {

        // Player cached components :
        GameObject _player;
        Transform _playerTr;
        PlayerScript _playerScript;


        // Vector 2 direction of the object
        Vector2 _seekDir = new Vector2(-1.0f, 0.0f);

        // delay before changing direction ("_seekDir")
        float _seekDelay = 0.0f;
        float _seekDelayMax = 2.0f;

        // Use this for initialization
        public override void Start()
        {

            _player = GameObject.FindWithTag("Player");
            _playerScript = _player.GetComponent<PlayerScript>();
            _playerTr = _player.transform;
            base.Start();
        }


        // Update is called once per frame
        public override void Update()
        {
            TempMovement();
            base.Update();
        }

        public override IEnumerator ApplyDamage(int damage)
        {
            _playerScript.UpdateScore(ScoreValue);
            return base.ApplyDamage(damage);
        }

        void TempMovement()
        {
            // Direction : Forward
            if (_seekDir == new Vector2(-1.0f, 0.0f) || _seekDir == new Vector2(0.0f, 0.0f))
            {
                _myTr.position = new Vector3(_myTr.position.x - Speed * Time.deltaTime, _myTr.position.y, _myTr.position.z);
                if (_mySpriteRdr.sprite != GoForward) _mySpriteRdr.sprite = GoForward;
            }

            // Direction : Up
            else if (_seekDir == new Vector2(-1.0f, 1.0f))
            {
                _myTr.position = new Vector3(_myTr.position.x - Speed * 0.5f * Time.deltaTime, _myTr.position.y + Speed * 0.5f * Time.deltaTime, _myTr.position.z);

                if (_mySpriteRdr.sprite != GoUp) _mySpriteRdr.sprite = GoUp;
            }

            // Direction : Down
            else if (_seekDir == new Vector2(-1.0f, -1.0f))
            {
                _myTr.position = new Vector3(_myTr.position.x - Speed * 0.5f * Time.deltaTime, _myTr.position.y - Speed * 0.5f * Time.deltaTime, _myTr.position.z);

                if (_mySpriteRdr.sprite != GoDown) _mySpriteRdr.sprite = GoDown;
            }

            // If seek delay doesn't reach max delay, increment it and abort the function
            if (_seekDelay < _seekDelayMax) { _seekDelay = _seekDelay + 0.1f; return; }

            // (else) Reset the delay counter
            _seekDelay = 0.0f;

            // if gameObject is behind player, change direction to forward
            if (_myTr.position.x < _playerTr.position.x || _mySpriteRdr.isVisible == false) _seekDir = new Vector2(-1.0f, 0.0f);


            // Else if player is above gameObject, change direction to down
            else if (_myTr.position.y < _playerTr.position.y - 0.03f) _seekDir = new Vector2(-1.0f, 1.0f);


            // else If player is under gameObject, change direction to up
            else if (_myTr.position.y > _playerTr.position.y + 0.03f) _seekDir = new Vector2(-1.0f, -1.0f);


            // else player is in front of gameObject, change direction to forward
            else _seekDir = new Vector2(-1.0f, 0.0f);

        }

    }

}

