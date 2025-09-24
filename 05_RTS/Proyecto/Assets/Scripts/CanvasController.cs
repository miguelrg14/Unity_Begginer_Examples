using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasController : MonoBehaviour
{
    private Canvas canvas;

    public ArmyController army;

    public RectTransform selectionImage;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    // Start is called before the first frame update
    void Start()
    {
        selectionImage.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (army.selecting)
        {
            selectionImage.gameObject.SetActive(true);

            selectionImage.anchoredPosition = new Vector2(army.selectionRect.x / canvas.scaleFactor, army.selectionRect.y / canvas.scaleFactor);
            selectionImage.sizeDelta = new Vector2(army.selectionRect.width / canvas.scaleFactor, army.selectionRect.height / canvas.scaleFactor);
        }
        else
        {
            selectionImage.gameObject.SetActive(false);
        }
    }
}
