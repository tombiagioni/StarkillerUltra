using UnityEngine;
using System.Collections;

namespace Shipception
{ 
public class Item : MonoBehaviour {

    float randomX;
    float randomY;
    float smoothLerp = 0.01f;
    Vector3 basePosition;
    Vector3 newPosition;

    public float playerAttractionRange = 0.1f;

    bool asleep = true;

    bool ready = false;
    bool attracted = false;
    bool earned = false;
    float distanceFromPlayer;

    public float boundToCamDelay;
    public float boundToCamDelayMax = 600.0f;

    // Object references :
    Transform myTr;
    GameObject myGo;
    SpriteRenderer mySpriteRdr;
    Animator myAnimator;

    // Player :
    GameObject player;
    Transform playerTr;
    PlayerScript playerScript;

    // Camera :
    Camera cam;
    Transform camTr;

    public virtual void OnEnable()
    {

        // Check if the script has initialized
        if (ready == true)
        {
            SetItemType(); // Setup item upgrade type
        }

    }

    public virtual void OnBecameVisible()
    {

        // Wake up object
        asleep = false;

    }

    public virtual IEnumerator DestroyObject()
    {

        if (audio.isPlaying) yield return new WaitForSeconds(audio.clip.length);  // Wait for the end of explosion audio clip

        yield return null; // yield function is needed because an animation is playing

        if (gameObject.activeInHierarchy == true)
        {
            // Restore the Animator and the sorting order of the sprite, that were possibly changed to display upgrade notification (in "Update()")
            myAnimator.enabled = true;
            mySpriteRdr.sortingOrder = 2;

            myGo.SetActive(false);
            earned = attracted = false;
            asleep = true;
            boundToCamDelay = 0.0f;
            mySpriteRdr.color = Color.white;
        }

    }

    public virtual void OnBecameInvisible()
    {

        // Destroy object
        if (gameObject.activeInHierarchy == true && ready == true) StartCoroutine(DestroyObject());

    }

	// Use this for initialization
	public virtual void Start () {
        myGo = gameObject;
        myTr = transform;
        mySpriteRdr = myTr.GetComponent<SpriteRenderer>();
        myAnimator = myTr.GetComponent<Animator>();

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();
        playerTr = player.transform;

        cam = Camera.main;
        camTr = cam.transform;

        StartCoroutine(Prepare());
	
	}

    public IEnumerator Prepare()
    {

        float rand = UnityEngine.Random.Range(-1.0f, 1.0f);
        float randAdd = UnityEngine.Random.Range(-0.25f, 0.25f);

        if (rand <= 0) randomX = -1 + randAdd;
        else randomX = 1 + randAdd;


        rand = UnityEngine.Random.Range(-1.0f, 1.0f);
        randAdd = UnityEngine.Random.Range(-0.25f, 0.25f);
        if (rand <= 0) randomY = -1 + randAdd;
        else randomY = 1 + randAdd;

        smoothLerp = .4f;//smoothLerp = Random.Range(1.0, 2.0);

        yield return null;

        ready = true;

        SetItemType(); // Determine wich type of item is created

    }

    public virtual void SetItemType()
    {
        
    }
	
	// Update is called once per frame
	public virtual void Update () {
	
	}
}
}