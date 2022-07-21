using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorThingsUI : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private StringListEvent showObservedProperty;

    [SerializeField] private Button buttonTemplate;

    [Header("Invoke")]
    [SerializeField] private StringEvent filterOnPropertyID;
    [SerializeField] private TriggerEvent getAllObservableProperties;

    void Awake()
    {
        showObservedProperty.started.AddListener(DrawObservedProperty);
        buttonTemplate.gameObject.SetActive(false);

        getAllObservableProperties.started.Invoke();
    }

    private void DrawObservedProperty(List<string> nameDescriptionAndID)
    {
        var propertyButton = Instantiate(buttonTemplate,this.transform);
        propertyButton.gameObject.SetActive(true);
        propertyButton.name = nameDescriptionAndID[2];

        var prettyName = $"{nameDescriptionAndID[1]} ({nameDescriptionAndID[0]})".ToFirstCharacterUpperCase();
        propertyButton.GetComponentInChildren<Text>().text = prettyName;

        propertyButton.onClick.AddListener(() => SelectedPropertyFilter(propertyButton.name));
    }

    private void SelectedPropertyFilter(string id)
    {
        filterOnPropertyID.Invoke(id);
    }

    public void ClearButtons()
    {
        foreach (Transform button in transform)
        {
            if (button.gameObject.activeSelf)
            {
                button.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
        }
    }
}
