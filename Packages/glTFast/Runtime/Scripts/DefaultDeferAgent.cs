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

using System;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// To be added to a GameObject along a default <see cref="IDeferAgent"/>.
    /// Will (un)register it as GltfImport's default when it's enabled or disabled.
    /// </summary>
    [RequireComponent(typeof(IDeferAgent))]
    [DefaultExecutionOrder(-1)]
    class DefaultDeferAgent : MonoBehaviour
    {

        void OnEnable()
        {
            var deferAgent = GetComponent<IDeferAgent>();
            if (deferAgent != null)
            {
                GltfImport.SetDefaultDeferAgent(deferAgent);
            }
        }

        void OnDisable()
        {
            var deferAgent = GetComponent<IDeferAgent>();
            if (deferAgent != null)
            {
                GltfImport.UnsetDefaultDeferAgent(deferAgent);
            }
        }
    }
}
