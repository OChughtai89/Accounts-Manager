﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountsManager
{
    public class MasterPasswordManager
    {
        public const string ADMINACCT = "admin";        
        private static string passwordHash;
        private static string passwordSalt;
        private static string configXmlFilePath;
        private AccountsManagerConfigFileParser amcp;
        private AccountsManagerConfigFileWriter amcw;        
        private static Lazy<MasterPasswordManager> instance = new Lazy<MasterPasswordManager>(( ) => new MasterPasswordManager(configXmlFilePath));

        private MasterPasswordManager(string configXmlFile)
        {            
            amcp = new AccountsManagerConfigFileParser(configXmlFile);
            amcw = new AccountsManagerConfigFileWriter(configXmlFile);
            var configFileData = amcp.parseaccountsConfigFile();
            passwordHash = configFileData.passwordHash;
            passwordSalt = configFileData.salt;            
        }

        public static MasterPasswordManager getInstance(string filePath= "")
        {
            MasterPasswordManager.configXmlFilePath = filePath;
            return instance.Value;
        }

        public string getPasswordHash()
        {
            return passwordHash;
        }
        
        public string getPasswordSalt()
        {
            return passwordSalt;
        }       

        public bool validatePaswword(string password)
        {                          
            FileEncryptor.CreateHash(password, passwordSalt);
            string hash = Convert.ToBase64String(FileEncryptor.DES.Key);
            if (passwordHash == hash)            
                return true;            
            else
                return false;
        }

        public void setPassword(string newPassword)
        {
            passwordSalt =  FileEncryptor.CreateSalt(10);
            passwordHash = FileEncryptor.CreateHash(newPassword, passwordSalt);
            amcw.WriteToFile(passwordHash, passwordSalt);
        }

        public void changePassword(string oldPassword, string newPassword)
        {
            var correctPassword = validatePaswword(oldPassword);
            if (!correctPassword)
            {
                System.Windows.MessageBox.Show("Old Password is not correct!");
                return;
            }
            else
            {
                setPassword(newPassword);
            }
        }
    }


}