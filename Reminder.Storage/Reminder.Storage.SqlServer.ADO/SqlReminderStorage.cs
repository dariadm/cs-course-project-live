using Reminder.Storage.Core;
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

        public List<ReminderItem> Get(int count = 0, int startPosition = 0)
        {
            var result = new List<ReminderItem>();
            result.AddRange(Get(ReminderItemStatus.Awaiting));
            result.AddRange(Get(ReminderItemStatus.Failed));
            result.AddRange(Get(ReminderItemStatus.Ready));
            result.AddRange(Get(ReminderItemStatus.Sent));

            return result;
        }

        public ReminderItem Get(Guid id)
        {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[GetReminderItemById]";

                cmd.Parameters.AddWithValue("@reminderId", id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows || !reader.Read())
                    {
                        return null;
                    }

                    var result = new ReminderItem();

                    result.Id = id;
                    result.ContactId = reader.GetString(reader.GetOrdinal("ContactId"));
                    result.Date = reader.GetDateTimeOffset(reader.GetOrdinal("TargetDate"));
                    result.Message = reader.GetString(reader.GetOrdinal("Message"));
                    result.Status = (ReminderItemStatus)reader.GetByte(reader.GetOrdinal("StatusId"));

                    return result;
                }
            }
        }

        public List<ReminderItem> Get(ReminderItemStatus status)
        {
            var result = new List<ReminderItem>();

            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[GetReminderItemByStatus]";

                cmd.Parameters.AddWithValue("@reminderItemStatus", (byte)status);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return result;
                    }

                    int idColumnIndex = reader.GetOrdinal("Id");
                    int contactIdColumnIndex = reader.GetOrdinal("ContactId");
                    int dateColumnIndex = reader.GetOrdinal("TargetDate");
                    int messageColumnIndex = reader.GetOrdinal("Message");
                    int statusColumnIndex = reader.GetOrdinal("StatusId");

                    while (reader.Read())
                    {
                        var newReminderItem = new ReminderItem
                        {
                            Id = reader.GetGuid(idColumnIndex),
                            ContactId = reader.GetString(contactIdColumnIndex),
                            Date = reader.GetDateTimeOffset(dateColumnIndex),
                            Message = reader.GetString(messageColumnIndex),
                            Status = (ReminderItemStatus)reader.GetByte(statusColumnIndex)
                        };

                        result.Add(newReminderItem);
                    }
                    return result;
                }
            }
        }

        public void UpdateStatus(IEnumerable<Guid> ids, ReminderItemStatus status)
        {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "CREATE TABLE #ReminderItem ([Id] UNIQUEIDENTIFIER NOT NULL)";
                cmd.ExecuteNonQuery();

                //Insert data
                using (SqlBulkCopy copy = new SqlBulkCopy(sqlConnection))
                {
                    copy.BatchSize = 1000;
                    copy.DestinationTableName = "#ReminderItem";

                    DataTable tempTable = new DataTable("#ReminderItem");

                    tempTable.Columns.Add("Id", typeof(Guid));

                    foreach(Guid id in ids)
                    {
                        DataRow row = tempTable.NewRow();
                        row["Id"] = id;
                        tempTable.Rows.Add(row);
                    }

                    copy.WriteToServer(tempTable);
                }

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[UpdateReminderItemsBulk";
                cmd.Parameters.AddWithValue("@statusId", (byte)status);
                cmd.ExecuteNonQuery();

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DROP TABLE #ReminderItem";
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateStatus(Guid id, ReminderItemStatus status)
        {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[UpdateReminderItemStatusById]";

                cmd.Parameters.AddWithValue("@reminderId", id);
                cmd.Parameters.AddWithValue("@statusId", (byte)status);

                cmd.ExecuteNonQuery();
            }
        }

        public int Count
        {
            get
            {
                using (var sqlConnection = GetOpenedSqlConnection())
                {
                    var cmd = sqlConnection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[GetReminderItemsCount]";

                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool Remove(Guid id)
        {
            using (var sqlConnection = GetOpenedSqlConnection())
            {
                var cmd = sqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[RemoveReminderItem]";

                cmd.Parameters.AddWithValue("@reminderId", id);

                return (bool)cmd.ExecuteScalar();
            }
        }


        private SqlConnection GetOpenedSqlConnection()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}
