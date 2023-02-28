using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Globalization;
using Netherlands3D.Events;

namespace Netherlands3D.TileSystem
{
    public class SelectSubObjects : MonoBehaviour
    {
        private Ray ray;
        private string lastSelectedID = "";

        [Header("Sub object selection:")]
        [SerializeField] private string removeFromID = "NL.IMBAG.Pand.";
        [SerializeField] private string brotliExtention = ".br";

        [SerializeField]
        private LayerMask clickCheckLayerMask;
        private BinaryMeshLayer containerLayer;

        private const string emptyID = "null";
        private const int maxRayPiercing = 10;
        private const int renderFrameEvery = 5;

        private RaycastHit lastRaycastHit;

        [SerializeField] private int submeshIndex = 0;

        [SerializeField] private bool applyHideOnReload = false;
        [SerializeField] private bool applyOnExistingTiles = true;

        [SerializeField]
        private float maxSelectDistance = 8000;

        [SerializeField]
        [ColorUsage(true, true)]
        private Color selectionVertexColor;

        private List<string> selectedIDs;
        private List<string> hiddenIDs;
        private List<string> tilesWithInteractedSubObjects;

        private bool doingMultiselect = false;
        private bool pauseSelectHighlighting = false;

        [Header("Invoke events")]
        [SerializeField]
        private BoolEvent clickedOnObject;
        [SerializeField]
        private StringListEvent selectedIdsOnClick;

        [Header("Listen to events")]
        [SerializeField]
        private Vector3Event clickedOnPosition;
        [SerializeField]
        private BoolEvent onColoringSubobjects;

        public List<string> SelectedIDs { get => selectedIDs; set => selectedIDs = value; }
        public List<string> HiddenIDs { get => hiddenIDs; set => hiddenIDs = value; }
        public List<string> TilesWithInteractedSubObjects { get => tilesWithInteractedSubObjects; set => tilesWithInteractedSubObjects = value; }

        private void Awake()
        {
            //Global settings for subobject data
            SubObjects.removeFromID = removeFromID;
            SubObjects.brotliExtention = brotliExtention;

            SelectedIDs = new List<string>();
            HiddenIDs = new List<string>();
            TilesWithInteractedSubObjects = new List<string>();

            if (onColoringSubobjects)
                onColoringSubobjects.AddListenerStarted(DisableWhileColoring);

            if (clickedOnPosition)
                clickedOnPosition.AddListenerStarted(ShootRayAtPosition);

            containerLayer = gameObject.GetComponent<BinaryMeshLayer>();
        }

        private void OnTransformChildrenChanged()
        {
            if (applyHideOnReload && hiddenIDs.Count > 0)
            {
                UpdateHiddenListToChildren(applyOnExistingTiles);
            }
        }

        public void UpdateHiddenListToChildren(bool applyToExistingSubObjects = false)
        {
            foreach (Transform child in transform)
            {
                if (!TilesWithInteractedSubObjects.Contains(child.name)) continue;

                SubObjects subObjects = child.gameObject.GetComponent<SubObjects>();
                if (!subObjects)
                {
                    if (child.gameObject.GetComponent<MeshFilter>())
                    {
                        subObjects = child.gameObject.AddComponent<SubObjects>();
                        subObjects.HideWithIDs(hiddenIDs);
                    }
                }
                else if (applyToExistingSubObjects)
                {
                    subObjects.HideWithIDs(hiddenIDs);
                }
            }
        }

        private void ShootRayAtPosition(Vector3 screenPosition)
        {
            var ray = Camera.main.ScreenPointToRay(screenPosition);
            SelectWithInputs(ray, false, false);
        }

        private void DisableWhileColoring(bool coloring)
        {
            pauseSelectHighlighting = coloring;
        }

        public void SelectWithInputs(Ray inputRay, bool multiSelect, bool secondary = false)
        {
            if (pauseSelectHighlighting)
                return;

            ray = inputRay;
            doingMultiselect = multiSelect;

            if (secondary)
            {
                SecondarySelect();
                return;
            }
            Select();
        }

        private void Select()
        {
            if (!enabled) return;
            FindSelectedID();
        }

        private void SecondarySelect()
        {
            //On a secondary click, only select if we did not make a multisselection yet.
            if (SelectedIDs.Count < 2)
            {
                Select();
            }
            else
            {
                //Simply retrigger the selection list with the new values
                HighlightObjectsWithIDs(SelectedIDs);
            }
        }

        public void Deselect()
        {
            if (!enabled) return;
            ClearSelection();
        }

        /// <summary>
        /// Select a mesh ID underneath the pointer
        /// </summary>
        private void FindSelectedID()
        {
            //Clear selected ids if we are not adding to a multiselection
            if (!doingMultiselect) SelectedIDs.Clear();

            //Try to find a selected mesh ID and highlight it
            StartCoroutine(FindSelectedSubObjectID(ray, (id) =>
            {
                HighlightSelectedID(id);
            }));
        }

        /// <summary>
        /// Add a single object to highlight selection. If we clicked an empty ID, clear the selection if we are not in multiselect
        /// </summary>
        /// <param name="id">The object ID</param>
        public void HighlightSelectedID(string id)
        {
            if (!enabled) return;

            if (id == emptyID && !doingMultiselect)
            {
                ClearSelection();
                clickedOnObject.InvokeStarted(false);
            }
            else
            {
                List<string> singleIdList = new List<string>();
                //Allow clicking a single object multiple times to move them in and out of our selection
                if (doingMultiselect && SelectedIDs.Contains(id))
                {
                    SelectedIDs.Remove(id);
                }
                else
                {
                    var filteredID = id;
                    if (removeFromID.Length > 0) filteredID = filteredID.Replace(removeFromID, "");

                    Debug.Log($"Select ID:{filteredID}");
                    singleIdList.Add(filteredID);
                }
                HighlightObjectsWithIDs(singleIdList);
                clickedOnObject.InvokeStarted(true);
            }
        }

        /// <summary>
        /// Removes an object with this specific ID from the selected list, and update the highlights
        /// </summary>
        /// <param name="id">The unique ID of this item</param>
        public void DeselectSpecificID(string id)
        {
            if (!enabled) return;

            if (SelectedIDs.Contains(id))
            {
                SelectedIDs.Remove(id);
                HighlightObjectsWithIDs(SelectedIDs);
            }
        }

        /// <summary>
        /// Add list of ID's to our selected objects list
        /// </summary>
        /// <param name="ids">List of IDs to add to our selection</param>
        private void HighlightObjectsWithIDs(List<string> ids = null)
        {
            if (!enabled) return;

            if (ids != null) SelectedIDs.AddRange(ids);
            SelectedIDs = SelectedIDs.Distinct().ToList(); //Filter out any possible duplicates

            lastSelectedID = (SelectedIDs.Count > 0) ? SelectedIDs.Last() : emptyID;

            HighlightSelectedWithColor(selectionVertexColor);

            selectedIdsOnClick.InvokeStarted(SelectedIDs);
        }

        /// <summary>
        /// Highlight the current selected subobject(s) with a color
        /// </summary>
        /// <param name="highlightColor">Color for object(s)</param>
        public void HighlightSelectedWithColor(Color highlightColor)
        {
            //Apply highlight to all selected objects
            var subObjectContainers = GetComponentsInChildren<SubObjects>();
            foreach (var subObjectContainer in subObjectContainers)
            {
                subObjectContainer.ColorWithIDs(SelectedIDs, highlightColor);
            }
        }

        /// <summary>
        /// Highlight all subobjects with a color
        /// </summary>
        /// <param name="highlightColor">Color for objects</param>
        public void HighlightAllWithColor(Color highlightColor)
        {
            //Apply highlight to all objects
            var subObjectContainers = GetComponentsInChildren<SubObjects>();
            foreach (var subObjectContainer in subObjectContainers)
            {
                subObjectContainer.ColorAll(highlightColor);
            }
        }

        /// <summary>
        /// Hide the selected subobjects by changing the vertex colors alpha
        /// </summary>
        public void HideSelectedSubObjects()
        {
            //Apply highlight to all objects
            var subObjectContainers = GetComponentsInChildren<SubObjects>();
            foreach (var subObjectContainer in subObjectContainers)
            {
                subObjectContainer.HideWithIDs(HiddenIDs);
            }
        }

        /// <summary>
        /// Clear all colors, and with that the alpha 0 for the vertex colors
        /// </summary>
        public void UnhideAllSubObjects()
        {
            //Apply highlight to all objects
            var subObjectContainers = GetComponentsInChildren<SubObjects>();
            foreach (var subObjectContainer in subObjectContainers)
            {
                subObjectContainer.ResetColors();
            }

            TilesWithInteractedSubObjects.Clear();
        }

        /// <summary>
        /// Clear our list of selected objects, and update the highlights
        /// </summary>
        public void ClearSelection()
        {
            if (!enabled) return;

            if (SelectedIDs.Count != 0)
            {
                lastSelectedID = emptyID;
                SelectedIDs.Clear();
            }

            //Remove highlights by highlighting our empty list
            HighlightObjectsWithIDs();
        }

        /// <summary>
        /// Hides all objects that matches the list of ID's, and remove them from our selection list.
        /// </summary>
        public void HideSelectedIDs()
        {
            if (!enabled) return;

            if (SelectedIDs.Count > 0)
            {
                //Adds selected ID's to our hidding objects of our layer
                HiddenIDs.AddRange(SelectedIDs);

                HideSelectedSubObjects();
                SelectedIDs.Clear();
            }

            //If we hide something, make sure our context menu is reset to default again.
            //ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
        }

        /// <summary>
        /// Shows all hidden objects by clearing our selection and hiding that empty list
        /// </summary>
        public void UnhideAll()
        {
            if (!enabled) return;

            lastSelectedID = emptyID;
            HiddenIDs.Clear();
            SelectedIDs.Clear();
            UnhideAllSubObjects();
        }

        IEnumerator FindSelectedSubObjectID(Ray ray, System.Action<string> callback)
        {
            //Check area that we clicked, and add the (heavy) mesh collider there
            Plane worldPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
            worldPlane.Raycast(ray, out float distance);
            var samplePoint = ray.GetPoint(Mathf.Min(maxSelectDistance, distance));
            containerLayer.AddMeshColliders(samplePoint);

            yield return new WaitForEndOfFrame();

            //Now fire a raycast towards our meshcolliders to see what face we hit 
            if (Physics.Raycast(ray, out lastRaycastHit, 10000, clickCheckLayerMask.value) == false)
            {
                callback(emptyID);
                yield break;
            }

            //Get the mesh we selected and find the triangle vert index we hit
            var tileObject = lastRaycastHit.collider.gameObject;

            Mesh mesh = tileObject.GetComponent<MeshFilter>().sharedMesh;
            int triangleVertexIndex = lastRaycastHit.triangleIndex * 3;
            var vertexIndex = mesh.GetIndices(submeshIndex)[triangleVertexIndex];
            var tileContainer = lastRaycastHit.collider.gameObject;

            if (mesh.colors.Length > 0)
            {
                var hitAlpha = mesh.colors[vertexIndex].a;
                int pierces = maxRayPiercing;
                //If this vert has a transparant color, pierce through to find visible faces
                while (hitAlpha == 0 && pierces > 0)
                {
                    pierces--;
                    if (pierces % renderFrameEvery == 0)
                        yield return new WaitForEndOfFrame();

                    Vector3 deeperHitPoint = lastRaycastHit.point + (ray.direction * 0.01f);
                    ray = new Ray(deeperHitPoint, ray.direction);
                    if (Physics.Raycast(ray, out lastRaycastHit, 10000, clickCheckLayerMask.value))
                    {
                        triangleVertexIndex = lastRaycastHit.triangleIndex * 3;
                        vertexIndex = mesh.GetIndices(submeshIndex)[triangleVertexIndex];
                        hitAlpha = mesh.colors[vertexIndex].a;
                    }
                }

                //No visible geometry found under click? Then stop and break out.
                if (hitAlpha == 0)
                    yield break;
            }
            //Fetch this tile's subject data (if we didnt already)
            SubObjects subObjects = tileContainer.GetComponent<SubObjects>();
            if (!subObjects) subObjects = tileContainer.AddComponent<SubObjects>();

            //Pass down the ray we used to click to get the ID we clicked
            subObjects.Select(vertexIndex, callback);

            //Add this mesh name to the list of tilenames that contain selection/hidden files
            if (!TilesWithInteractedSubObjects.Contains(tileObject.name))
            {
                TilesWithInteractedSubObjects.Add(tileObject.name);
            }
        }
    }
}