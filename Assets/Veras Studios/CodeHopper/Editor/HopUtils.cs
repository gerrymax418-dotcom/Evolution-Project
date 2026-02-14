using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace VerasStudios.Hopper
{
    using UnityEngine;
    using UnityEditor;
    using System.Text;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility class for various helper methods used in Code Hopper
    /// </summary>
    public static class HopUtils
    {
        /// <summary>
        /// Deletes all saved editor preferences related to Code Hopper
        /// </summary>
        [MenuItem("Tools/Veras/Code Hopper/Reset Settings", priority = 13)]
        public static void ResetSettings()
        {
            EditorPrefs.DeleteKey(HopperManager.HOPPER_TYPES);
            EditorPrefs.DeleteKey(HopperManager.EXCLUDE_FOLDER_KEY);
            EditorPrefs.DeleteKey(HopperManager.EXCLUDE_FILE_KEY);
            EditorPrefs.DeleteKey(HopperManager.LAYOUT_KEY);
            EditorPrefs.DeleteKey(HopperManager.LOADED_KEY);

            HopperManager.LoadHopperTypes();
        }

        /// <summary>
        /// Extracts segments of a line that are not within quotes
        /// </summary>
        /// <param name="line">The line of text to process</param>
        /// <returns>A string containing only the unquoted segments of the line</returns>
        public static string ExtractUnqotedSegments(string line)
        {
            if (string.IsNullOrEmpty(line)) return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char character = line[i];

                if (character == '"')
                {
                    bool isEscaped = (i > 0 && line[i - 1] == '\\');

                    if (!isEscaped)
                    {
                        inQuotes = !inQuotes;
                    }
                }

                if (!inQuotes)
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Builds a dynamic regex pattern for matching Hopper items based on defined types
        /// </summary>
        /// <returns>A regex pattern string for matching Hopper items</returns>
        public static string BuildDynamicHopperPatterns()
        {
            var escapedTodos = HopperManager.HopperTypes.Keys
                .Select(Regex.Escape)
                .ToList();

            return $@"//\s*({string.Join("|", escapedTodos)}):?.*";
        }

        /// <summary>
        /// Opens a script at a specific line from the Unity Editor
        /// </summary>
        /// <param name="path">The path to the script file</param>
        /// <param name="line">The line number to open</param>
        public static void OpenScriptAtLine(string path, int line)
        {
            Object script = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (script != null)
            {
                AssetDatabase.OpenAsset(script, line);
            }
            else
            {
                Debug.LogWarning($"Could not find script at path: {path}");
            }
        }

        /// <summary>
        /// Gets the priority of a Hopper type for sorting purposes
        /// </summary>
        /// <param name="type">The hopper type to evaluate</param>
        /// <returns>The priority index of the hopper type</returns>
        public static int GetHopperPriority(string type)
        {
            var keys = HopperManager.HopperTypes.Keys.ToList();
            int index = keys.IndexOf(type);
            return index != -1 ? index : int.MaxValue;
        }

        /// <summary>
        /// Determines a readable text color based on the luminance of the given color
        /// </summary>
        /// <param name="color">The background color to evaluate</param>
        /// <returns>Returns black if the color is light; otherwise returns white</returns>
        public static Color GetReadableTextColor(Color color) => GetLuminance(color) > .5f ? Color.black : Color.white;

        /// <summary>
        /// Calculates the luminance of a given color
        /// </summary>
        /// <param name="color">The color to calculate luminance for</param>
        /// <returns>The luminance value of the color</returns>
        public static float GetLuminance(Color color) => 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;

        /// <summary>
        /// Normalizes a file path by replacing backslashes with forward slashes
        /// </summary>
        /// <param name="path">The file path to normalize</param>
        /// <returns>The normalized file path</returns>
        public static string NormalizePath(string path) => path.Replace("\\", "/");
    }

    public enum SaveLocation
    {
        UnityWide,
        OnlyThisProject
    }

    [Serializable]
    public class TagList
    {
        public List<Tag> tags = new List<Tag>();
    }
    
    [Serializable]
    public class Tag : IEquatable<Tag>
    {
        public string HopperTag;
        public Color TagColor;
        public int HopperIndex = -1;

        public bool Equals(Tag other)
        {
            return HopperTag == other.HopperTag;
        }

        public override bool Equals(object obj)
        {
            return obj is Tag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (HopperTag != null ? HopperTag.GetHashCode() : 0);
        }
    }
}
