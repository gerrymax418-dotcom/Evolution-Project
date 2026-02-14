namespace VerasStudios.Hopper
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor.TerrainTools;

    /// <summary>
    /// Editor window for displaying Hopper items in the Unity Editor
    /// </summary>
    [InitializeOnLoad]
    public class HopperWindow : EditorWindow
    {
        private static readonly ConcurrentBag<(string path, int line, string content, string type)> HopperItems = new ConcurrentBag<(string path, int line, string content, string type)>();

        private static bool _isScanning = false;

        private GUIStyle _leftAlignedButtonStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _wrappedCommentStyle;
        private GUIStyle _coloredBackgroundStyle;

        private Texture2D _coloredTexture;

        private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

        private Vector2 _scrollPosition;
        
        /// <summary>
        /// Static constructor to initialize teh auto-scan on load
        /// </summary>
        static HopperWindow()
        {
            EditorApplication.delayCall += AutoScanComplete;
        }

        /// <summary>
        /// Automatically scans for Hopper items when the editor loads
        /// </summary>
        private static void AutoScanComplete()
        {
            ScanForHoppersAsync();
        }

        [MenuItem("Tools/Veras/Code Hopper/Hopper Panel", priority = 1)]
        public static void ShowWindow()
        {
            GetWindow<HopperWindow>("Code Hopper");
        }

        private void OnEnable()
        {
            HopperManager.LoadHopperTypes();
            HopperManager.LoadExclusions();            
        }

        private void OnGUI()
        {
            GUILayout.Label("Hops", EditorStyles.boldLabel);

            Rect lastRect = GUILayoutUtility.GetLastRect();

            float buttonWidth = 18f;
            float buttonHeight = 18f;
            float padding = 5f;

            Rect cogRect = new Rect(
                position.width - buttonWidth - padding,
                lastRect.y + padding,
                buttonWidth,
                buttonHeight);

            Rect refreshRect = new Rect(
                cogRect.x - buttonWidth - padding,
                lastRect.y + padding,
                buttonWidth,
                buttonHeight);

            GUIContent cogIcon = new GUIContent(EditorGUIUtility.IconContent("_Popup", "Manage Hopper Settings"));
            GUIContent refreshIcon = new GUIContent(EditorGUIUtility.IconContent("d_Refresh", "Refresh List"));

            Color originalColor = GUI.color;
            if (lastRect.Contains(Event.current.mousePosition))
            {
                GUI.color = new Color(1f, 1f, 1f, .6f);
                Repaint();
            }

            if (GUI.Button(cogRect, cogIcon, EditorStyles.iconButton))
            {
                HopperManagerWindow.ShowWindow();
            }
            GUI.color = originalColor;

            if (refreshRect.Contains(Event.current.mousePosition))
            {
                GUI.color = new Color(1f, 1f, 1f, .6f);
                Repaint();
            }

            if (GUI.Button(refreshRect, refreshIcon, EditorStyles.iconButton))
            {
                ScanForHoppersAsync();
            }
            GUI.color = originalColor;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            var groupedHoppers = HopperItems
                .GroupBy(item => item.type)
                .OrderBy(group => HopUtils.GetHopperPriority(group.Key));

            foreach (var group in groupedHoppers)
            {
                if (!_foldoutStates.ContainsKey(group.Key))
                {
                    _foldoutStates[group.Key] = true;
                }
 
                _foldoutStates[group.Key] = EditorGUILayout.Foldout(_foldoutStates[group.Key], $"{group.Key} ({group.Count()})");

                if (_foldoutStates[group.Key])
                {
                    foreach (var item in group)
                    {
                        DrawHopperItem(item);
                    }
                }
            }

            if (_isScanning)
            {
                GUILayout.Label("Scanning...", EditorStyles.boldLabel);
            }

            if (HopperManager.HopperTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("No tags available for scanning. Add a new tag to start scanning for Hops!", MessageType.Info);
            }
            GUILayout.EndScrollView();

            if (HopperManagerWindow.IsOpen)
            {
                Repaint();
            }
        }

        /// <summary>
        /// Asynchronously scans for Hopper items in the project
        /// </summary>
        public static async void ScanForHoppersAsync()
        {
            if (_isScanning) return;

            _isScanning = true;
            HopperItems.Clear();
            HopperManager.LoadHopperTypes();

            if (HopperManager.HopperTypes.Count == 0)
            {
                _isScanning = false;
                return;
            }

            string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            ConcurrentBag<(string path, int line, string content, string type)> localHopperItems = new ConcurrentBag<(string, int, string, string)>();

            string hopperPattern = HopUtils.BuildDynamicHopperPatterns();

            await Task.Run(() =>
            {
                Parallel.ForEach(files, file =>
                {
                    string relativePath = HopUtils.NormalizePath("Assets" + file.Substring(Application.dataPath.Length));

                    if (HopperManager.IsPathExcluded(relativePath)) return;

                    string[] lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        string unquotedLine = HopUtils.ExtractUnqotedSegments(line);

                        Match hopperMatch = Regex.Match(unquotedLine, hopperPattern, RegexOptions.IgnoreCase);
                        if (hopperMatch.Success && hopperMatch.Groups.Count > 1)
                        {
                            string fullMatch = hopperMatch.Value.Trim();
                            string matchedType = hopperMatch.Groups[1].Value;

                            localHopperItems.Add((relativePath, i + 1, fullMatch, matchedType.ToUpper()));
                        }
                    }
                });
            });

            foreach (var item in localHopperItems)
            {
                HopperItems.Add(item);
            }

            _isScanning = false;

            if (HasOpenInstances<HopperWindow>())
            {
                GetWindow<HopperWindow>().Repaint();
            }
        }

        /// <summary>
        /// Draws a single Hopper item in the window
        /// </summary>
        /// <param name="item">The Hopper item to draw</param>
        private void DrawHopperItem((string path, int line, string content, string type) item)
        {
            Color color = HopperManager.HopperTypes.ContainsKey(item.type)
                ? HopperManager.HopperTypes[item.type]
                : Color.white;

            if (HopperManager.UseMultiLineLayout)
            {
                PaintMultilineItem(item, color);
            }
            else
            {
                PaintSingleLineItem(item, color);
            }
        }

        /// <summary>
        /// Paints a hopper item in a single-line format
        /// </summary>
        /// <param name="item">The hopper item to paint</param>
        /// <param name="color">The color associated with the hopper type</param>
        private void PaintSingleLineItem((string path, int line, string content, string type) item, Color color)
        {
            _leftAlignedButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = HopUtils.GetReadableTextColor(color) },
                hover = { textColor = HopUtils.GetReadableTextColor(color) },
            };

            GUI.backgroundColor = color;

            string comment = Regex.Replace(item.content, $@"^\s*//\s*{item.type}\s*", "", RegexOptions.IgnoreCase)
                               .Trim();

            if (GUILayout.Button($"{comment} - {Path.GetFileName(item.path)} (Line {item.line})", _leftAlignedButtonStyle))
            {
                HopUtils.OpenScriptAtLine(item.path, item.line);
            }
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Paints a Hopper item in a multi-line format
        /// </summary>
        /// <param name="item">The hopper item to paint</param>
        /// <param name="color">The color associated with the hopper type</param>
        private void PaintMultilineItem((string path, int line, string content, string type) item, Color color)
        {
            _boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperLeft,
            };

            _wrappedCommentStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
            };
            
            _coloredBackgroundStyle = new GUIStyle
            {
                padding = new RectOffset(8, 8, 8, 8),
                border = new RectOffset(4, 4, 4, 4),
            };

            _coloredTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _coloredTexture.wrapMode = TextureWrapMode.Repeat;

            string comment = Regex.Replace(item.content, $@"^\s*//\s*{item.type}\s*", "", RegexOptions.IgnoreCase).Trim();

            float viewWidth = EditorGUIUtility.currentViewWidth - 32;
            float commentHeight = _wrappedCommentStyle.CalcHeight(new GUIContent(comment), viewWidth);
            float totalHeight = EditorGUIUtility.singleLineHeight + commentHeight + 16;
            Rect blockRect = GUILayoutUtility.GetRect(1, totalHeight, GUILayout.ExpandWidth(true));

            bool isHovering = blockRect.Contains(Event.current.mousePosition);

            Color finalColor = isHovering
                ? (HopUtils.GetLuminance(color) > 0.7f ? color * 0.9f : color * 1.1f)
                : color;

            Color readableColor = HopUtils.GetReadableTextColor(finalColor);
            
            _boldLabelStyle.normal.textColor = readableColor;
            _boldLabelStyle.hover.textColor = readableColor;
            
            _wrappedCommentStyle.normal.textColor = readableColor;
            _wrappedCommentStyle.hover.textColor = readableColor;
            
            _coloredTexture.SetPixel(0, 0, finalColor);
            _coloredTexture.Apply();
            _coloredBackgroundStyle.normal.background = _coloredTexture;

            GUI.Box(blockRect, GUIContent.none, _coloredBackgroundStyle);

            Rect labelRect = new Rect(blockRect.x + 8, blockRect.y + 8, blockRect.width - 16, EditorGUIUtility.singleLineHeight);
            GUI.Label(labelRect, $"{Path.GetFileName(item.path)} ({item.line})", _boldLabelStyle);

            Rect commentRect = new Rect(blockRect.x + 8, blockRect.y + 28, blockRect.width - 16, commentHeight);
            GUI.contentColor = HopUtils.GetReadableTextColor(finalColor);
            GUI.Label(commentRect, comment, _wrappedCommentStyle);
            GUI.contentColor = Color.white;

            if (GUI.Button(blockRect, GUIContent.none, GUIStyle.none))
            {
                HopUtils.OpenScriptAtLine(item.path, item.line);
            }

            GUILayout.Space(5);
        }   
    }
}