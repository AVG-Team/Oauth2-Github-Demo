using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Test_Thuyet_Trinh_Oauth2.Models;

namespace Test_Thuyet_Trinh_Oauth2.Manages
{

    public class UserManager
    {
        private readonly MyDataDataContext _data = new MyDataDataContext();

        public UserManager()
        {
        }

        public async Task<IdentityResult> CreateAccountUserAsync(string fullname, User usertmp, string email)
        {
            if (usertmp.username == null)
                return IdentityResult.Failed();

            try
            {
                if (usertmp.password == null)
                    usertmp.password = "12345678";
                string password = BCrypt.Net.BCrypt.HashPassword(usertmp.password);
                usertmp.password = password;
                usertmp.fullname = fullname;
                usertmp.email = email;
                _data.Users.InsertOnSubmit(usertmp);
                _data.SubmitChanges();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed();
            }
        }

        //true : exist ; false : no exist
        public bool CheckUsername(string username)
        {
            User user = _data.Users.Where(x => x.username == username || x.email == username).FirstOrDefault();
            if (user != null)
                return true;
            return false;
        }

        public User GetUserFromToken()
        {
            MyDataDataContext _data1 = new MyDataDataContext();
            string token = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
            string username = TokenHelper.GetUsernameFromToken(token);
            User user = _data1.Users.Where(x => x.email == username).FirstOrDefault();
            return user;
        }

        public bool IsAuthenticated()
        {
            string authToken = HttpContext.Current.Request.Cookies["AuthToken"]?.Value;
            return !string.IsNullOrEmpty(authToken);
        }

        public void login(string email)
        {
            var token = TokenHelper.GenerateToken(email);
            HttpCookie cookie = new HttpCookie("AuthToken", token);
            cookie.Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public void logout()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["AuthToken"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}