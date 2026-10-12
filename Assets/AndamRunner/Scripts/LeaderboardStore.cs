using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AndamRunner
{
    [Serializable]
    public sealed class LeaderboardEntry
    {
        public string runId;
        public string nickname;
        public int score;
        public int supplies;
        public int distance;
        public string completedUtc;
    }

    [Serializable]
    public sealed class LeaderboardData
    {
        public int version = 1;
        public string lastNickname = "OPONG ANDAM";
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    // Offline top-ten board. A run ID prevents repeated results/navigation from adding duplicates.
    public sealed class LeaderboardStore
    {
        public const int Capacity = 10;
        private readonly string path;
        private readonly bool persist;
        private LeaderboardData data;
        public IReadOnlyList<LeaderboardEntry> Entries => data.entries;
        public string LastNickname => data.lastNickname;
        public bool LastSaveSucceeded { get; private set; } = true;

        public LeaderboardStore(string filePath, bool saveToDisk = true)
        {
            path = filePath;
            persist = saveToDisk;
            data = new LeaderboardData();
            if (persist && !TryLoad(path)) TryLoad(path + ".bak");
        }

        public static string CleanNickname(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "OPONG ANDAM";
            var clean = new StringBuilder(12);
            foreach (char letter in value.Trim().ToUpperInvariant())
            {
                if ((letter >= 'A' && letter <= 'Z') || (letter >= '0' && letter <= '9') || letter == ' ' || letter == '-') clean.Append(letter);
                if (clean.Length >= 12) break;
            }
            string nickname = clean.ToString().Trim();
            return nickname.Length > 0 ? nickname : "OPONG ANDAM";
        }

        public int Record(string id, string nickname, int score, int supplies, int distance)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A run needs an ID.", nameof(id));
            for (int i = 0; i < data.entries.Count; i++) if (data.entries[i].runId == id) return i + 1;
            data.lastNickname = CleanNickname(nickname);
            var entry = new LeaderboardEntry
            {
                runId = id, nickname = data.lastNickname, score = Math.Max(0, score),
                supplies = Mathf.Clamp(supplies, 0, PreparednessContent.SupplyCount), distance = Math.Max(0, distance),
                completedUtc = DateTime.UtcNow.ToString("o")
            };
            data.entries.Add(entry);
            SortAndTrim();
            int rank = 0;
            for (int i = 0; i < data.entries.Count; i++) if (data.entries[i].runId == id) rank = i + 1;
            LastSaveSucceeded = Save();
            return rank;
        }

        // Score first; more checklist types then distance break ties; older run wins an exact tie.
        private void SortAndTrim()
        {
            data.entries.Sort((a, b) =>
            {
                int order = b.score.CompareTo(a.score);
                if (order == 0) order = b.supplies.CompareTo(a.supplies);
                if (order == 0) order = b.distance.CompareTo(a.distance);
                if (order == 0) order = string.CompareOrdinal(a.completedUtc, b.completedUtc);
                if (order == 0) order = string.CompareOrdinal(a.runId, b.runId);
                return order;
            });
            if (data.entries.Count > Capacity) data.entries.RemoveRange(Capacity, data.entries.Count - Capacity);
        }

        private bool TryLoad(string source)
        {
            if (string.IsNullOrEmpty(source) || !File.Exists(source)) return false;
            try
            {
                LeaderboardData loaded = JsonUtility.FromJson<LeaderboardData>(File.ReadAllText(source));
                if (loaded == null || loaded.version != 1 || loaded.entries == null) return false;
                data = loaded;
                data.lastNickname = CleanNickname(data.lastNickname);
                var ids = new HashSet<string>();
                data.entries.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.runId) || !ids.Add(entry.runId));
                foreach (LeaderboardEntry entry in data.entries)
                {
                    entry.nickname = CleanNickname(entry.nickname);
                    entry.score = Math.Max(0, entry.score); entry.distance = Math.Max(0, entry.distance);
                    entry.supplies = Mathf.Clamp(entry.supplies, 0, PreparednessContent.SupplyCount);
                }
                SortAndTrim(); return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                return false;
            }
        }

        private bool Save()
        {
            if (!persist) return true;
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Copy(path, path + ".bak", true);
                File.Copy(temporary, path, true);
                File.Delete(temporary);
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                Debug.LogWarning("Leaderboard could not be saved: " + error.Message);
                return false;
            }
        }
    }
}
