using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] Rigidbody2D doorRigidBody;
    [SerializeField] Transform doorPosition;
    Vector3 closedPosition;
    Vector3 openPosition;

    [SerializeField] [Tooltip("Used to determine who can enter this area")] int accessLevel = 0;
    [SerializeField] [Tooltip("Dictate whether the door will open left or right")] bool opensToRight = false;

    List<GameObject> currentGOs = new List<GameObject>(); 

    // Start is called before the first frame update
    void Start()
    {
        closedPosition = doorPosition.localPosition;

        if (opensToRight)
            openPosition = new Vector3(doorPosition.localPosition.x + 2, doorPosition.localPosition.y, doorPosition.localPosition.z);
        else
            openPosition = new Vector3(doorPosition.localPosition.x - 2, doorPosition.localPosition.y, doorPosition.localPosition.z);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var go = collision.gameObject;
        
        // Determine whether the collision was with an entity possessing an "accessLevel" property
        if (go != null)
        {
            int entityAccessLevel = 0;

            // TODO: when there's a basic inventory system in place, change this line to check for an inventory system, an existing key card or etc.
            go.TryGetComponent<Inventory>(out Inventory inventory);

            // If entity has an Inventory
            if (inventory != null)
                entityAccessLevel = inventory.areaAccessLevel;            

            if (entityAccessLevel >= accessLevel)
            {
                Debug.Log(go);
                currentGOs.Add(go);

                if (currentGOs.Count < 2)
                    OpenDoor();
            }
                
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var go = collision.gameObject;

        if (currentGOs.Contains(go))
            currentGOs.Remove(go);

        if (currentGOs.Count <= 0)
            CloseDoor();
    }


    void OpenDoor()
    {
        doorPosition.localPosition = openPosition;        
    }

    void CloseDoor()
    {
        doorPosition.localPosition = closedPosition;
    }
}
