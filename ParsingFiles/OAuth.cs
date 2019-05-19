using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace CanvasRemindWebApp.ParsingFiles
{
    //Get the JSON response of the token request after OAuth has been approved
    [DataContract(Name = "token")]
    public class OAuth
    {
            //Gets the Access Token from the JSON response
            [DataMember(Name = "access_token")]
            public string AccessToken { get; set; }

            //Gets the Refresh Token from the JSON response
            [DataMember(Name = "refresh_token")]
            public string RefreshToken { get; set; }

            [DataMember(Name = "error")]
            public string Error {get; set;}

            [DataMember(Name = "error_description")]
            public string ErrorDescription {get; set;}

    }
}
