using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CanvasRemindWebApp.Models;
using CanvasRemindWebApp.ParsingFiles;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Net.Http.Formatting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using System.Security.Cryptography;
using System.IO;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace CanvasRemindWebApp.Controllers
{

    public class CanvasRemindController : Controller
    {
        //Create a DbContext variable to use to interact with the Entity Framework Core DB
        private readonly CanvasRemindWebAppContext _context;
        
        //Create an IOptions variable to use as access to the Configuration JSON files
        private readonly IOptions<CanvasTestAccess> _CanvasTestAccess;
        private readonly IOptions<AESEncryption> _Aes;

        public CanvasRemindController(CanvasRemindWebAppContext context, IOptions<CanvasTestAccess> configuration, IOptions<AESEncryption> aes)
        {
            _context = context;
            _CanvasTestAccess = configuration;
            _Aes = aes;
        }

        //Return the Home Page
        public IActionResult Index()
        {
            return View();
        }

        //Return the About Page
        public IActionResult About()
        {
            return View();
        }

        //Return the Sign Up Page
        public IActionResult SignUpPage()
        {
            return View();
        }

        //Return the Thank You Page
        public IActionResult ThankYou()
        {
            string name = HttpContext.Request.Cookies["Name"];
            ViewData["Name"] = Decrypt(name);
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        //Function to add a user to the Database. This function is called on the submission of the Start Up Page form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(User user)
        {
            try
            {
                if(HttpContext.Request.Cookies[".AspNet.Consent"] == "yes")
                {
                    if (ModelState.IsValid)
                    {
                        //Stores the user name and email to be used in a later function to update the user with more information
                        user.Name = Encrypt(user.Name);
                        user.Email = Encrypt(user.Email);
                        user.Phone = Encrypt(user.Phone);
                        Response.Cookies.Append("Name", user.Name);
                        Response.Cookies.Append("Email", user.Email);
                        _context.User.Add(user);
                        await _context.SaveChangesAsync();

                        //Redirect the user to the OAuth function of this Web App
                        return OAuth();

                    }
                }
                else
                {
                    return RedirectToAction(nameof(SignUpPage));
                }



            }
            catch(DbUpdateException ex)
            {
                ModelState.AddModelError( " ", "Unable to save changes. Try again, if the problem persists see your system administrator");
            }
            return RedirectToAction(nameof(SignUpPage));

        }

        //Updates a users information with the Access Token and Refresh Token recieved from the Canvas API after OAuth has been completed
        public async Task UpdateUser(User user)
        {
            try
            {
                string name = HttpContext.Request.Cookies["Name"];
                string email = HttpContext.Request.Cookies["Email"];
                
                //Use the variables to find the first user with these specific attributes in the database 
                var dbChange =  _context.User.Where(d => d.Name == name && d.Email == email).First();

                //Update the access token and refresh token variables of the unique user
                dbChange.AccessToken = user.AccessToken;
                dbChange.RefreshToken = user.RefreshToken;

                //Save the changes to the database
                await _context.SaveChangesAsync();
                


            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(" ", "Unable to save changes. Try again, if the problem persists see your system administrator");
            }



        }

        public async Task DeleteUser(User user)
        {
            try
            {
                //Get the user name and email from the cookie 
                user.Name = HttpContext.Request.Cookies["Name"];
                user.Email = HttpContext.Request.Cookies["Email"];

                //Set a variable to be used to delete that user from the data base
                var userToDelete = _context.User.Where(d => d.Name == user.Name && d.Email == user.Email).First();
                
                _context.User.Remove(userToDelete);

                await _context.SaveChangesAsync();
                

            }
            catch(DbUpdateException ex)
            {
                ModelState.AddModelError(" ", "Unable to save changes. Try again, if the problem persists see your system administrator");
            }
        }

        //Function used to redirect a user to the Canvas OAuth2 page to authorize this application as a service they would like to use

        private IActionResult OAuth()
        {
            //Get the Client ID for this application from the Configuration JSON file
            string ClientID =  _CanvasTestAccess.Value.Client_Id;

            //Redirect the user to the proper OAuth page in order to get first part of the OAuth2 Workflow
            return Redirect("http://192.168.31.67/login/oauth2/auth?client_id=" + ClientID + "&response_type=code&redirect_uri=https://localhost:5001/canvasremind/OAuth_Completed");
        }

        //Function that gets the Access Token and Refresh Token. Redirected to after the Canvas OAuth page is interacted with. 
        public async Task<IActionResult> OAuth_Completed(string code, string error)
        {
            //Create a user variable to store access token and refresh token in. Will be passed into the UpdateUser function
            User user = new User();

            if(error == "access_denied")
            {
                await DeleteUser(user);
                return RedirectToAction(nameof(Error));
            }

            //Create an HttpClient to make web requests
            HttpClient client = new HttpClient();

            //Serializer to read incoming JSON
            var Serializer = new DataContractJsonSerializer(typeof(List<OAuth>));

            //Values that will be sent through the header during the POST request to get the Access Token and Refresh Token
            var values = new Dictionary<string, string>()
            {
                {"grant_type", "authorization_code" },
                {"client_id",  _CanvasTestAccess.Value.Client_Id},
                { "client_secret",  _CanvasTestAccess.Value.Client_Secret},
                {"redirect_uri", "https://localhost:5001/canvasremind/OAuth_Completed" },
                {"code", code }
            };

            //Form encode values so it is sent through the header to the url
            var content = new FormUrlEncodedContent(values);

            //Send a POST web request asynchronously to the Canvas API OAuth2 endpoint that returns the Access Token and Refresh Token
            var response = await client.PostAsync("http://192.168.31.67/login/oauth2/token", content);


            //Get the response in JSON format and read it in as an OAuth object <---- Created from the OAuth.cs Class in the Parsing_Files folder
            var Stream = response.Content.ReadAsAsync<OAuth>(new[] { new JsonMediaTypeFormatter() }).Result;
           
           if(Stream.AccessToken == null || Stream.RefreshToken == null)
           {
               return RedirectToAction(nameof(Error));
           }
           
            //Add the access token  and refresh token to the user variable
            user.AccessToken = Encrypt(Stream.AccessToken);
            user.RefreshToken = Encrypt(Stream.RefreshToken);



            //asynchronously update the user signing up for the service
            await UpdateUser(user);
            return RedirectToAction(nameof(ThankYou));

        }

        //Function used to encrypt the data being brought in and stored into the database using AES encryption library
        private string Encrypt(string input)
        {
            //Create the variables necessary to encrypt the data
            string EncryptedString = ""; //string to store the Base64 string that will result from encryption. This value will be returned.
            byte[] keyByteArray = Convert.FromBase64String( _Aes.Value.Key); //Convert the Base64 string into a byte array for encryption
            byte[] ivByteArray = Convert.FromBase64String( _Aes.Value.IV); //Convert the Base64 string into a byte array for encryption
            byte[] EncryptedBytes;

            //Instantiate an instance of an AES Encryption method
            using (Aes aes = Aes.Create())
            {
                //set the key to the byte array to be used as the key
                aes.Key = keyByteArray;

                //set the IV to the byte array to be used as the IV
                aes.IV = ivByteArray;

                //Create an encryptor using the new key and IV that will perform the stream transform
                ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);

                //Create the streams necessary for encryption
                using(MemoryStream mem = new MemoryStream())
                {

                    using(CryptoStream cryptStream = new CryptoStream(mem, crypto, CryptoStreamMode.Write))
                    {

                        using(StreamWriter streamWriter = new StreamWriter(cryptStream))
                        {
                            //Write the stream to the string passed in as a parameter
                            streamWriter.Write(input);
                        }
                        //Create a byte array by making the memory stream an array
                        EncryptedBytes = mem.ToArray();
                    }
                }


            }

                //Convert the encrypted bytes into a Base64 string to be used in string related database fields
                EncryptedString = Convert.ToBase64String(EncryptedBytes);

                //Return the Base64 string
                return EncryptedString;
        }


        //Function used to decrypt the necessary encrypted data using AES encryption
        private string Decrypt(string EncryptedString)
        {
            //Initialize the variables to be used 
            string DecryptedString = ""; //string to store the decrypted value
            byte[] keyByteArray = Convert.FromBase64String(_Aes.Value.Key);//Convert the Base64 string into a byte array for decryption
            byte[] ivByteArray = Convert.FromBase64String( _Aes.Value.IV); //Convert the Base64 string into a byte array for decryption
            byte[] EncryptedBytes = Convert.FromBase64String(EncryptedString); //Convert the encrypted string to a byte array for decryption


            //Instantiate a new Aes Encryption method
            using(Aes aes = Aes.Create())
            {
                //Set the key to the byte array of key
                aes.Key = keyByteArray;

                //set the IV to the byte array of the IV
                aes.IV = ivByteArray;

                //Create a decryptor using the Key and IV to perform the stream transform
                ICryptoTransform decrypt = aes.CreateDecryptor(aes.Key, aes.IV);

                //Create the necessary streams for decryption
                using(MemoryStream mem = new MemoryStream(EncryptedBytes))
                {
                    using(CryptoStream crypt = new CryptoStream(mem, decrypt, CryptoStreamMode.Read))
                    {
                        using(StreamReader reader = new StreamReader(crypt))
                        {
                            //Place the decrypted string into a variable
                            DecryptedString = reader.ReadToEnd();
                        }
                    }

                }

            }

            //Return the decrypted string
            return DecryptedString;
        }


        private string GetParameterFromAWS(string parameterName)
        {   
            var ssmClient = new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.USEast2);
            var response = ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = true
                
            });



            return response.ToString();
        }


    }
}

