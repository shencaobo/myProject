using System;
using System.ComponentModel.DataAnnotations;

namespace myProject.Model.dbo
{
    public class BlogApply
    {
        [Key]
        public int iid { get; set; }
        public string id { get; set; }
        public int state { get; set; }// state = 0 待处理;state = 1 已同意; state = 2 已拒绝
        public string apply_name { get; set; }
        public string apply_userid { get; set; } //申请人ID
        public string apply_position { get; set; }
        public string apply_reason { get; set; }
        public DateTime? apply_createtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string adminid { get; set; } //处理人，管理员id
        public string feedback { get; set; }
    }
}