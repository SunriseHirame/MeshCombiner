using System.Collections.Generic;
using UnityEngine;

namespace Hirame.MeshCombiner
{
    public class MeshMerger : MonoBehaviour
    {
        [SerializeField] private CombineOptions options;
        
        [Header ("Sources")]
        [SerializeField] private GameObject[] rootGameObjects;

        [Header ("Combined")]
        [SerializeField] private List<GameObject> combinedMeshes;

        [ContextMenu ("Combine")]
        public void Combine ()
        {
            if (rootGameObjects == null || rootGameObjects.Length == 0)
            {    
                Debug.Log ("No Source meshes!");
                return;
            }
            
            var map = CreateCombineMap ();

            DestroyOldCombineMeshes ();
            
            foreach (var kvp in map)
            {
                CreateCombinedMesh (kvp.Key, kvp.Value);
            }

            foreach (var root in rootGameObjects)
            {
                if (root)
                    root.SetActive (false);
            }
        }

        public void Split ()
        {
            foreach (var combined in combinedMeshes)
            {
                if (combined)
                    combined.SetActive (false);
            }

            foreach (var root in rootGameObjects)
            {
                if (root)
                    root.SetActive (true);
            }
        }

        private Dictionary<Material, List<CombineInstance>> CreateCombineMap ()
        {
            var map = new Dictionary<Material, List<CombineInstance>> ();
            
            foreach (var root in rootGameObjects)
            {
                if (root == false)
                    continue;
                
                var filters = root.GetComponentsInChildren<MeshFilter> ();
                var renderers = root.GetComponentsInChildren<MeshRenderer> ();

                for (var i = 0; i < filters.Length; i++)
                {
                    AddToCombinedMesh (filters[i], renderers[i], map);
                }
            }

            return map;
        }

        private void AddToCombinedMesh (
            MeshFilter filter, MeshRenderer renderer, Dictionary<Material, List<CombineInstance>> map)
        {
            var sharedMesh = filter.sharedMesh;
            var subMeshCount = sharedMesh.subMeshCount;
            var materialCount = renderer.sharedMaterials.Length;

            if (subMeshCount > materialCount)
            {
                throw new System.Exception ("A Mesh has more sub meshes than renderer materials.");
            }

            var combineMatrix = filter.transform.localToWorldMatrix;

            for (var i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                var sharedMaterial = renderer.sharedMaterials[i];
                if (options.HasFlagNonAlloc (CombineOptions.IgnoreInstancedMaterials) 
                    && sharedMaterial.enableInstancing)
                {
                    continue;
                }   
                
                if (!map.TryGetValue (sharedMaterial, out var combineList))
                {
                    combineList = new List<CombineInstance> ();
                    print (sharedMaterial);
                    map.Add (sharedMaterial, combineList);
                }
                
                var combineInstance = new CombineInstance
                {
                    mesh = sharedMesh,
                    subMeshIndex = i,
                    transform = combineMatrix,
                };
                
                combineList.Add (combineInstance);
            }
        }

        private void CreateCombinedMesh (Material material, List<CombineInstance> sources)
        {
            if (combinedMeshes == null)
                combinedMeshes = new List<GameObject> ();

            var combinedName = $"CombinedMesh_{material.name}";
            var combinedMesh = new Mesh ();
            combinedMesh.name = combinedName;
            
            combinedMesh.CombineMeshes (sources.ToArray (), true, true);

            var combineObject = new GameObject (combinedName);
            combineObject.transform.SetParent (transform);
            combinedMeshes.Add (combineObject);

            var meshFilter = combineObject.AddComponent<MeshFilter> ();
            meshFilter.mesh = combinedMesh;

            var meshRenderer = combineObject.AddComponent<MeshRenderer> ();
            meshRenderer.sharedMaterial = material;
        }

        private void DestroyOldCombineMeshes ()
        {
            foreach (var combined in combinedMeshes)
            {
                if (Application.isPlaying)
                    Destroy (combined);
                else
                    DestroyImmediate (combined);
            }
            combinedMeshes.Clear ();
        }
    }
    
}
