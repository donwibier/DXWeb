using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DX.Test.Web.Blazor.Data
{
	public class RegisterUserModel
	{
		public RegisterUserModel() { }
		[Required()]
		[EmailAddress()]
		public string EmailAddress { get; set; }
		//[RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).{6,20}$", ErrorMessage = "Your password is not secure enough")]
		[Required()]		
		public string Password { get; set; }
		[Compare("Password", ErrorMessage ="Passwords must match")]
		public string PasswordConfirm { get; set; }
	
		[Required()]
		public DateTime BirthDate { get; set; }
		public string Street { get; set; }
		public string HouseNo { get; set; }
		public string HouseNoSuffix { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }

		public string ReturnUrl { get; set; }

	}

}
