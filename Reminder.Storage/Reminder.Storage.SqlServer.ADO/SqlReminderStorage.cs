﻿using Reminder.Storage.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace Reminder.Storage.SqlServer.ADO
{
    public class SqlReminderStorage : IReminderStorage
    {
        private readonly string _connectionString;

        public SqlReminderStorage(string connectionString)
        {
            _connectionString = connectionString; 
        }

        public Guid Add(ReminderItemRestricted reminder)
        {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[AddReminderItem]";

                cmd.Parameters.AddWithValue("@contactId", reminder.ContactId);
                cmd.Parameters.AddWithValue("@targetDate", reminder.Date);
                cmd.Parameters.AddWithValue("@message", reminder.Message);
                cmd.Parameters.AddWithValue("@statusId", (byte)reminder.Status);

                var reminderIdParameter = new SqlParameter("@reminderId", SqlDbType.UniqueIdentifier);
                reminderIdParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(reminderIdParameter);

                cmd.ExecuteNonQuery();

                return (Guid)reminderIdParameter.Value;
            }
        }

        public ReminderItem Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public List<ReminderItem> Get(ReminderItemStatus status)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatus(IEnumerable<Guid> ids, ReminderItemStatus status)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatus(Guid id, ReminderItemStatus status)
        {
            throw new NotImplementedException();
        }

        private SqlConnection GetOpenedSqlConnection()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}
