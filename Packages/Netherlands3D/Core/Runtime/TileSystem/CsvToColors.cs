using Netherlands3D.Core;
using Netherlands3D.Core.Colors;
using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CsvToColors : MonoBehaviour
{
    [SerializeField]
    private int idColumn = 0;
    [SerializeField]
    private int colorColumn = 2;
    private Coroutine runningParse;

    [SerializeField]
    private ColorInterpretation colorInterpretation = ColorInterpretation.HEX;

    [Header("Interpolation")]
    [SerializeField]
    private double minimumValue;
    [SerializeField]
    private double maximumValue;
    [SerializeField]
    private GradientContainer gradientContainer;

    [Header("Default")]
    [SerializeField]
    private Color defaultColor;

    [Header("Listen to events")]
    [SerializeField]
    private IntEvent onSetIDColumn;
    [SerializeField]
    private IntEvent onSetColorColumn;
    [SerializeField]
    private TriggerEvent onSetHEXColorMode;
    [SerializeField]
    private TriggerEvent onSetInterpolateColorMode;
    [SerializeField]
    private GradientContainerEvent onSetGradient;
    [SerializeField]
    private StringEvent parseCSVFile;

    [Header("Invoke events")]
    [SerializeField]
    private ObjectEvent parsedIdsAndColors;

	private void Awake()
	{
		AddEventListeners();
	}

	private void AddEventListeners()
	{
		if (parseCSVFile)
		{
			parseCSVFile.started.AddListener(ParseData);
		}
		if (onSetIDColumn)
		{
			onSetIDColumn.started.AddListener(SetIDColumn);
		}
		if (onSetColorColumn)
		{
			onSetColorColumn.started.AddListener(SetColorColumn);
		}
		if (onSetHEXColorMode)
		{
			onSetHEXColorMode.started.AddListener(SetHexColorMode);
		}
		if (onSetInterpolateColorMode)
		{
			onSetInterpolateColorMode.started.AddListener(SetInterpolateColorMode);
		}
		if (onSetGradient)
		{
			onSetGradient.started.AddListener(SwapGradient);
		}
	}

	public enum ColorInterpretation
    {
        HEX,
        INTERPOLATE
    }

    public void ParseData(string csvPath)
    {
        if(runningParse != null)
        {
            StopCoroutine(runningParse);
		}
        runningParse = StartCoroutine(LoadCSV(csvPath));
	}

    public void SwapGradient(GradientContainer newGradientContainer)
    {
        gradientContainer = newGradientContainer;
    }

    private void SetInterpolateColorMode()
	{
        colorInterpretation = ColorInterpretation.INTERPOLATE;
    }

	private void SetHexColorMode()
	{
        colorInterpretation = ColorInterpretation.HEX;
    }

	private void SetIDColumn(int columnIndex)
	{
        idColumn = columnIndex;
    }
    private void SetColorColumn(int columnIndex)
    {
        colorColumn = columnIndex;
    }

    private IEnumerator LoadCSV(string csvPath)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(csvPath))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Could not load {csvPath}");
            }
            else
            {
                Dictionary<string, Color> idColors = new Dictionary<string, Color>();
                //Ready CSV lines ( skip header )
                var lines = CsvParser.ReadLines(webRequest.downloadHandler.text, 1);
                foreach (var line in lines)
                {
                    Color color = Color.magenta;
                    string id = line[idColumn];
                    ParseColor(line[colorColumn], out color);

                    if (idColors.ContainsKey(id))
                    {
                        Debug.Log($"Duplicate key found in dataset:{id}. Skipping.");
                    }
                    else
                    {
                        idColors.Add(id, color);
                    }
                }
                parsedIdsAndColors.started.Invoke(idColors);
            }
            
        }
    }

    private void ParseColor(string colorInput, out Color color)
    {
        color = Color.white;
        switch (colorInterpretation)
        {
            case ColorInterpretation.HEX:
                ColorUtility.TryParseHtmlString(colorInput, out color);
                break;
            case ColorInterpretation.INTERPOLATE:
                if (float.TryParse(colorInput, out float parsed))
                {
                    color = gradientContainer.gradient.Evaluate(Mathf.InverseLerp((float)minimumValue, (float)maximumValue, parsed));
                }
                else
                {
                    Debug.Log($"Cant parse {colorInput} as float");
                }
                break;
            default:
                break;
        }
    }
}
