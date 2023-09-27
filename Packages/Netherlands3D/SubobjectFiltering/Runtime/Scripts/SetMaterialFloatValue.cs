using UnityEngine;

public class SetMaterialFloatValue : MonoBehaviour
{
    [SerializeField]
    private Material material;

    [SerializeField]
    private string valueName = "";

    [SerializeField]
    private bool invertNormalisedFloat = true;
    public void Set(float value)
    {
        material.SetFloat(valueName, (invertNormalisedFloat) ? 1-value : value);
    }
}
