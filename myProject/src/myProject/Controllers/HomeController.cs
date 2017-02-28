using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using myProject.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace myProject.Controllers
{
    public class HomeController : Controller
    {
        myDataBase db = new myDataBase();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("loginid") == null || HttpContext.Session.GetString("loginid") == "")
            {

            }
            else
            {
                //已登录跳 MyCenter页
                Response.Redirect(Url.Action("MyCenter", "MyIndex"));
                return Redirect(Url.Action("MyCenter", "MyIndex"));
            }
            return View();
        }

        public IActionResult Registered()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]//登录判断
        public JsonResult LoginAjax()
        {
            bool bSuccess = false;
            string strMes = string.Empty;

            string account = System.Net.WebUtility.HtmlEncode(Request.Form["account"]);
            string password = System.Net.WebUtility.HtmlEncode(Request.Form["password"]);

            string md5pass = Md5(password);
            //验证账号密码是否正确，暂时都归为密码不存在，账号不考虑
            var user = (from u in db.Users
                        where u.loginid == account && u.password == md5pass
                        select u).FirstOrDefault();

            if (null == user)
            {
                bSuccess = false;
                strMes = "登陆失败，用户名或者密码错误";
                return Json(new { success = bSuccess, message = strMes });
            }
            bSuccess = true;
            strMes = "登录成功";
            HttpContext.Session.SetString("loginid", user.id);
            HttpContext.Session.SetString("isadmin", user.isadmin.ToString());
            if (user.name!=null)
                HttpContext.Session.SetString("loginname", user.name);
            else
                HttpContext.Session.SetString("loginname", "");
            return Json(new { success = bSuccess, message = strMes });
        }

        [HttpPost]//注册
        public JsonResult RegisteredAjax()
        {
            bool bSuccess = false;
            string strMes = string.Empty;

            string account = System.Net.WebUtility.HtmlEncode(Request.Form["account"]);
            string password = System.Net.WebUtility.HtmlEncode(Request.Form["password"]);

            string md5pass = Md5(password);
            
            var user = (from u in db.Users
                        where u.loginid == account
                        select u).FirstOrDefault();
            //账号不存在，注册
            if (null == user)
            {

                Model.dbo.MyUser NewUser = new Model.dbo.MyUser
                {
                    loginid = account,
                    id = Guid.NewGuid().ToString(),
                    password = md5pass,
                    isadmin=0,
                    openblog=0,
                    createtime = DateTime.Now
                };

                db.Users.Add(NewUser);
                db.SaveChanges();

                bSuccess = true;
                strMes = "注册成功！";
                return Json(new { success = bSuccess, message = strMes });
            }
            bSuccess = false;
            strMes = "账号已存在，请重新输入";
            return Json(new { success = bSuccess, message = strMes });
        }

        //MD5加密
        private static string Md5(string Source)
        {
            MD5 md5 = MD5.Create();
            var result = md5.ComputeHash(Encoding.UTF8.GetBytes(Source));
            var strResult = BitConverter.ToString(result);
            return strResult.Replace("-", "");
        }
    }
}
