using FileEncryptionWebApp.DataAccess.EF.Context;
using FileEncryptionWebApp.DataAccess.EF.Models;

namespace FileEncryptionWebApp.DataAccess.EF.Repositories
{
    public class EntriesRepository
    {
        private FileEncryptionProjectDatabaseContext _dbContext;

        public EntriesRepository(FileEncryptionProjectDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public int Create(Entry entry)
        {
            _dbContext.Add(entry);
            _dbContext.SaveChanges();

            return entry.EntryId;
        }

        public int? Update(Entry entry)
        {
            Entry? existingEntry = _dbContext.Entries.Find(entry.EncryptionKeyId);
            if (existingEntry == null)
            {
                return null;
            }
            existingEntry.Entry1 = entry.Entry1;
            existingEntry.CreationDate = entry.CreationDate;
            existingEntry.IsEncrypted = entry.IsEncrypted;
            existingEntry.EncryptionKeyId = entry.EncryptionKeyId;


            _dbContext.SaveChanges();

            return existingEntry.EntryId;
        }

        public bool Delete(int entryId)
        {
            Entry? entry = _dbContext.Entries.Find(entryId);
            if (entry == null)
            {
                return false;
            }
            _dbContext.Remove(entry);
            _dbContext.SaveChanges();
            return true;
        }

        public List<Entry>? GetAllEntries()
        {
            List<Entry> entriesList = _dbContext.Entries.ToList();
            return entriesList;
        }

        public Entry? GetEntryById(int entrytId)
        {
            Entry? entry = _dbContext.Entries.Find(entrytId);
            if (entry == null)
            {
                return null;
            }
            return entry;
        }
    }
}
