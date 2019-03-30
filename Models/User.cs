using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace CanvasRemindWebApp.Models
{
    public class User
    {
        //User model that will be used to create a new user and display currently logged in user - Link courses and assignments table to this model

        //User ID
        public int Id { get; set; }

        //Name of User
        [Required(ErrorMessage = "A Name is Required to Sign Up")]
        public string Name { get; set; }

        //User Email Address
        [Required(ErrorMessage = "An Email Address is Required to Sign Up")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        //User Phone Number
        [Required(ErrorMessage = "A Phone Number Is Required. After All it is the Point of This Service!")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("([0-9]{3})?([0-9]{3})?([0-9]{4})", ErrorMessage = "Sorry but that isnt the correct format try ##########")]
        public string Phone { get; set; }

        //string that will returned for the phone numbers service provider
        [Required(ErrorMessage = "We need to know your service provider in order to send you a text!")]
        public string ServiceProvider {get; set;}

        //User Access Token From OAuth
        public string AccessToken { get; set; }

        //OAuth Refresh Token
        public string RefreshToken { get; set; }


    }
}
