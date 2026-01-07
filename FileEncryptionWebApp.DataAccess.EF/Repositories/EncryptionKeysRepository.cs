using FileEncryptionWebApp.DataAccess.EF.Context;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileEncryptionWebApp.DataAccess.EF.Models;

namespace FileEncryptionWebApp.DataAccess.EF.Repositories
{
    public class EncryptionKeysRepository
    {
        private FileEncryptionProjectDatabaseContext _dbContext;

        public EncryptionKeysRepository(FileEncryptionProjectDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public int Create(EncryptionKey encryptionKey)
        {
            _dbContext.Add(encryptionKey);
            _dbContext.SaveChanges();

            return encryptionKey.EncryptionKeyId;
        }

        public int? Update(EncryptionKey encryptionKey)
        {
            EncryptionKey? existingEncryptionKey = _dbContext.EncryptionKeys.Find(encryptionKey.EncryptionKeyId);
            if (existingEncryptionKey == null)
            {
                return null;
            }
            existingEncryptionKey.CreatedAt = encryptionKey.CreatedAt;
            existingEncryptionKey.IsActive = encryptionKey.IsActive;
            existingEncryptionKey.Nonce = encryptionKey.Nonce;
            existingEncryptionKey.EncryptionKey1 = encryptionKey.EncryptionKey1;

            _dbContext.SaveChanges();

            return encryptionKey.EncryptionKeyId;
        }

        public bool Delete(int encryptionKeyId)
        {
            EncryptionKey? encryptionKey = _dbContext.EncryptionKeys.Find(encryptionKeyId);
            if (encryptionKey == null)
            {
                return false;
            }
            _dbContext.Remove(encryptionKey);
            _dbContext.SaveChanges();
            return true;
        }

        public List<EncryptionKey>? GetAllEncryptionKeys()
        {
            List<EncryptionKey> encryptionKeysList = _dbContext.EncryptionKeys.ToList();
            return encryptionKeysList;
        }

        public EncryptionKey? GetEncryptionKeyByID(int encryptionKeyId)
        {
            EncryptionKey? encryptionKey = _dbContext.EncryptionKeys.Find(encryptionKeyId);
            return encryptionKey;
        }
    }
}
