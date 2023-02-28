using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Visualisers
{
	public class LineVisualiser : MonoBehaviour
	{
		[SerializeField]
		private Vector3ListEvent lineCoordinatesEvent;

		[SerializeField]
		private TriggerEvent finishedDrawing;

		[SerializeField]
		private Material lineRendererMaterial;

		[SerializeField]
		private Color lineColor;

		[SerializeField]
		private float thickness = 1.0f;

		[SerializeField]
		private Vector3 offset = Vector3.zero;

		[SerializeField] private GameObjectEvent lineParentEvent;
		[SerializeField] private ColorEvent lineColorEvent;

		private Transform lineParent;

		void Start()
		{
			lineCoordinatesEvent.started.AddListener(DrawLine);
			if(lineParentEvent) lineParentEvent.AddListenerStarted((parentObject) => lineParent = parentObject.transform);
			if (lineColorEvent) lineColorEvent.AddListenerStarted((color) => lineColor = color);
		}

		public void DrawLine(List<Vector3> linePoints)
		{
			var lineRenderObject = new GameObject();
			if(lineParent != null)
            {
				lineRenderObject.transform.SetParent(lineParent);
            }
            else
            {
				lineRenderObject.transform.SetParent(this.transform);
            }

			LineRenderer newLineRenderer = lineRenderObject.AddComponent<LineRenderer>();
			newLineRenderer.positionCount = linePoints.Count;
			newLineRenderer.material = lineRendererMaterial;
			newLineRenderer.startWidth = thickness;
			newLineRenderer.endWidth = thickness;
			newLineRenderer.startColor = lineColor;
			newLineRenderer.endColor = lineColor;
			newLineRenderer.useWorldSpace = false;

			for (int i = 0; i < linePoints.Count; i++)
			{
				newLineRenderer.SetPosition(i, linePoints[i] + offset);
			}
		}
	}
}