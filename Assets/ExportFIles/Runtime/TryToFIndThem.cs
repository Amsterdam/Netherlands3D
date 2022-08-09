using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;

public class TryToFIndThem : MonoBehaviour
{

    List<Geometryfile> FileFormatClasses = new List<Geometryfile>();
    [SerializeField] List<string> availableFileFormats;

    [Header("input")]
    [SerializeField] StringListEvent SelectedLayers;

    [SerializeField] List<string> availableLayers;

    // Start is called before the first frame update
    void Start()
    {
        getAvailableLayersV2();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        getAvailableFileFormats();
    }

    void getAvailableLayers()
    {
        TileHandler tileHandler = FindObjectOfType<TileHandler>();
        List<Layer> layers = tileHandler.layers;
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].GetType() == typeof(BinaryMeshLayer))
            {
                availableLayers.Add(layers[i].gameObject.name);
            }
        }
    }

    void getAvailableLayersV2()
    {
        Layer[] layers = FindObjectsOfType<BinaryMeshLayer>();
        for (int i = 0; i < layers.Length; i++)
        {
                availableLayers.Add(layers[i].gameObject.name);
         }
    }


    void getAvailableFileFormats()
    {
        FileFormatClasses.Clear();
        availableFileFormats = new List<string>();
        var types = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(Geometryfile).IsAssignableFrom(t) &&
                        t != typeof(Geometryfile))
                    .ToArray();

        foreach (var item in types)
        {
            Geometryfile newService = (Geometryfile)System.Activator.CreateInstance(item);
            FileFormatClasses.Add(newService);
            availableFileFormats.Add(newService.filetype);
        }
    }
}
