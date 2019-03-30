using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanvasRemindWebApp.Models
{
    //Model used to read the AES Key and IV values from the configuration files
    public class AESEncryption
    {
        public string Key { get; set; }
        public string IV { get; set; }
    }
}
