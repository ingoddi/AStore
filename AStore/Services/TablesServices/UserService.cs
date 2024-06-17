using AStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AStore.Services.TablesServices
{
    public class UserService
    {
        private readonly DatabaseManager _context;

        public UserService(DatabaseManager context)
        {
            _context = context;
        }

        public void CreateUser(string username, string password)
        {
            var user = new User { Username = username, Password = password };
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUser(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public void CreateManager(string username, string password, int accessLevel)
        {
            var user = GetUser(username, password);
            if (user != null)
            {
                var manager = new Manager { UserId = user.UserId, AccessLevel = accessLevel };
                _context.Managers.Add(manager);
                _context.SaveChanges();
            }
        }

        public Manager GetManager(int userId)
        {
            return _context.Managers.SingleOrDefault(m => m.UserId == userId);
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username.ToLower() == username.ToLower());
        }
    }
}
