﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

class PasswordManager
{
    private static readonly string PasswordFile = "passwords.dat";
    private static readonly string MasterPasswordFile = "master_password.dat";

    // Get a secure hash of the master password
    private static byte[] GetKey(string masterPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
        }
    }

    // Encrypt a string using AES
    private static string Encrypt(string plaintext, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                ms.Write(iv, 0, iv.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plaintext);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // Decrypt a string using AES
    private static string Decrypt(string ciphertext, byte[] key)
    {
        byte[] cipherBytes = Convert.FromBase64String(ciphertext);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            using (var ms = new MemoryStream(cipherBytes))
            {
                byte[] iv = new byte[16];
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }

    // Save encrypted passwords to file
    private static void SavePasswords(List<Tuple<string, string>> passwords, byte[] key)
    {
        using (var writer = new StreamWriter(PasswordFile))
        {
            foreach (var entry in passwords)
            {
                string encrypted = Encrypt(entry.Item1 + ":" + entry.Item2, key);
                writer.WriteLine(encrypted);
            }
        }
    }

    // Load and decrypt passwords from file
    private static List<Tuple<string, string>> LoadPasswords(byte[] key)
    {
        var passwords = new List<Tuple<string, string>>();
        if (!File.Exists(PasswordFile)) return passwords;

        using (var reader = new StreamReader(PasswordFile))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string decrypted = Decrypt(line, key);
                var parts = decrypted.Split(':');
                if (parts.Length == 2)
                {
                    passwords.Add(new Tuple<string, string>(parts[0], parts[1]));
                }
            }
        }
        return passwords;
    }

    // Save the encrypted master password
    private static void SaveMasterPassword(string masterPassword)
    {
        byte[] key = GetKey("hardcoded_salt"); // A fixed salt for encrypting the master password
        string encryptedMaster = Encrypt(masterPassword, key);
        File.WriteAllText(MasterPasswordFile, encryptedMaster);
    }

    // Load the encrypted master password
    private static string LoadMasterPassword()
    {
        if (!File.Exists(MasterPasswordFile)) return null;

        byte[] key = GetKey("hardcoded_salt");
        string encryptedMaster = File.ReadAllText(MasterPasswordFile);
        return Decrypt(encryptedMaster, key);
    }

    static void Main()
    {
        string masterPassword = LoadMasterPassword();
        if (masterPassword == null)
        {
            Console.Write("Set your master password: ");
            masterPassword = Console.ReadLine();
            SaveMasterPassword(masterPassword);
            Console.WriteLine("Master password saved securely.");
        }
        else
        {
            while (true)
            {
                Console.Write("Enter your master password: ");
                string inputPassword = Console.ReadLine();
                if (inputPassword == masterPassword)
                {
                    Console.WriteLine("Access granted!");
                    break;
                }
                else
                {
                    Console.WriteLine("Incorrect password. Try again.");
                }
            }
        }

        var passwords = new List<Tuple<string, string>>();

        while (true)
        {
            Console.WriteLine("\nPassword Manager");
            Console.WriteLine("1. Add Password");
            Console.WriteLine("2. View Passwords");
            Console.WriteLine("3. Exit");
            Console.Write("Choice: ");

            string choice = Console.ReadLine();

            byte[] masterKey = GetKey(masterPassword);

            if (choice == "1")
            {
                Console.Write("Enter website: ");
                string website = Console.ReadLine();
                Console.Write("Enter password: ");
                string password = Console.ReadLine();
                passwords.Add(new Tuple<string, string>(website, password));
                SavePasswords(passwords, masterKey);
            }
            else if (choice == "2")
            {
                try
                {
                    passwords = LoadPasswords(masterKey);
                    foreach (var entry in passwords)
                    {
                        Console.WriteLine($"Website: {entry.Item1}, Password: {entry.Item2}");
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to load passwords. Incorrect master password or corrupted file.");
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("Exiting...");
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }
        }
    }
}

