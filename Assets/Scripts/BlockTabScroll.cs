using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockTabScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool mouseOver = false;
    public float initialY;

    private void Awake()
    {
        initialY = transform.position.y;
    }

    private void Update()
    {
        if (mouseOver)
        {
            transform.position += new Vector3(0, 20 * -Input.mouseScrollDelta.y);
            if (transform.position.y < initialY)
            {
                transform.position = new Vector3(transform.position.x, initialY);
            }
        }
    }

    public void Reposition()
    {
        transform.position = new Vector3(transform.position.x, initialY);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }
}
