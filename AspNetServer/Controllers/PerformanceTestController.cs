using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AspNetServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Shared;

namespace AspNetServer.Controllers
{
	[Route("")]
	public class PerformanceTestController : Controller
	{
		private const int _CONTENT_SIZE_IN_MB = 20;
		private static readonly byte[] _bytes;

		static PerformanceTestController()
		{
			_bytes = Helpers.GetRandomBytes(_CONTENT_SIZE_IN_MB);
		}

		[HttpGet("/")]
		public IActionResult Ping()
		{
			return Json("OK");
		}

		[HttpPost("Upload")]
		public async Task<IActionResult> Upload()
		{
			var sw = Stopwatch.StartNew();

			var bufferSize = 4 * 1024;
			var totalBytes = await Helpers.ReadStream(Request.Body, bufferSize);

			sw.Stop();
			Helpers.PrintDuration("Upload", totalBytes, sw.Elapsed);

			return Ok();
		}

		[HttpPost("UploadMultipartUsingIFormFile")]
		public async Task<IActionResult> UploadMultipartUsingIFormFile(UploadMultipartModel model)
		{
			var sw = Stopwatch.StartNew();

			var bufferSize = 32 * 1024;
			var totalBytes = await Helpers.ReadStream(model.File.OpenReadStream(), bufferSize);

			sw.Stop();
			Helpers.PrintDuration($"UploadMultipartUsingIFormFile with Value={model.SomeValue}", totalBytes, sw.Elapsed);

			return Ok();
		}

		[HttpPost("UploadMultipartUsingReader")]
		public async Task<IActionResult> UploadMultipartUsingReader()
		{
			var sw = Stopwatch.StartNew();

			var boundary = GetBoundary(Request.ContentType);
			var reader = new MultipartReader(boundary, Request.Body, 80 * 1024);

			var valuesByKey = new Dictionary<string, string>();
			var totalBytes = 0;
			MultipartSection section;

			while ((section = await reader.ReadNextSectionAsync()) != null)
			{
				var contentDispo = section.GetContentDispositionHeader();

				if (contentDispo.IsFileDisposition())
				{
					var fileSection = section.AsFileSection();
					var bufferSize = 32 * 1024;
					totalBytes = await Helpers.ReadStream(fileSection.FileStream, bufferSize);
				}
				else if (contentDispo.IsFormDisposition())
				{
					var formSection = section.AsFormDataSection();
					var value = await formSection.GetValueAsync();
					valuesByKey.Add(formSection.Name, value);
				}
			}

			sw.Stop();
			Helpers.PrintDuration($"UploadMultipartUsingReader with Value={valuesByKey["SomeValue"]}", totalBytes, sw.Elapsed);

			return Ok();
		}

		[HttpGet("Download")]
		public IActionResult Download(bool useCustomFileResult)
		{
			var stream = new MemoryStream(_bytes);
			var contentType = "application/octet-stream";

			return useCustomFileResult
				? (IActionResult) new CustomFileResult(stream, contentType)
				: File(stream, contentType);
		}


		private static string GetBoundary(string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException(nameof(contentType));

			var elements = contentType.Split(' ');
			var element = elements.First(entry => entry.StartsWith("boundary="));
			var boundary = element.Substring("boundary=".Length);

			boundary = HeaderUtilities.RemoveQuotes(boundary);

			return boundary;
		}
	}
}