/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Netherlands3D.Rendering
{
    public class DynamicShadowDistance : MonoBehaviour
    {
        UniversalRenderPipelineAsset universalRenderPipelineAsset;

        [SerializeField]
        private float range = 6.5f;

        [SerializeField]
        private float minShadowDistance = 100;
		[SerializeField]
		private float maxShadowDistance = 4000;

		[Tooltip("Will default to main camera transform")]
		[SerializeField]
		private Transform referenceTransform;

		private void Awake()
		{
			if (!referenceTransform) referenceTransform = Camera.main.transform;
		}

		void Update()
		{
			SetShadowDistanceOnCurrentRenderPipeline();
		}

		private void OnDisable()
		{
			SetShadowDistanceOnCurrentRenderPipeline();
		}

		/// <summary>
		/// Calculates shadow distance based on transform height.
		/// This allows drawing shadows even when the camera is far above the world, and still have sharp shadows at close range.
		/// </summary>
		private void SetShadowDistanceOnCurrentRenderPipeline()
		{
			if (!referenceTransform) return;
			var dynamicShadowDistance = Mathf.Min(Mathf.Max(referenceTransform.position.y * range, minShadowDistance), maxShadowDistance);
			ApplyMaxShadowDistance(dynamicShadowDistance);
		}
		
		/// <summary>
		/// Sets max shadow distance in quality settings, and active Render Pipeline asset if it is not null
		/// </summary>
		/// <param name="dynamicShadowDistance">Maximum shadow distance</param>
		private void ApplyMaxShadowDistance(float dynamicShadowDistance)
		{
			QualitySettings.shadowDistance = dynamicShadowDistance;

			if (!QualitySettings.renderPipeline)
			{
				return;
			}

			if (universalRenderPipelineAsset != QualitySettings.renderPipeline)
				universalRenderPipelineAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;

			universalRenderPipelineAsset.shadowDistance = dynamicShadowDistance;
		}
	}
}