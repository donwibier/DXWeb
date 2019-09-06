using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DX.Test.Web.Blazor.Data
{
	public class RegisterUser
	{
		
		public RegisterUser()
		{
			
		}

		[Required()]
		[EmailAddress()]
		public string EmailAddress { get; set; }
		//[RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).{6,20}$", ErrorMessage = "Your password is not secure enough")]
		[Required()]		
		public string Password { get; set; }
		[Compare("Password", ErrorMessage ="Passwords must match")]
		public string PasswordConfirm { get; set; }
	}
	public class UserLogin
	{		
		public UserLogin()
		{
			
		}
		public string Username { get; set; }
		public string Password { get; set; }
		public bool Remember { get; set; }
	}
}
