INSERT INTO [dbo].[ReminderItem] (
	[Id] ,
	[ContactId],
	[TargetDate],
	[Message], 
	StatusId ,
	[CreatedDate],
	[UpdatedDate])
VALUES(
	'00000000-0000-0000-0000-111111111111',
	'ContactId_1',
	'2020-01-01 00:00:00 +00:00',
	'Message_1',
	0,
	'2019-01-01 00:00:00 +00:00',
	'2019-01-01 00:00:00 +00:00')
INSERT INTO [dbo].[ReminderItem] (
	[Id] ,
	[ContactId],
	[TargetDate],
	[Message], 
	StatusId ,
	[CreatedDate],
	[UpdatedDate])
VALUES(
	'00000000-0000-0000-0000-222222222222',
	'ContactId_2',
	'2020-02-02 00:00:00 +00:00',
	'Message_2',
	2,
	'2029-02-02 00:00:00 +00:00',
	'2029-02-02 00:00:00 +00:00')
INSERT INTO [dbo].[ReminderItem] (
	[Id] ,
	[ContactId],
	[TargetDate],
	[Message], 
	StatusId ,
	[CreatedDate],
	[UpdatedDate])
VALUES(
	'00000000-0000-0000-0000-333333333333',
	'ContactId_3',
	'3030-03-03 00:00:00 +00:00',
	'Message_3',
	2,
	'3039-03-03 00:00:00 +00:00',
	'3039-03-03 00:00:00 +00:00')