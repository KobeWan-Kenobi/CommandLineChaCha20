//using FileEncryptionWebApp.DataAccess.EF.Repositories;
//using FileEncryptionWebApp.DataAccess.EF.Models;
//using FileEncryptionWebApp.DataAccess.EF.Context;
//namespace FileEncryptionWebApp.ViewModels
//{
//    public class UsersViewModel
//    {
//        private UsersRepository _repo;

//        public List<User> UserList { get; set; }

//        public User CurrentUser { get; set; }

//        public bool IsActionSuccess { get; set; }

//        public string ActionMessage { get; set; }

//        public UsersViewModel(FileEncryptionProjectDatabaseContext context)
//        {
//            _repo = new UsersRepository(context);
//            UserList = GetAllUsers();
//            CurrentUser = UserList.FirstOrDefault();
//        }

//        public UsersViewModel(FileEncryptionProjectDatabaseContext context, int userId)
//        {
//            _repo = new UsersRepository(context);
//            UserList = GetAllUsers();

//            if (userId > 0)
//            {
//                CurrentUser = GetUser(userId);
//            }
//            else
//            {
//                CurrentUser = new User();
//            }
//        }

//        public void SaveUser(User user)
//        {
//            if (user.UserId > 0)
//            {
//                _repo.Update(user);
//            }
//            else
//            {
//                user.UserId = _repo.Create(user);
//            }

//            UserList = GetAllUsers();
//            CurrentUser = GetUser(user.UserId);
//        }

//        public void RemoveUser(int userId)
//        {
//            _repo.Delete(userId);
//            UserList = GetAllUsers();
//            CurrentUser = UserList.FirstOrDefault();
//        }

//        public List<User> GetAllUsers()
//        {
//            return _repo.GetAllUsers();
//        }

//        public User GetUser(int userId)
//        {
//            return _repo.GetUserById(userId);
//        }
//    }
//}
