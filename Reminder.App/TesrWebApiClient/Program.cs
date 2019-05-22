using System;
using Reminder.Storage.Core;
using Reminder.Storage.WebApi.Client;
using Reminder.Storage.WebApi.Core;


namespace TesrWebApiClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new ReminderStorageWebApiClient("https://localhost:5001");
			var reminderItem = new ReminderItemRestricted
			{
				ContactId = "TastContact",
				Date = DateTimeOffset.Now,
				Message = "TestMessage"
			};

			var id = client.Add(reminderItem);

			Console.WriteLine("Adding done!");

			var reminderItemFromStorage = client.Get(id);

			Console.WriteLine(
				"Reading done:\n" + 
				$"{reminderItemFromStorage.Id}\n" +
				$"{reminderItemFromStorage.ContactId}\n" + 
				$"{reminderItemFromStorage.Date}\n" + 
				$"{reminderItemFromStorage.Message}\n");

            Console.ReadKey();
		}
	}
}
