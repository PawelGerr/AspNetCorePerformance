using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared
{
	public static class Helpers
	{
		public static void PrintDuration(string methodName, int contentLength, TimeSpan duration)
		{
			var sizeInMb = contentLength / 1024 / 1024;
			Console.WriteLine($"{methodName} of {sizeInMb} MB took {duration}. Speed = {sizeInMb / duration.TotalSeconds:F} MB/s");
		}

		public static byte[] GetRandomBytes(int sizeInMb)
		{
			var size = sizeInMb * 1024 * 1024;
			var bytes = new byte[size];
			var random = new Random(0);
			random.NextBytes(bytes);

			return bytes;
		}

		public static async Task<int> ReadStream(Stream stream, int bufferSize)
		{
			var buffer = new byte[bufferSize];

			int bytesRead;
			int totalBytes = 0;

			do
			{
				bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);
				totalBytes += bytesRead;
			} while (bytesRead > 0);
			return totalBytes;
		}
	}
}