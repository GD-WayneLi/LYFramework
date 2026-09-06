using System;
using UnityEngine;

namespace LYUnity.Utility.Unity
{
    /// <summary>
    /// Provides a small, typed wrapper around Unity's PlayerPrefs persistence API.
    /// </summary>
    public static class UnityLocalStorage
    {
        public static bool HasKey(string key)
        {
            ValidateKey(key);
            return PlayerPrefs.HasKey(key);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            ValidateKey(key);
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            ValidateKey(key);
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            ValidateKey(key);
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static T GetObject<T>(string key) where T : class
        {
            return GetObject<T>(key, null);
        }

        public static T GetObject<T>(string key, T defaultValue) where T : class
        {
            ValidateKey(key);
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
                return defaultValue;

            return JsonUtility.FromJson<T>(json);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            ValidateKey(key);
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        public static void SetInt(string key, int value)
        {
            ValidateKey(key);
            PlayerPrefs.SetInt(key, value);
        }

        public static void SetFloat(string key, float value)
        {
            ValidateKey(key);
            PlayerPrefs.SetFloat(key, value);
        }

        public static void SetString(string key, string value)
        {
            ValidateKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            PlayerPrefs.SetString(key, value);
        }

        public static void SetObject<T>(string key, T value) where T : class
        {
            ValidateKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            PlayerPrefs.SetString(key, JsonUtility.ToJson(value));
        }

        public static void SetBool(string key, bool value)
        {
            ValidateKey(key);
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static void DeleteKey(string key)
        {
            ValidateKey(key);
            PlayerPrefs.DeleteKey(key);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("PlayerPrefs key cannot be null, empty, or whitespace.", nameof(key));
        }
    }
}
