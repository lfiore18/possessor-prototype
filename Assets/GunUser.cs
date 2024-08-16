using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatBehaviour
{
    void Attack();
    void StopAttacking();
}

public class GunUser : MonoBehaviour, ICombatBehaviour
{
    [SerializeField] Gun gun;
    [SerializeField] float fireFor = 2f;
    [SerializeField] float waitFor = 2f;
    [SerializeField] bool isFiring = false;
    [SerializeField] Coroutine combatCoroutine;


    void Start()
    {
        if (gun == null)
        {
            gun = GetComponentInChildren<Gun>();
        }
    }

    // If enemy is a gun wielder, should start firing when player is in line of sight
    public void Attack()
    {
        if (!isFiring)
        {
            isFiring = true;
            combatCoroutine = StartCoroutine(WaitAndFire(fireFor, waitFor));
            Debug.Log("firing");
        }
    }


    // Immediatley stop firing and combat coroutines
    public void StopAttacking()
    {
        isFiring = false;
        
        gun.StopFiring();
        StopCoroutine(combatCoroutine);        
    }

    IEnumerator WaitAndFire(float fireForSecs, float waitForSecs)
    {
        gun.Fire();
        yield return new WaitForSeconds(fireForSecs);
        gun.StopFiring();
        yield return new WaitForSeconds(waitForSecs);
        isFiring = false;
    }
}
