using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.TryGetComponent<Death>(out Death enemy);
        //if (collision.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>()) Destroy(gameObject);

        if (enemy != null)
        {
            Vector2 hitPoint = collision.GetContact(0).point;
            enemy.Kill(hitPoint);
        }

        Destroy(gameObject);
    }
}
