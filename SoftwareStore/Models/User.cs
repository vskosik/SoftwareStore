﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SoftwareStore.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		[DisplayName("First Name")]
		public string FirstName { get; set; }
		[DisplayName("Last Name")]
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
	}
}
