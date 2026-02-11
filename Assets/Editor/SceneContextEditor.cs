// using UnityEditor;
// using UnityEngine;
// using Newtonsoft.Json.Linq;
// using System.Collections.Generic;
// using System.Linq;

// [CustomEditor(typeof(SceneLocalizationContext))]
// public class SceneContextEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         SceneLocalizationContext script = (SceneLocalizationContext)target;

//         script.viJson = (TextAsset)EditorGUILayout.ObjectField("Vietnamese JSON", script.viJson, typeof(TextAsset), false);
//         script.enJson = (TextAsset)EditorGUILayout.ObjectField("English JSON", script.enJson, typeof(TextAsset), false);

//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("Scene Scopes", EditorStyles.boldLabel);

//         if (script.viJson == null)
//         {
//             EditorGUILayout.HelpBox("Please assign a JSON file to see available keys.", MessageType.Info);
//             return;
//         }

//         JObject root;
//         try {
//             root = JObject.Parse(script.viJson.text);
//         } catch {
//             EditorGUILayout.HelpBox("Invalid JSON format!", MessageType.Error);
//             return;
//         }

//         if (script.sceneScopeKeys == null) script.sceneScopeKeys = new string[0];

//         for (int i = 0; i < script.sceneScopeKeys.Length; i++)
//         {
//             EditorGUILayout.BeginVertical(GUI.skin.box);
//             EditorGUILayout.BeginHorizontal();
            
//             EditorGUILayout.LabelField($"Scope {i + 1}: {script.sceneScopeKeys[i]}", EditorStyles.miniBoldLabel);
            
//             if (GUILayout.Button("X", GUILayout.Width(20)))
//             {
//                 var list = script.sceneScopeKeys.ToList();
//                 list.RemoveAt(i);
//                 script.sceneScopeKeys = list.ToArray();
//                 break;
//             }
//             EditorGUILayout.EndHorizontal();

//             DrawKeySelector(root, i, script);
            
//             EditorGUILayout.EndVertical();
//         }

//         if (GUILayout.Button("Add New Scope Path"))
//         {
//             var list = script.sceneScopeKeys.ToList();
//             list.Add("");
//             script.sceneScopeKeys = list.ToArray();
//         }

//         if (GUI.changed) EditorUtility.SetDirty(script);
//     }

//     private void DrawKeySelector(JObject root, int index, SceneLocalizationContext script)
//     {
//         string currentFullPath = script.sceneScopeKeys[index];
//         string[] parts = currentFullPath.Split('.');
        
//         JToken currentToken = root;
//         string builtPath = "";

//         for (int level = 0; level <= parts.Length; level++)
//         {
//             if (currentToken is JObject obj)
//             {
//                 List<string> options = obj.Properties().Select(p => p.Name).ToList();
//                 options.Insert(0, level < parts.Length ? "- Select -" : "- End Here -");

//                 int selectedIdx = 0;
//                 if (level < parts.Length && options.Contains(parts[level]))
//                 {
//                     selectedIdx = options.IndexOf(parts[level]);
//                 }

//                 int newIdx = EditorGUILayout.Popup($"Level {level}", selectedIdx, options.ToArray());

//                 if (newIdx > 0)
//                 {
//                     string selectedName = options[newIdx];
//                     if (selectedName == "- End Here -")
//                     {
//                         script.sceneScopeKeys[index] = builtPath.TrimEnd('.');
//                         return;
//                     }

//                     builtPath += selectedName + ".";
//                     currentToken = obj[selectedName];
                    
//                     if (level >= parts.Length || parts[level] != selectedName)
//                     {
//                         script.sceneScopeKeys[index] = builtPath.TrimEnd('.');
//                         return;
//                     }
//                 }
//                 else if (newIdx == 0 && level < parts.Length)
//                 {
//                     script.sceneScopeKeys[index] = builtPath.TrimEnd('.');
//                     return;
//                 }
//                 else { return; }
//             }
//             else { break; }
//         }
//     }
// }
