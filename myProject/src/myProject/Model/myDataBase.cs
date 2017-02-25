using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using myProject.Model.dbo;

namespace myProject.Model
{
    public class myDataBase: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Data Source=(local);uid=sa;pwd=11;DataBase=myDataBase");
        }
        public DbSet<MyUser> Users { get; set; } // 用户表
        public DbSet<BlogApply> BlogApply { get; set; } // 博客申请表
    }
}
