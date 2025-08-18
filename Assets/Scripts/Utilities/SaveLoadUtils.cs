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
    public static GameData Data { get; private set; } = new();

    private static string xorKey = "finalproject2025"; //Key cho XOR
    private static string aesKey = "finalproject2025"; //16/24/32 ký tự cho AES
    private static string filePath = Application.persistentDataPath + "/GameData.json";

    // ================== SAVE ==================
    public static void Save(EncryptionType type = EncryptionType.None)
    {
        try
        {
            Data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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

            File.WriteAllText(filePath, finalData);
            Debug.Log($"Save data: {filePath} ({type})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    // ================== LOAD ==================
    public static void Load(EncryptionType type = EncryptionType.None)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.Log($"File not found: {filePath}");
                Data = new();
                return;
            }

            string fileData = File.ReadAllText(filePath);
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

            Data = JsonUtility.FromJson<GameData>(json) ?? new(); ;
            Debug.Log($"Load data: {Data.saveTime}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            Data = new();
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
            aes.IV = new byte[16]; // IV cố định (có thể random + lưu riêng để bảo mật hơn)

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private static string DecryptAES(string cipherText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GetAESKey(key);
            aes.IV = new byte[16];

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    private static byte[] GetAESKey(string key)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha.ComputeHash(keyBytes);

            // lấy 16 byte đầu tiên (AES-128)
            byte[] finalKey = new byte[16];
            Array.Copy(hash, finalKey, 16);
            return finalKey;
        }
    }
}
