using System;
using Reminder.Storage.WebApi.Client;
using Reminder.Storage.WebApi.Core;


namespace TesrWebApiClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new ReminderStorageWebApiClient("https://localhost:5001");
			var reminderItem = new ReminderItem
			{
				ContactId = "TastContact",
				Date = DateTimeOffset.Now,
				Message = "TestMessage"
			};

			client.Add(reminderItem);

			Console.WriteLine("Hello World!");
		}
	}
}
