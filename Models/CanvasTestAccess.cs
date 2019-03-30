using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasRemindWebApp.Models
{
    //Model used to read the Client_Id and Client_Secret values from Configuration files
    public class CanvasTestAccess
    {
        public string Client_Id { get; set; }
        public string Client_Secret { get; set; }

    }
}
