using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Combat/Killable Child Collision")]
// ReSharper disable once CheckNamespace
public class KillableChildCollision : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public Killable killable;
    // ReSharper restore InconsistentNaming

    private bool _isValid = true;

    private Killable KillableToAlert {
        get {
            if (killable != null) {
                return killable;
            }

            if (transform.parent != null) {
                var parentKill = transform.parent.GetComponent<Killable>();

                if (parentKill != null) {
                    killable = parentKill;
                }
            }

            if (killable != null)
            {
                return killable;
            }
            LevelSettings.LogIfNew("Could not locate Killable to alert from KillableChildCollision script on GameObject '" + name + "'.");
            _isValid = false;
            return null;
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnCollisionEnter(Collision collision) {
        if (!_isValid) {
            return;
        }

        var kill = KillableToAlert;
        if (!_isValid) {
            return;
        }

        kill.CollisionEnter(collision);
    }

    // ReSharper disable once UnusedMember.Local
    void OnTriggerEnter(Collider other) {
        if (!_isValid) {
            return;
        }

        var kill = KillableToAlert;
        if (!_isValid) {
            return;
        }

        kill.TriggerEnter(other);
    }

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
    // not supported
#else
    // ReSharper disable once UnusedMember.Local
		void OnCollisionEnter2D(Collision2D coll) {
			if (!_isValid) {
				return;
			}
			
			KillableToAlert.CollisionEnter2D(coll);
		}
	
    // ReSharper disable once UnusedMember.Local
		void OnTriggerEnter2D(Collider2D other) {
            if (!_isValid) {
				return;
			}
			
			KillableToAlert.TriggerEnter2D(other);
		}
#endif
}