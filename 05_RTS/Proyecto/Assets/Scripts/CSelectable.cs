using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSelectable : MonoBehaviour
{
    [HideInInspector]
    public bool selected = false;

    private Material outlineMaterial;

    void Awake()
    {
        outlineMaterial = GetComponent<MeshRenderer>().GetComponent<Renderer>().materials[1];
        outlineMaterial.SetFloat("_Outline_Width", 0f);
    }

    /// <summary>
    ///     Sets entity outline material color
    /// </summary>
    public void Set_Color(Color color)
    {
        outlineMaterial.SetColor("_Outline_Color", color);
    }
    /// <summary>
    ///     Changes entity to selected mode
    /// </summary>
    public void Set_Selected()
    {
        selected = true;
        outlineMaterial.SetFloat("_Outline_Width", 0.08f);
    }
    /// <summary>
    ///     Changes entity to deselected mode
    /// </summary>
    public void Set_Deselected()
    {
        selected = false;
        outlineMaterial.SetFloat("_Outline_Width", 0f);
    }

    private void OnDestroy()
    {
        Destroy(outlineMaterial);
    }
}
