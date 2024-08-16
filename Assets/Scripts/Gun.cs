using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float fireRate = 0.2f;

    Coroutine firingCoroutine;


    public void Fire()
    {
        firingCoroutine = StartCoroutine(FireContinuously());
    }

    public void StopFiring()
    {
        StopCoroutine(firingCoroutine);
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject bulletInstance =
                Instantiate(bulletPrefab,
                            transform.GetChild(0).position,
                            Quaternion.identity)
                as GameObject;

            bulletInstance.transform.rotation = transform.rotation;
            bulletInstance.GetComponent<Rigidbody2D>().velocity = transform.up * projectileSpeed;
            yield return new WaitForSeconds(fireRate);
        }
    }
}
