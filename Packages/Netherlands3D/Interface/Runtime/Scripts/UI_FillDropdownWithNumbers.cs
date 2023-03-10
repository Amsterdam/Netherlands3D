using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FillDropdownWithNumbers : MonoBehaviour
{
    [SerializeField]
    private string firstItem = "Choose";

    [SerializeField]
    private int from = 1;

    [Tooltip("(Inclusive)")]
    [SerializeField]
    private int to = 12;

    private Dropdown dropdown;

    [Header("Invoke events")]
    [SerializeField]
    private IntEvent onSelectedNumberValue;

    private void Start()
	{
        dropdown = GetComponent<Dropdown>();
    }

    /// <summary>
    /// Invokes event with dropdown value as number
    /// </summary>
    /// <param name="dropdownItem">Dropdown option index</param>
    public void InvokeNumberValue(int dropdownItemIndex)
    {
        if(onSelectedNumberValue && int.TryParse(dropdown.options[dropdownItemIndex].text,out int parsedValue))
        {
            onSelectedNumberValue.InvokeStarted(parsedValue);
        }
    }


	void OnValidate()
    {
        Dropdown dropdown = GetComponent<Dropdown>();
        if(!dropdown)
        {
            Debug.Log("Component is not on a Dropdown UI item.");
            return;
		}

        List<string> options = new List<string>();
        options.Add(firstItem);
        for (int i = from; i <= to; i++)
		{
            options.Add(i.ToString());
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }
}
