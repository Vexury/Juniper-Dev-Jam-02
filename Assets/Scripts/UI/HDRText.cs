using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class HDRText : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color hdrColor = Color.white;

    void OnValidate()
    {
        var tmp = GetComponent<TextMeshPro>();
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, hdrColor);
    }
}