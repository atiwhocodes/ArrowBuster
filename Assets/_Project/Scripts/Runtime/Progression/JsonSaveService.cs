using System;
using System.IO;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Local JSON save (01 §14): atomic write to save.tmp then File.Replace with save.bak; load falls back to the
    /// backup and then to a fresh save. Failures are logged, never thrown into gameplay.
    /// </summary>
    public sealed class JsonSaveService : ISaveService
    {
        private readonly string _path;
        private readonly string _tmpPath;
        private readonly string _bakPath;

        public SaveGame Data { get; private set; }

        public JsonSaveService(string directory = null)
        {
            directory = directory ?? Application.persistentDataPath;
            _path = Path.Combine(directory, "save.json");
            _tmpPath = Path.Combine(directory, "save.tmp");
            _bakPath = Path.Combine(directory, "save.bak");
            Data = TryLoad(_path) ?? TryLoad(_bakPath) ?? new SaveGame();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(_tmpPath, JsonUtility.ToJson(Data, true));
                if (File.Exists(_path)) File.Replace(_tmpPath, _path, _bakPath);
                else File.Move(_tmpPath, _path);
            }
            catch (Exception e)
            {
                Log.Warn(LogCat.Save, "Save failed: " + e.Message);
                Services.Crash.RecordNonFatal("save_failed: " + e.Message);
            }
        }

        private static SaveGame TryLoad(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var data = JsonUtility.FromJson<SaveGame>(File.ReadAllText(path));
                if (data == null) return null;
                data.levels = data.levels ?? new System.Collections.Generic.List<LevelProgress>();
                data.settings = data.settings ?? new SettingsData();
                data.seenPrompts = data.seenPrompts ?? new System.Collections.Generic.List<string>();
                return data;
            }
            catch (Exception e)
            {
                Log.Warn(LogCat.Save, "Load failed for " + path + ": " + e.Message);
                return null;
            }
        }
    }
}
