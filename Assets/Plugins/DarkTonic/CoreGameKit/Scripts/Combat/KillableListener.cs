using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/Killable Listener")]
// ReSharper disable once CheckNamespace
public class KillableListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
    public string sourceKillableName;
    // ReSharper restore InconsistentNaming

    // ReSharper disable once UnusedMember.Local
    void Reset() {
        var src = GetComponent<Killable>();
        if (src == null)
        {
            return;
        }
        src.listener = this;
        sourceKillableName = name;
    }

    public virtual void SpawnerDestroyed() {
        // your code here.
    }

    public virtual void Despawning(TriggeredSpawner.EventType eType) {
        // your code here.
    }

    public virtual void Despawned() {
        // your code here
    }

    public virtual void TakingDamage(int pointsDamage, Killable enemyHitBy) {
        // your code here.
    }

    public virtual void DamagePrevented(int pointsDamage, Killable enemyHitBy) {
        // your code here.
    }

    public virtual void DamagePrefabSpawned(Transform damagePrefab) {
        // your code here.
    }

    public virtual void DamagePrefabFailedToSpawn(Transform damagePrefab) {
        // your code here.  
    }

	public virtual void DeathDelayStarted(float delayTime) {
		// your code here.
	}
	
    public virtual void DeathPrefabSpawned(Transform deathPrefab) {
        // your code here.
    }

    public virtual void DeathPrefabFailedToSpawn(Transform deathPrefab) {
        // your code here.  
    }

    public virtual void ModifyingDamageWorldVariables(List<WorldVariableModifier> variableModifiers) {
        // your code here. You can change the variable modifiers before they get used if you want.
    }

    public virtual void ModifyingDeathWorldVariables(List<WorldVariableModifier> variableModifiers) {
        // your code here. You can change the variable modifiers before they get used if you want.
    }

	public virtual void WaitingToDestroyKillable(Killable deadKillable) {
		// your code here;
	}

    public virtual void DestroyingKillable(Killable deadKillable) {
        // your code here.
    }

    public virtual string DeterminingScenario(Killable deadKillable, string scenario) {
        // if you wish to use logic to change the Scenario, do it here. Example below.

        // if (yourLogicHere == true) {
        //   scenario = "ReachedTower";
        // }

        return scenario;
    }

    public virtual void Spawned(Killable newKillable) {
        // your code here
    }
}
