using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reminder.Storage.Core;
using Reminder.Storage.SqlServer.ADO.Tests.Properties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reminder.Storage.SqlServer.ADO.Tests
{
    [TestClass]
    public class SqlReminderStorageTests
    {
        private const string _connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=ReminderTests;Integrated Security=True";
                   // @"Data Source=localhost\SQLEXPRESS; Initial Catalog=ReminderTests;Integrated Security=True";

        [TestInitialize]
        public void TestInitialize()
        {
            new SqlReminderStorageInit(_connectionString).InitializeDatabase(); 
        }

        [TestMethod]
        public void Method_Add_Returns_Not_Empty_Guid()
        {
            var storage = new SqlReminderStorage(_connectionString);

            Guid actual = storage.Add(new Core.ReminderItemRestricted
            {
                ContactId = "TestContactdId",
                Date = DateTimeOffset.Now.AddHours(1),
                Message = "Test Message",
                Status = Core.ReminderItemStatus.Awaiting
            });

            Assert.AreNotEqual(Guid.Empty, actual);
        }

        [TestMethod]
        public void Get_By_Id_Method_Returns_Just_Added_Item()
        {
            var storage = new SqlReminderStorage(_connectionString);

            DateTimeOffset expectedDate = DateTimeOffset.Now;
            string expectedContactId = "TEST_CONTACT_ID";
            string expectedMessage = "TEST_MESSAGE_TEXT";
            ReminderItemStatus expectedStatus = ReminderItemStatus.Awaiting;

            Guid id = storage.Add(new ReminderItemRestricted
            {
                ContactId = expectedContactId,
                Date = expectedDate,
                Message = expectedMessage,
                Status = expectedStatus
            });

            Assert.AreNotSame(Guid.Empty, id);

            var actualItem = storage.Get(id);

            Assert.IsNotNull(actualItem);
            Assert.AreEqual(id, actualItem.Id);
            Assert.AreEqual(expectedContactId, actualItem.ContactId);
            Assert.AreEqual(expectedDate, actualItem.Date);
            Assert.AreEqual(expectedMessage, actualItem.Message);
            Assert.AreEqual(expectedStatus, actualItem.Status);
        }

        [TestMethod]
        public void Get_By_Id_Returns_Null_If_Not_Found()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actual = storage.Get(Guid.Empty);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void Get_By_Status_0_Returns_Valid_Number_Of_Items_For_Each_Status()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actualItemList = storage.Get(ReminderItemStatus.Awaiting);

            Assert.AreEqual(1, actualItemList.Count);
        }

        [TestMethod]
        public void Get_By_Status_2_Returns_Valid_Number_Of_Items_For_Each_Status()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actualItemList = storage.Get(ReminderItemStatus.Sent);

            Assert.AreEqual(2, actualItemList.Count);
        }

        [TestMethod]
        public void Get_By_Status_3_Returns_Valid_Number_Of_Items_For_Each_Status()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actualItemList = storage.Get(ReminderItemStatus.Failed);

            Assert.AreEqual(0, actualItemList.Count);
        }

        [TestMethod]
        public void Get_By_Status_1_Returns_Valid_Number_Of_Items_For_Each_Status()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actualItemList = storage.Get(ReminderItemStatus.Ready);

            Assert.AreEqual(0, actualItemList.Count);
        }

        [TestMethod]
        public void Get_By_Status_255_Returns_Empty_List()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var actualItemList = storage.Get((ReminderItemStatus)255);

            Assert.IsNotNull(actualItemList);
            Assert.AreEqual(0, actualItemList.Count);
        }

        [TestMethod]
        public void Update_Status_1_Returns_Valid_Status()
        {
            var storage = new SqlReminderStorage(_connectionString);

            Guid id = new Guid("00000000-0000-0000-0000-111111111111");

            storage.UpdateStatus(id, ReminderItemStatus.Ready);

            ReminderItem reminderItem = storage.Get(id);

            Assert.AreEqual(reminderItem.Status, ReminderItemStatus.Ready);
        }

        [TestMethod]
        public void Property_Count_Returns_Three_For_Initial_DataSet()
        {
            var storage = new SqlReminderStorage(_connectionString);

            int actual = storage.Count;

            Assert.AreEqual(3, actual);
        }

        [TestMethod]
        public void Remove_Returns_1_If_NExists()
        {
            var storage = new SqlReminderStorage(_connectionString);

            Guid id = new Guid("00000000-0000-0000-0000-111111111111");

            bool result = storage.Remove(id);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Remove_Returns_0_If_Not_Exist()
        {
            var storage = new SqlReminderStorage(_connectionString);

            Guid id = new Guid("00000000-8888-0000-0000-111111111111");

            bool result = storage.Remove(id);

            Assert.AreEqual(false, result);
       }

        [TestMethod]
        public void Update_Status_Method_With_Ids_Collection_Updates_Corresponded_Items()
        {
            var storage = new SqlReminderStorage(_connectionString);

            var ids = new List<Guid>
            {
                new Guid("00000000-0000-0000-0000-111111111111"),
                new Guid("00000000-0000-0000-0000-333333333333")
            };

            storage.UpdateStatus(ids, ReminderItemStatus.Failed);

            var actual =  storage.Get(ReminderItemStatus.Failed);

            Assert.IsTrue(actual.Select(x => x.Id).Contains([ids[0]));
            Assert.IsTrue(actual.Select(x => x.Id).Contains([ids[1]));
        }
    }
}
