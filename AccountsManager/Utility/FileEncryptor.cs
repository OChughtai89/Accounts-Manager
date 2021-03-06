﻿using AccountsManager.MasterConfig;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AccountsManager.Encrpytion
{

    public class FileEncryptor
    {        
        private static FileStream fsCrypt;
        private static CryptoStream cs;
        private static FileStream fsIn;

        public FileEncryptor()
        {

        }

        public static RijndaelManaged DES
        {
            get;
            set;
        }
             
        //salt should only be created when password is created or changed, store it somewhere, and use it to generate hash when validating password.
        public static string CreateSalt(int size)
        {            
            var rng = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[size];
            
            rng.GetBytes(buffer);
            var salt = Convert.ToBase64String(buffer);            
            return salt;
        }

        public static string CreateHash(string password,string passwordSalt)
        {
            var salt = Convert.FromBase64String(passwordSalt);            
            DES = FileEncryptor.CreateDES(password, salt);
            var hash = Convert.ToBase64String(FileEncryptor.DES.Key);
            return hash;
        }
      
        public static void Encrypt(string file, string password, string salt)
        {
            var saltValue = Convert.FromBase64String(MasterConfigManager.getInstance().getPasswordSalt());
            EncryptFile(file,password,saltValue);
        }

        public static void Decrypt(string file, string password,string salt)
        {
            var saltValue = Convert.FromBase64String(salt);
            DecryptFile(file,password,saltValue);
        }

        
        public static RijndaelManaged CreateDES(string key,byte[] salt)
        {
            Rfc2898DeriveBytes keygen = new Rfc2898DeriveBytes(key,salt,1000);            
            RijndaelManaged des = new RijndaelManaged();            
            des.Key = keygen.GetBytes(32);            
            des.IV = keygen.GetBytes(16);            
            return des;
        }

        private static void EncryptFile(string file, string password,byte[] salt)
        {
            try
            {                
                byte[] fileBuffer = File.ReadAllBytes(file);                
                fsCrypt = new FileStream(file, FileMode.Create);               
                DES = CreateDES(password,salt);                                
                cs = new CryptoStream(fsCrypt, DES.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(fileBuffer, 0, fileBuffer.Length);
                cs.FlushFinalBlock();
                MasterConfigManager.getInstance().setFileEncrypted(true);
            }          
            finally
            {
                if (fsCrypt != null)
                    fsCrypt.Close();
                if (cs != null)
                    cs.Close();
            }
        }

        private static void DecryptFile(string file,string password,byte[] salt)
        {
            try
            {                
                UnicodeEncoding ue = new UnicodeEncoding();
                MemoryStream ms = new MemoryStream();
                DES = CreateDES(password,salt);
                fsCrypt = new FileStream(file, FileMode.Open);

                cs = new CryptoStream(fsCrypt, DES.CreateDecryptor(), CryptoStreamMode.Read);

                cs.CopyTo(ms);
                cs.Close();
                ms.Position = 0;
                fsIn = new FileStream(file, FileMode.Create);
                int data;

                while ((data=ms.ReadByte()) != -1)
                {
                    fsIn.WriteByte((byte)data);
                }
                MasterConfigManager.getInstance().setFileEncrypted(false);
            }          
            finally
            {
                if (fsIn != null)
                    fsIn.Close();
                if (fsCrypt != null)
                    fsCrypt.Close();               
            }
        }
    }
}
