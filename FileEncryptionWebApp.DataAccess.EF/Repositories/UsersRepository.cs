using FileEncryptionWebApp.DataAccess.EF.Context;
using FileEncryptionWebApp.DataAccess.EF.Models;

namespace FileEncryptionWebApp.DataAccess.EF.Repositories
{
    public class UserRepository
    {
        private FileEncryptionProjectDatabaseContext _dbContext;

        public UserRepository(FileEncryptionProjectDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public int Create(User user)
        {
            _dbContext.Add(user);
            _dbContext.SaveChanges();

            return user.UserId;
        }

        public int? Update(User user)
        {
            User? existingUser = _dbContext.Users.Find(user.UserId);
            if (existingUser == null)
            {
                return null;
            }
            existingUser.EncryptionKey = user.EncryptionKey;
            existingUser.Username = user.Username;
            existingUser.Password = user.Password;


            _dbContext.SaveChanges();

            return existingUser.UserId;
        }

        public bool Delete(int userId)
        {
            User? existingUser = _dbContext.Users.Find(userId);
            if (existingUser == null)
            {
                return false;
            }
            _dbContext.Remove(existingUser);
            _dbContext.SaveChanges();
            return true;
        }

        public List<User>? GetAllUsers()
        {
            List<User> usersList = _dbContext.Users.ToList();
            return usersList;
        }

        public User? GetUserById(int userId)
        {
            User? existingUser= _dbContext.Users.Find(userId);
            if (existingUser == null)
            {
                return null;
            }
            return existingUser;
        }
    }
}
