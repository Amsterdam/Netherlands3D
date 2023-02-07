using UnityEngine;
using System.Collections;
using UnityEditor;
using Netherlands3D.Core.Tiles;

[CustomEditor(typeof(WebTilePrioritiser))]
class DrawTilePriorityLabels : Editor
{
    void OnSceneGUI()
    {
        WebTilePrioritiser tilePrioritiser = (WebTilePrioritiser)target;
        if (tilePrioritiser == null || !tilePrioritiser.showPriorityNumbers)
        {
            return;
        }

        Handles.color = Color.blue;

        foreach(var tile in tilePrioritiser.PrioritisedTiles)
        {
            Handles.Label(tile.Bounds.center, $"{tile.priority}");
        }
    }
}