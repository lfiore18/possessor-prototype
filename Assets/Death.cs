using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{

    Rigidbody2D rb;
    [SerializeField] float decelerationRate = 1.0f;
    [SerializeField] GameObject bloodDecalPrefab;
    bool isDead = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            DecelerateBody();
    }


    void DecelerateBody()
    {
        if (rb.velocity.magnitude > 0)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, decelerationRate * Time.deltaTime);
            rb.freezeRotation = true;
        }
        else
        {
            rb.velocity = Vector3.zero;
            Destroy(this);
            // Optionally, you can set isDead to false or perform other actions
            // once the enemy body has fully stopped.
        }
    }

    public void Kill(Vector2 hitPoint)
    {
        isDead = true;
        CreateBloodStain(hitPoint);

    }

    void CreateBloodStain(Vector2 hitPoint)
    {
        if (bloodDecalPrefab != null)
        {
            Instantiate(bloodDecalPrefab, hitPoint, Quaternion.Euler(0, 0, Random.Range(0, 359)));            
        }
        else
        {
            Debug.LogWarning("Blood Decal effect not set on " + this);
        }
    }
}
