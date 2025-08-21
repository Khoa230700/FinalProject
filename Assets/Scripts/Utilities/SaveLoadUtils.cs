using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

public enum EncryptionType
{
    None,
    XOR,
    AES
}

public static class SaveLoadUtils
{
    private static readonly object locker = new(); // Thread-safety
    public static GameData Data { get; private set; } = new();

    private static string fileName = "GameData";
    private static int maxBackup = 3;
    private static string xorKey = "finalproject2025";
    private static string aesKey = "finalproject2025";

    // ================== SAVE ==================
    public static void Save(EncryptionType type = EncryptionType.None)
    {
        lock (locker)
        {
            try
            {
                Data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                for (int i = maxBackup - 1; i >= 0; i--)
                {
                    string oldPath = GetPath(i);
                    string newPath = GetPath(i + 1);

                    if (File.Exists(oldPath))
                    {
                        if (i + 1 >= maxBackup)
                            File.Delete(oldPath);
                        else
                            File.Copy(oldPath, newPath, true);
                    }
                }

                string json = JsonUtility.ToJson(Data, true);
                string finalData = json;

                switch (type)
                {
                    case EncryptionType.XOR:
                        finalData = EncryptXOR(json, xorKey);
                        break;
                    case EncryptionType.AES:
                        finalData = EncryptAES(json, aesKey);
                        break;
                }

                File.WriteAllText(GetPath(0), finalData, Encoding.UTF8);
                Debug.Log("Game saved: " + GetPath(0));
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e.Message);
            }
        }
    }

    // ================== LOAD ==================
    public static GameData Load(EncryptionType type = EncryptionType.None)
    {
        lock (locker)
        {
            for (int i = 0; i < maxBackup; i++)
            {
                string path = GetPath(i);
                if (!File.Exists(path)) continue;

                try
                {
                    string fileData = File.ReadAllText(path, Encoding.UTF8);

                    string json = fileData;
                    switch (type)
                    {
                        case EncryptionType.XOR:
                            json = DecryptXOR(fileData, xorKey);
                            break;
                        case EncryptionType.AES:
                            json = DecryptAES(fileData, aesKey);
                            break;
                    }

                    Data = JsonUtility.FromJson<GameData>(json);
                    if (Data != null)
                    {
                        Debug.Log($"Game loaded from: {path}");
                        return Data;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Load failed from {path}: {e.Message}");
                }
            }

            Debug.LogWarning("No valid save file found.");
            return Data = new GameData();
        }
    }

    // ================== XOR ==================
    private static string EncryptXOR(string plainText, string key)
    {
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= keyBytes[i % keyBytes.Length];

        return Convert.ToBase64String(data);
    }

    private static string DecryptXOR(string cipherText, string key)
    {
        byte[] data = Convert.FromBase64String(cipherText);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= keyBytes[i % keyBytes.Length];

        return Encoding.UTF8.GetString(data);
    }

    // ================== AES ==================
    private static string EncryptAES(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetAESKey(key);
            aes.IV = new byte[16];

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    private static string DecryptAES(string cipherText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetAESKey(key);
            aes.IV = new byte[16];

            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }

    private static byte[] GetAESKey(string key)
    {
        using var sha = SHA256.Create();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] hash = sha.ComputeHash(keyBytes);

        // lấy 16 byte đầu tiên (AES-128)
        byte[] finalKey = new byte[16];
        Array.Copy(hash, finalKey, 16);
        return finalKey;
    }

    private static string GetPath(int index)
    {
        return Path.Combine(Application.persistentDataPath, $"{fileName}_{index}.json");
    }
}
