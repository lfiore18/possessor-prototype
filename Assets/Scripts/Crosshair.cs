using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    Vector2 mousePos;
    Vector2 moveOffset;

    // TODO: Consider decoupling player from crosshair
    Movement player;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        transform.position = new Vector3(0, 0, 1);
        player = GameObject.Find("Player").GetComponent<Movement>();
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        moveOffset = new Vector2(player.GetDeltaX(), player.GetDeltaY());

        transform.position = new Vector2(mousePos.x + moveOffset.x, mousePos.y + moveOffset.y);
    }

    public void SetPlayerBody(Movement currentlyPossessed)
    {
        player = currentlyPossessed;
    }
}
