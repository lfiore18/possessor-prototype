using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Possessor : MonoBehaviour
{
    [SerializeField] Movement playerBody;
    [SerializeField] Movement currentlyPossessed;
    [SerializeField] CinemachineVirtualCamera followCamera;

    Crosshair crosshair;    


    private void Start()
    {
        currentlyPossessed = playerBody;
        crosshair = GetComponent<Crosshair>();
        crosshair.SetPlayerBody(currentlyPossessed);
        followCamera.Follow = playerBody.transform;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.E) && currentlyPossessed != playerBody)
        {
            PassControlToEntity(playerBody);            
        }
    }

    // On collision with crosshair, check if colliding object has "possessable" tag
    // Check if left mouse is down and the collider is not attached to player body
    // Set currentlyPossessed to the movement script of the collided object
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Possessable"))
        {
            Debug.Log(collision.name + " is possessable");
            if (Input.GetButtonDown("Fire1") && collision.name != playerBody.name)
            {
                PassControlToEntity(collision.GetComponent<Movement>());
            }
        }
    }

    private void PassControlToEntity(Movement entity)
    {
        // If the entity has default enemy behaviour, turn it off before possessing entity
        if (entity.GetComponent<EnemyBehaviour>() != null)
        {
            entity.GetComponent<EnemyBehaviour>().enabled = false;
        }

        // If you're chaining possessions, reactivate the enemy behaviour on the entity you're relingquish control of
        if (currentlyPossessed.GetComponent<EnemyBehaviour>() != null)
        {
            currentlyPossessed.GetComponent<EnemyBehaviour>().enabled = true;
        }

        currentlyPossessed.enabled = false;
        currentlyPossessed = entity;
        currentlyPossessed.enabled = true;

        crosshair.SetPlayerBody(entity);
        followCamera.Follow = entity.transform;
    }
}
