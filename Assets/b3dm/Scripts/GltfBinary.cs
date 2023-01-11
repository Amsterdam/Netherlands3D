// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    using Loading;
    using Logging;
    using Materials;

    /// <summary>
    /// Base component for code-less loading of glTF files
    /// </summary>
    public class GltfBinary : GltfAssetBase
    {
      
        /// <summary>
        /// Scene to load (-1 loads glTFs default scene)
        /// </summary>
        protected int SceneId => sceneId;

  

        /// <inheritdoc cref="GLTFast.InstantiationSettings"/>
        public InstantiationSettings InstantiationSettings
        {
            get => instantiationSettings;
            set => instantiationSettings = value;
        }

        [SerializeField]
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        int sceneId = -1;


        [SerializeField]
        InstantiationSettings instantiationSettings;

        public GameObjectSceneInstance SceneInstance { get; protected set; }

        /// <inheritdoc />
        public virtual async Task<bool> LoadBinaryAndInstantiate(
            byte[] bytes,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await LoadFromBytes(bytes, downloadProvider, deferAgent, materialGenerator, logger);
            if (success)
            {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId >= 0)
                {
                    success = await InstantiateScene(sceneId, logger);
                }
                else
                {
                    success = await Instantiate(logger);
                }
            }
            return success;
        }

        public virtual async Task<bool> LoadFromBytes(
            byte[] bytes,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            Importer = new GltfImport(downloadProvider, deferAgent, materialGenerator, logger);
            return await Importer.Load(bytes);
        }

        /// <inheritdoc />
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
        {
            return new GameObjectInstantiator(Importer, transform, logger, instantiationSettings);
        }

        /// <inheritdoc />
        protected override void PostInstantiation(IInstantiator instantiator, bool success)
        {
            SceneInstance = (instantiator as GameObjectInstantiator)?.SceneInstance;
#if UNITY_ANIMATION
            if (SceneInstance != null) {
                if (playAutomatically) {
                    var legacyAnimation = SceneInstance.LegacyAnimation;
                    if (legacyAnimation != null) {
                        SceneInstance.LegacyAnimation.Play();
                    }
                }
            }
#endif
            base.PostInstantiation(instantiator, success);
        }

        /// <inheritdoc />
        public override void ClearScenes()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            SceneInstance = null;
        }
    }
}
