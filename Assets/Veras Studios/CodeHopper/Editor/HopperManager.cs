using System;
using System.IO;

namespace VerasStudios.Hopper
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Manages the Hopper System, including loading and saving hopper types and exclusions.
    /// </summary>
    public static class HopperManager
    {
        /// <summary>
        /// Dictionary that holds each hopper type as the key and their color as the value
        /// </summary>
        public static Dictionary<string, Color> HopperTypes { get; set; } = new Dictionary<string, Color>();

        /// <summary>
        /// Gets or set whether or not to use the multi-line layout and saves it in editor
        /// prefs
        /// </summary>
        public static bool UseMultiLineLayout
        {
            get => EditorPrefs.GetBool(LAYOUT_KEY, true);
            set => EditorPrefs.SetBool(LAYOUT_KEY, value);
        }

        /// <summary>
        /// List of folders to be excluded from scanning
        /// </summary>
        public static List<string> ExcludedFolders = new List<string>();

        /// <summary>
        /// List of files to be excluded from scanning
        /// </summary>
        public static List<string> ExcludedFiles = new List<string>();

        public const string HOPPER_TYPES = "TodoTypes";
        public const string EXCLUDE_FOLDER_KEY = "ExcludedFolders";
        public const string EXCLUDE_FILE_KEY = "ExcludedFiles";
        public const string LAYOUT_KEY = "UseMultiLayout";
        public const string LOADED_KEY = "Loaded";

        /// <summary>
        /// Loads the hopper types from the editor preferences
        /// </summary>
        public static void LoadHopperTypes()
        {
            HopperTypes.Clear();
            string savedData = EditorPrefs.GetString(HOPPER_TYPES, "");

            if (!string.IsNullOrEmpty(savedData))
            {
                string[] entries = savedData.Split('|');

                foreach (string entry in entries)
                {
                    string[] parts = entry.Split(':');

                    if (parts.Length == 2)
                    {
                        string[] colorParts = parts[1].Split(",");
                        Color color = new Color(
                            float.Parse(colorParts[0], CultureInfo.InvariantCulture),
                            float.Parse(colorParts[1], CultureInfo.InvariantCulture),
                            float.Parse(colorParts[2], CultureInfo.InvariantCulture),
                            float.Parse(colorParts[3], CultureInfo.InvariantCulture));

                        HopperTypes.Add(parts[0], color);
                    }
                }

                string path = Application.dataPath + "/Settings/config.json";
                if (File.Exists(path))
                {
                    TagList tags = JsonUtility.FromJson<TagList>(File.ReadAllText(path));
                    List<KeyValuePair<string, Color>> configToInsert = new List<KeyValuePair<string, Color>>();
                    
                    foreach (Tag tag in tags.tags)
                    {
                        if (HopperTypes.ContainsKey(tag.HopperTag)) continue;
                        configToInsert.Add(new KeyValuePair<string, Color>(tag.HopperTag.ToUpper(), tag.TagColor));
                    }
                    
                    configToInsert.Sort((a, b) =>
                    {
                        int indexA = tags.tags.Find(t => t.HopperTag.ToUpper() == a.Key)?.HopperIndex ?? int.MaxValue;
                        int indexB = tags.tags.Find(t => t.HopperTag.ToUpper() == b.Key)?.HopperIndex ?? int.MaxValue;
                        return indexA.CompareTo(indexB);
                    });

                    Dictionary<string, Color> newDict = new Dictionary<string, Color>();
                    int currentIndex = 0;

                    foreach (KeyValuePair<string, Color> pair in HopperTypes)
                    {
                        while (configToInsert.Count > 0 &&
                               tags.tags.Find(t => t.HopperTag.ToUpper() == configToInsert[0].Key)?.HopperIndex ==
                               currentIndex)
                        {
                            var toInsert = configToInsert[0];
                            newDict[toInsert.Key] = toInsert.Value;
                            configToInsert.RemoveAt(0);
                        }
                        
                        newDict[pair.Key] = pair.Value;
                        currentIndex++;
                    }

                    foreach (var leftover in configToInsert)
                    {
                        newDict[leftover.Key] = leftover.Value;
                    }
                    
                    HopperTypes = newDict;
                }
            }
            // If this is the first time loading the asset, this will add
            // default hopper types for the example to work
            else if (!EditorPrefs.HasKey(LOADED_KEY))
            {
                EditorPrefs.SetBool(LOADED_KEY, true);

                AddUnityHopperType("BUG", new Color(0.4654087f, 0.1214746f, 0.1214746f));
                AddUnityHopperType("TODO", new Color(0.1918264f, 0.408805f, 0.1349827f));
                AddUnityHopperType("HOP", new Color(0.07396854f, 0.1482112f, 0.254902f));

                HopperWindow.ScanForHoppersAsync();
            }
        }

        /// <summary>
        /// Saves the current hopper types to the editor preferences
        /// </summary>
        public static void SaveUnityHopperTypes(Dictionary<string, Color> hopsToSave)
        {
            List<string> data = new List<string>();

            foreach (var type in hopsToSave)
            {
                string colorString = $"{type.Value.r},{type.Value.g},{type.Value.b},{type.Value.a}";
                data.Add($"{type.Key.ToUpper()}:{colorString}");
            }

            EditorPrefs.SetString(HOPPER_TYPES, string.Join("|", data));
        }

        /// <summary>
        /// Adds a new hopper type with the specified tag and color
        /// for every unity project
        /// </summary>
        /// <param name="tag">The tag for the hopper type</param>
        /// <param name="color">The color associated with the hopper type</param>
        public static void AddUnityHopperType(string tag, Color color)
        {
            if (!HopperTypes.ContainsKey(tag.ToUpper()))
            {
                HopperTypes.Add(tag.ToUpper(), color);
                SaveUnityHopperTypes(HopperTypes);
            }
        }

        /// <summary>
        /// Adds a new hopper type with the specified tag and color
        /// for this specific project
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        public static void AddProjectHopperType(string tag, Color color)
        {
            if (HopperTypes.ContainsKey(tag.ToUpper())) return;
            
            string path = Application.dataPath + "/Settings/config.json";
            TagList tags = new();
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                tags = JsonUtility.FromJson<TagList>(json);
            }

            Tag tagToAdd = new Tag { HopperTag = tag.ToUpper(), TagColor = color };

            if (tags.tags.Contains(tagToAdd)) return;
            
            tags.tags.Add(tagToAdd);
            File.WriteAllText(path, JsonUtility.ToJson(tags, prettyPrint: true));
            HopperTypes.Add(tag.ToUpper(), color);
            LoadHopperTypes();
        }

        public static void SaveProjectHopperColor(String tag, Color color)
        {
            string path = Application.dataPath + "/Settings/config.json";

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Cannot find config file");
            }
            
            string json = File.ReadAllText(path);
            TagList jsonTags = JsonUtility.FromJson<TagList>(json);

            Tag tagToAdd = new Tag { HopperTag = tag.ToUpper(), TagColor = color };
            
            if (jsonTags.tags.Contains(tagToAdd))
            {
                Tag oldTag = jsonTags.tags.Find(t => t.HopperTag.ToUpper() == tag.ToUpper());
                tagToAdd.HopperIndex = oldTag.HopperIndex;
                jsonTags.tags.Remove(tagToAdd);
                jsonTags.tags.Add(tagToAdd);
            }
            
            File.WriteAllText(path, JsonUtility.ToJson(jsonTags, prettyPrint: true));
        }

        /// <summary>
        /// Removes a hopper type with the specified tag
        /// </summary>
        /// <param name="tag">The tag of the hopper type to remove</param>
        public static void RemoveHopperType(string tag)
        {
            if (!HopperTypes.ContainsKey(tag.ToUpper())) return;
            
            string path = Application.dataPath + "/Settings/config.json";
            
            if (File.Exists(path))
            {
                TagList jsonTags = JsonUtility.FromJson<TagList>(File.ReadAllText(path));
                Tag tagToRemove = new Tag{HopperTag = tag.ToUpper()};

                if (jsonTags.tags.Contains(tagToRemove))
                {
                    jsonTags.tags.Remove(tagToRemove);
                    File.WriteAllText(path, JsonUtility.ToJson(jsonTags, prettyPrint: true));
                }
            }

            HopperTypes.Remove(tag.ToUpper());
            Dictionary<string, Color> hopsToSave = HopperTypes;
            foreach (string entry in GetProjectHopTags())
            {
                hopsToSave.Remove(entry);
            }
            SaveUnityHopperTypes(hopsToSave);
        }

        /// <summary>
        /// Compiles all the tags that are saved in the config file
        /// </summary>
        /// <returns>A list of strings representing the tag for each hop</returns>
        public static List<string> GetProjectHopTags()
        {
            string path = Application.dataPath + "/Settings/config.json";
            
            List<string> tagsToReturn = new List<string>();

            if (File.Exists(path))
            {
                TagList jsonTags = JsonUtility.FromJson<TagList>(File.ReadAllText(path));

                foreach (Tag tag in jsonTags.tags)
                {
                    tagsToReturn.Add(tag.HopperTag.ToUpper());
                }
            }
            
            return tagsToReturn;
        }

        /// <summary>
        /// Loads the excluded folders and files from the editor preferences.
        /// </summary>
        public static void LoadExclusions()
        {
            ExcludedFolders = EditorPrefs.GetString(EXCLUDE_FOLDER_KEY, "").Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
            ExcludedFiles = EditorPrefs.GetString(EXCLUDE_FILE_KEY, "").Split('|').Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        /// <summary>
        /// Saves the current excluded folders and files to the editor preferences.
        /// </summary>
        public static void SaveExclusions()
        {
            EditorPrefs.SetString(EXCLUDE_FOLDER_KEY, string.Join('|', ExcludedFolders));
            EditorPrefs.SetString(EXCLUDE_FILE_KEY, string.Join('|', ExcludedFiles));
        }

        /// <summary>
        /// Adds a path to the exclusion list
        /// </summary>
        /// <param name="path">The path to exclude</param>
        /// <param name="isFolder">Indicateds whether the path is a folder</param>
        public static void AddExcludedPath(string path, bool isFolder)
        {
            if (isFolder)
            {
                if (!ExcludedFolders.Contains(path))
                {
                    ExcludedFolders.Add(path);
                }
            }
            else
            {
                if (!ExcludedFiles.Contains(path))
                {
                    ExcludedFiles.Add(path);
                }
            }

            SaveExclusions();
        }

        /// <summary>
        /// Removes a path from the exlusion list
        /// </summary>
        /// <param name="path">The path to remove from exclusions</param>
        public static void RemoveExcludedPath(string path)
        {
            ExcludedFolders.Remove(path);
            ExcludedFiles.Remove(path);
            SaveExclusions();
        }

        /// <summary>
        /// Gets the list of excluded folders
        /// </summary>
        /// <returns>A list of excluded folder paths</returns>
        public static List<string> GetExcludedFolders() => ExcludedFolders;

        /// <summary>
        /// Gets the list of excluded files
        /// </summary>
        /// <returns>A list of excluded file paths</returns>
        public static List<string> GetExcludedFiles() => ExcludedFiles;

        /// <summary>
        /// Determines whether a given path is excluded
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>True if path is excluded; otherwise, false</returns>
        public static bool IsPathExcluded(string path)
        {
            path = HopUtils.NormalizePath(path);
            bool excluded = ExcludedFolders.Any(path.StartsWith) || ExcludedFiles.Contains(path);
            return excluded;
        }
    }
}