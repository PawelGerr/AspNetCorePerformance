using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetServer.Models
{
	public class UploadMultipartModel
	{
		public IFormFile File { get; set; }
		public int SomeValue { get; set; }
	}
}