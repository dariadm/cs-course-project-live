DROP PROCEDURE IF EXISTS [dbo].[AddReminderItem]
GO
CREATE PROCEDURE [dbo].[AddReminderItem](
	@contactId VARCHAR(50),
	@targetDate DATETIMEOFFSET,
	@message NVARCHAR(200),
	@statusId TINYINT, 
	@reminderId UNIQUEIDENTIFIER OUTPUT)
AS BEGIN
	SET NOCOUNT ON

	DECLARE 
		@tempReminderItemId AS UNIQUEIDENTIFIER,
		@now AS DATETIMEOFFSET

			
	SELECT
		@tempReminderItemId = NEWID(),
		@now = SYSDATETIMEOFFSET();		

	INSERT INTO [dbo].[ReminderItem](
		[Id],
		[ContactId],
		[TargetDate],
		[Message],
		[StatusId],
		[CreatedDate],
		[UPdatedDate])
	VALUES(
		@tempReminderItemId,
		@contactId,
		@targetDate,
		@message,
		@statusId,
		@now,
		@now)
	
	SET	@reminderId = @tempReminderItemId
END
GO

DROP PROCEDURE IF EXISTS [dbo].[GetReminderItemById]
GO
CREATE PROCEDURE [dbo].[GetReminderItemById](
	@reminderId AS UNIQUEIDENTIFIER
)
AS BEGIN
	SELECT
		[Id],
		[ContactId],
		[TargetDate],
		[Message],
		[StatusId]
	FROM
		[dbo].[ReminderItem]
	WHERE [Id] = @reminderId
END
GO

DROP PROCEDURE IF EXISTS [dbo].[GetReminderItemByStatus]
GO
CREATE PROCEDURE [dbo].[GetReminderItemByStatus](
	@reminderItemStatus AS TINYINT
)
AS BEGIN 
	SELECT
		[Id],
		[ContactId],
		[TargetDate],
		[Message],
		[StatusId]
	FROM
		[dbo].[ReminderItem]
	WHERE [StatusId] = @reminderItemStatus
END
GO

DROP PROCEDURE IF EXISTS [dbo].[UpdateReminderItemStatusById]
GO
CREATE PROCEDURE [dbo].[UpdateReminderItemStatusById](
	@reminderId AS UNIQUEIDENTIFIER,
	@statusId TINYINT)
AS BEGIN
	SET NOCOUNT ON

	UPDATE [dbo].[ReminderItem]
		SET [StatusId] = @statusId,
			[UpdatedDate] = SYSDATETIMEOFFSET()
	WHERE [Id] = @reminderId
END
GO
			
DROP PROCEDURE IF EXISTS [dbo].[GetReminderItemsCount]
GO
CREATE PROCEDURE [dbo].[GetReminderItemsCount]
AS BEGIN
	SELECT COUNT(*) AS [Count] 
	FROM [dbo].[ReminderItem]
END 
GO

DROP PROCEDURE IF EXISTS [dbo].[RemoveReminderItem]
GO
CREATE PROCEDURE [dbo].[RemoveReminderItem](
	@reminderId AS UNIQUEIDENTIFIER)
AS BEGIN
	SET NOCOUNT ON

	DELETE
	FROM
		[dbo].[ReminderItem]
	WHERE [Id] = @reminderId

	SELECT CAST (@@ROWCOUNT AS BIT)
END
GO

DROP PROCEDURE IF EXISTS [dbo].[UpdateReminderItemsBulk]
GO
CREATE PROCEDURE [dbo].[UpdateReminderItemsBulk](
	@statusId AS TINYINT
)
AS BEGIN
	UPDATE R 
		SET R.[StatusId] = @statusId,
		R.[UpdatedDate] = SYSDATETIMEOFFSET()
	FROM [dbo].[ReminderItem] AS R
	INNER JOIN #ReminderItem AS T
			ON T.Id = R.Id
END 
GO