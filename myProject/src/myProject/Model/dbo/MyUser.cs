using System;
using System.ComponentModel.DataAnnotations;

namespace myProject.Model.dbo
{
    public class MyUser
    {
        [Key]
        public int iid { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int isadmin { get; set; } // 0 不是，1 是
        public int openblog { get; set; }    //0 没有，1 有
        public string loginid { get; set; }
        public string password { get; set; }
        public DateTime? createtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string update_userid { get; set; }
    }
}