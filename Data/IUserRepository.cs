using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public interface IUserRepository
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToAdd);
        public IEnumerable<User> GetUsers();
        public User GetSigleUser(int userId);
        public UserSalary GetSigleUserSalary(int userId);
        public UserJobInfo GetUserJobInfo(int userId);
    }
}