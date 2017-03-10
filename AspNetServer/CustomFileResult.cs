using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AspNetServer
{
	public class CustomFileResult : IActionResult
	{
		private readonly Stream _stream;
		private readonly string _contentType;
		private readonly string _fileDownloadName;
		private readonly int _bufferSize;

		public CustomFileResult(Stream stream, string contentType, string fileDownloadName = null, int bufferSize = 64 * 1024)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (contentType == null)
				throw new ArgumentNullException(nameof(contentType));

			_stream = stream;
			_contentType = contentType;
			_fileDownloadName = fileDownloadName;
			_bufferSize = bufferSize;
		}

		public async Task ExecuteResultAsync(ActionContext context)
		{
			context.HttpContext.Response.ContentType = _contentType;

			if (_stream.CanSeek)
				context.HttpContext.Response.ContentLength = _stream.Length;

			if (!String.IsNullOrWhiteSpace(_fileDownloadName))
			{
				var dispositionHeaderValue = new ContentDispositionHeaderValue("attachment");
				dispositionHeaderValue.SetHttpFileName(_fileDownloadName);
				context.HttpContext.Response.Headers["Content-Disposition"] = dispositionHeaderValue.ToString();
			}

			var body = context.HttpContext.Response.Body;

			try
			{
				await _stream.CopyToAsync(body, _bufferSize);
			}
			finally
			{
				_stream.Dispose();
			}
		}
	}
}