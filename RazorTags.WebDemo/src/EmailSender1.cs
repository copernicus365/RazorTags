using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorTags.AspCoreWebDemo
{
	public interface IEmailSender1 : IDisposable
	{
		void SendACoolEmail();
	}

	public class EmailSender1 : IEmailSender1
	{
		public void Dispose()
		{
			int minute = DateTime.UtcNow.Minute;
			if(minute > 1)
				Console.WriteLine($"Hey I'm disposing, and the minute was greater than 1 ({minute}).");
		}

		public void SendACoolEmail()
		{
			Console.WriteLine("Email sent! (or not)");
		}
	}
}
