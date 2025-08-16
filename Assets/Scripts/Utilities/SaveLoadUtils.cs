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
    private static string xorKey = "finalproject2025"; //Key cho XOR
    private static string aesKey = "finalproject2025"; //16/24/32 ký tự cho AES
    private static string filePath = Application.persistentDataPath + "/";

    // ================== SAVE ==================
    public static void Save<T>(string fileName, T data, EncryptionType type = EncryptionType.None)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = filePath + fileName + ".json";

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

            File.WriteAllText(path, finalData);
            Debug.Log($"Save data: {path} ({type})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    // ================== LOAD ==================
    public static T Load<T>(string fileName, EncryptionType type = EncryptionType.None) where T : class
    {
        try
        {
            string path = filePath + fileName + ".json";

            if (!File.Exists(path))
            {
                Debug.Log($"File not found: {path}");
                return null;
            }

            string fileData = File.ReadAllText(path);

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

            T data = JsonUtility.FromJson<T>(json);
            Debug.Log($"Load data: {data}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            return null;
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
