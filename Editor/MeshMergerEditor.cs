using UnityEditor;
using UnityEngine;

namespace Hirame.MeshCombiner.Editor
{
    [CustomEditor (typeof (MeshMerger))]
    public class MeshMergerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI ()
        {
            var meshMerger = target as MeshMerger;
            if (meshMerger == null)
                return;
            
            serializedObject.Update ();
            
            using (var scope = new EditorGUI.ChangeCheckScope ())
            {
                using (new EditorGUILayout.HorizontalScope ())
                {
                    if (GUILayout.Button ("Combine"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo (target, "Combined Meshes");
                        meshMerger.Combine ();
                    }
                    
                    if (GUILayout.Button ("Split"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo (target, "Split Meshes");
                        meshMerger.Split ();
                    }
                }
                
                EditorGUILayout.Space ();
                EditorGUILayout.LabelField ("Options", EditorStyles.boldLabel);

                var optionsProp = serializedObject.FindProperty ("options");
                var value = (CombineOptions) EditorGUILayout.EnumFlagsField ((CombineOptions) optionsProp.enumValueIndex);
                optionsProp.intValue = (int) value;
                
                DrawPropertiesExcluding (serializedObject, "m_Script", "options");
                
                if (scope.changed)
                    serializedObject.ApplyModifiedProperties ();
            }
        }
    }

}