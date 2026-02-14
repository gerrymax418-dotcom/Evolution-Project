using System.IO;

namespace VerasStudios.Hopper
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Editor window for managing Hopper types and exclusions in the Unity Editor
    /// </summary>
    public class HopperManagerWindow : EditorWindow
    {
        /// <summary>
        /// Indicates whether the Hopper Manager window is currently open.
        /// </summary>
        public static bool IsOpen { get; private set; }

        private ReorderableList _reorderableList;
        private List<string> _hopperKeys;

        private Vector2 _scrollPosition;
        private Color _newHopperColor = Color.cyan;
        
        private string _newHopperType = "HOP";
        private SaveLocation _saveLocation = SaveLocation.UnityWide;

        /// <summary>
        /// Opens the Hopper Manager window from the Unity Editor menu
        /// </summary>
        [MenuItem("Tools/Veras/Code Hopper/Manager", priority = 12)]
        public static void ShowWindow()
        {
            GetWindow<HopperManagerWindow>("Hopper Manager");
        }

        private void OnEnable()
        {
            IsOpen = true;
            HopperManager.LoadHopperTypes();
            _hopperKeys = HopperManager.HopperTypes.Keys.ToList();

            SetupReorderableList();
        }

        private void OnDisable()
        {
            IsOpen = false;
            Dictionary<string, Color> hopsToSave = HopperManager.HopperTypes;
            foreach (string entry in HopperManager.GetProjectHopTags())
            {
                HopperManager.SaveProjectHopperColor(entry, HopperManager.HopperTypes[entry]);
                hopsToSave.Remove(entry);
            }
            HopperManager.SaveUnityHopperTypes(hopsToSave);
            HopperManager.SaveExclusions();
            HopperManager.LoadHopperTypes();
        }

        private void OnGUI()
        {
            GUILayout.Label("Hopper Manager", EditorStyles.boldLabel);

            GUILayout.Space(10);

            bool newLayoutChoice = EditorGUILayout.Toggle("Use Multi-line Layout", HopperManager.UseMultiLineLayout);

            if (newLayoutChoice != HopperManager.UseMultiLineLayout)
            {
                HopperManager.UseMultiLineLayout = newLayoutChoice;
            }

            GUILayout.Space(10);

            _reorderableList.DoLayoutList();
            GUILayout.Space(10);
            GUILayout.Label("Add New Hopper Type", EditorStyles.boldLabel);

            _newHopperType = EditorGUILayout.TextField("Tag:", _newHopperType);
            _newHopperColor = EditorGUILayout.ColorField("Color:", _newHopperColor);
            _saveLocation = (SaveLocation)EditorGUILayout.EnumPopup("Save:", _saveLocation);

            if (GUILayout.Button("Add Hopper Type"))
            {
                if (_saveLocation == SaveLocation.UnityWide)
                {
                    HopperManager.AddUnityHopperType(_newHopperType, _newHopperColor);
                }
                else
                {
                    HopperManager.AddProjectHopperType(_newHopperType, _newHopperColor);
                }
                
                _hopperKeys = HopperManager.HopperTypes.Keys.ToList();
                SetupReorderableList();
                HopperWindow.ScanForHoppersAsync();
            }

            GUILayout.Space(20);
            GUILayout.Label("Excluded Folders/Files from scan", EditorStyles.boldLabel);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach (var folder in HopperManager.GetExcludedFolders().ToArray())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(folder, EditorStyles.label);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    HopperManager.RemoveExcludedPath(folder);
                    HopperWindow.ScanForHoppersAsync();
                }
                GUILayout.EndHorizontal();
            }

            foreach (var file in HopperManager.GetExcludedFiles().ToArray())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(file, EditorStyles.label);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    HopperManager.RemoveExcludedPath(file);
                    HopperWindow.ScanForHoppersAsync();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            int buttonHeight = 50;

            if (GUILayout.Button("Exclude Folder", GUILayout.Height(buttonHeight)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder to Exclude", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    HopperManager.AddExcludedPath(relativePath, true);
                    HopperWindow.ScanForHoppersAsync();
                }
            }

            if (GUILayout.Button("Exclude File", GUILayout.Height(buttonHeight)))
            {
                string path = EditorUtility.OpenFilePanel("Select File to Exclude", Application.dataPath, "cs");
                if (!string.IsNullOrEmpty(path))
                {
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    HopperManager.AddExcludedPath(relativePath, false);
                    HopperWindow.ScanForHoppersAsync();
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Sets up the reorderable list for managing Hopper types
        /// </summary>
        private void SetupReorderableList()
        {
            _reorderableList = new ReorderableList(_hopperKeys, typeof(string), true, true, false, false)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Drag to Reorder");
                },

                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= _hopperKeys.Count) return;

                    var key = _hopperKeys[index];
                    rect.y += 2;

                    HopperManager.HopperTypes[key] = EditorGUI.ColorField(
                        new Rect(rect.x, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        key,
                        HopperManager.HopperTypes[key]);

                    if (GUI.Button(new Rect(rect.x + rect.width - 90, rect.y, 80, EditorGUIUtility.singleLineHeight), "Remove"))
                    {
                        HopperManager.RemoveHopperType(key);
                        _hopperKeys.RemoveAt(index);
                        SetupReorderableList();
                        HopperWindow.ScanForHoppersAsync();
                    }
                },

                onReorderCallback = (ReorderableList list) =>
                {
                    ReorderTodoTypes();
                }
            };
        }

        /// <summary>
        /// Reorders the Hopper types based on the current order in the reorderable list
        /// </summary>
        private void ReorderTodoTypes()
        {
            var reorderDict = new Dictionary<string, Color>();
            foreach (var key in _hopperKeys)
            {
                reorderDict[key] = HopperManager.HopperTypes[key];
            }
            
            HopperManager.HopperTypes = reorderDict;
            Dictionary<string, Color> hopsToSave = HopperManager.HopperTypes;
            foreach (string entry in HopperManager.GetProjectHopTags())
            {
                hopsToSave.Remove(entry);
            }
            HopperManager.SaveUnityHopperTypes(hopsToSave);
            
            string path = Application.dataPath + "/Settings/config.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                TagList configTags = JsonUtility.FromJson<TagList>(json);

                foreach (Tag tag in configTags.tags)
                {
                    int newIndex = _hopperKeys.IndexOf(tag.HopperTag.ToUpper());
                    
                    tag.HopperIndex = newIndex;
                }
                
                File.WriteAllText(path, JsonUtility.ToJson(configTags, prettyPrint: true));
            }
            
            HopperWindow.ScanForHoppersAsync();
        }
    }
}