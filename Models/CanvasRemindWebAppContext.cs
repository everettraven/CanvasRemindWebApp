using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CanvasRemindWebApp.Models
{
    public class CanvasRemindWebAppContext:DbContext
    {
        public CanvasRemindWebAppContext()
        {
        }

        public CanvasRemindWebAppContext(DbContextOptions<CanvasRemindWebAppContext> options) : base(options) { }


        public DbSet<User> User { get; set; }


    }
}
