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
    public class MyIndexController : Controller
    {
        myDataBase db = new myDataBase(); 

        public IActionResult Backstage()
        {
            if (!IsLogin())
            {
                Response.Redirect(Url.Action("Login", "Home"));
                return Redirect(Url.Action("Login", "Home"));
            }
            if (!IsAdmin())
            {
                Response.Redirect(Url.Action("Error", "Home"));
                return Redirect(Url.Action("Error", "Home"));
            }

            var id= HttpContext.Session.GetString("loginid");
            var userlist = from u in db.Users
                           where u.id!=id
                           orderby u.createtime descending
                           select u;

            ViewBag.userList = userlist;
            ViewBag.userCount = userlist.Count();

            return View();
        }

        public IActionResult DealBlogApply()
        {
            if (!IsLogin())
            {
                Response.Redirect(Url.Action("Login", "Home"));
                return Redirect(Url.Action("Login", "Home"));
            }
            if (!IsAdmin())
            {
                Response.Redirect(Url.Action("Error", "Home"));
                return Redirect(Url.Action("Error", "Home"));
            }

            var id = HttpContext.Session.GetString("loginid");
            var blogapplylist = from u in db.BlogApply
                           orderby u.apply_createtime descending
                           select u;

            ViewBag.blogapplylist = blogapplylist;
            ViewBag.blogapplycount = blogapplylist.Count();

            return View();
        }

        public IActionResult MyCenter()
        {
            if (!IsLogin())
            {
                Response.Redirect(Url.Action("Login", "Home"));
                return Redirect(Url.Action("Login", "Home"));
            }
      
            ViewData["Name"] = HttpContext.Session.GetString("loginname");
            var id = HttpContext.Session.GetString("loginid");

            #region 获取用户数据
            var user = (from u in db.Users
                        where u.id == id
                        select u).FirstOrDefault();

            if (null == user)
            {
                Response.Redirect(Url.Action("Error", "Home"));
            }
            else
            {
                ViewData["id"] = id;
                ViewData["isadmin"] = user.isadmin.ToString();
                ViewData["createtime"] = DateTime.Parse(user.createtime.ToString()).ToString("yyyy-MM-dd");
                ViewData["openblog"] = user.openblog.ToString();
            }
            #endregion

            #region 获取申请博客的数据。有申请且处理中，不显示申请博客按钮。

            var apply_c = (from u in db.BlogApply
                            where u.apply_userid == id && u.state == 0
                            select u).FirstOrDefault();

            if (null == apply_c)
            {
                ViewData["state"] = 1;
            }
            else
            {
                ViewData["state"] = 0;
            }

            #endregion
            
            return View();
        }

        public IActionResult BlogApply()
        {
            if (!IsLogin())
            {
                Response.Redirect(Url.Action("Login", "Home"));
                return Redirect(Url.Action("Login", "Home"));
            }

            ViewData["Name"] = HttpContext.Session.GetString("loginname");
            ViewData["id"] = HttpContext.Session.GetString("loginid");

            return View();
        }

        [HttpPost]//博客申请
        public JsonResult BlogApplyAjax()
        {
            bool bSuccess = false;
            string strMes = string.Empty;

            string name = System.Net.WebUtility.HtmlEncode(Request.Form["name"]);
            string position = System.Net.WebUtility.HtmlEncode(Request.Form["position"]);
            string reason = System.Net.WebUtility.HtmlEncode(Request.Form["reason"]);

            var id = HttpContext.Session.GetString("loginid");

            //验证账号密码是否正确，暂时都归为密码不存在，账号不考虑
            var apply_count = (from u in db.BlogApply
                        where u.apply_userid == id && u.state == 0
                        select u).FirstOrDefault();

            if (null == apply_count)
            {

                Model.dbo.BlogApply NewBlogApply = new Model.dbo.BlogApply
                {
                    id = Guid.NewGuid().ToString(),
                    state = 0,
                    apply_name = name,
                    apply_userid = id,
                    apply_position = position,
                    apply_reason = reason,
                    apply_createtime = DateTime.Now
                };

                db.BlogApply.Add(NewBlogApply);
                db.SaveChanges();


                bSuccess = true;
                strMes = "申请成功，请等待管理员通知！";
                return Json(new { success = bSuccess, message = strMes });
            }
            bSuccess = false;
            strMes = "您的博客申请正在审核中，请耐心等待！";
            return Json(new { success = bSuccess, message = strMes });
        }

        [HttpPost]//个人中心修改
        public string MyCenter(string id, string username )
        {
            string _name = System.Net.WebUtility.HtmlEncode(username.Trim());
            string _id = System.Net.WebUtility.HtmlEncode(id.Trim());

            var user = (from u in db.Users
                        where u.id == _id
                        select u).FirstOrDefault();

            if (null == user && 0 == _name.Length)
            {
                Response.Redirect(Url.Action("Error", "Home"));
                return "";
            }

            user.name = _name;
            user.updatetime = DateTime.Now;
            user.update_userid = _id;
            db.SaveChanges();
            HttpContext.Session.SetString("loginname", _name);

            Response.Redirect(Url.Action("MyCenter", "MyIndex"));
            return "";
        }

        [HttpPost]//设置管理员
        public JsonResult setAdmin(string id)
        {
            bool bSuccess = false;
            string strMes = string.Empty;

            string _id = System.Net.WebUtility.HtmlEncode(id.Trim());

            var user = (from u in db.Users
                        where u.id == _id
                        select u).FirstOrDefault();

            if (null == user)
            {
                bSuccess = false;
                strMes = "修改失败";
                return Json(new { success = bSuccess, message = strMes });
            }
            if (user.isadmin == 1)
            {
                user.isadmin = 0;
            }
            else 
            {
                user.isadmin = 1;
            }
            user.updatetime = DateTime.Now;
            user.update_userid = HttpContext.Session.GetString("loginid");
            db.SaveChanges();

            bSuccess = true;
            strMes = "设置成功！";
            return Json(new { success = bSuccess, message = strMes });
        }

        [HttpPost]//处理开通博客请求
        public JsonResult DealBlogApplyAjax(string id, string userid,int state,string feedback)
        {
            bool bSuccess = false;
            string strMes = string.Empty;

            string _id = System.Net.WebUtility.HtmlEncode(id.Trim());
            string _userid = System.Net.WebUtility.HtmlEncode(userid.Trim());
            string _feedback = System.Net.WebUtility.HtmlEncode(feedback.Trim());
            #region 修改用户表 openblog  同意开通的时候，执行
            if (state == 1)
            {
                var user = (from u in db.Users
                            where u.id == _userid
                            select u).FirstOrDefault();

                user.openblog = 1;
                user.updatetime = DateTime.Now;
                user.update_userid = HttpContext.Session.GetString("loginid");
            }
            #endregion

            #region 修改博客申请表状态
            var BlogApply = (from u in db.BlogApply
                        where u.id == _id
                        select u).FirstOrDefault();

            BlogApply.state = state;
            BlogApply.updatetime = DateTime.Now;
            BlogApply.adminid = HttpContext.Session.GetString("loginid");
            BlogApply.feedback = _feedback;
            #endregion
            db.SaveChanges();

            bSuccess = true;
            strMes = "设置成功！";
            return Json(new { success = bSuccess, message = strMes });
        }

        //判断是否登录
        private bool IsLogin()
        {
            if (HttpContext.Session.GetString("loginid") == null || HttpContext.Session.GetString("loginid") == "")
            {
                return false;
            }
            return true;
        }

        //判断是否是管理员
        private bool IsAdmin()
        {
            if (HttpContext.Session.GetString("isadmin") == "1")
            {
                return true;
            }
            return false;
        }

    }
}
