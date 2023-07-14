USE ["DATABASE"]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER TRIGGER ["SCHEMA"].[Trigger_"TABLE"Changes]
ON ["SCHEMA"].["TABLE"] -- Replace "Table1" with the actual table name where you want to track changes
AFTER UPDATE, DELETE, INSERT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN
        DECLARE @ChangeType CHAR(1);
		DECLARE @ChangedRecord NVARCHAR(MAX);
        
		IF (EXISTS (SELECT TOP 1 * FROM inserted) AND EXISTS (SELECT TOP 1 * FROM deleted))
			BEGIN
				SET @ChangeType = 'U'; -- Update
				SELECT @ChangedRecord = COALESCE( (SELECT TOP 1 * FROM inserted FOR JSON AUTO), 'U')
			END
		ELSE IF EXISTS (SELECT * FROM inserted)
			BEGIN
				SET @ChangeType = 'I'; -- Insert	
				SELECT @ChangedRecord = COALESCE( (SELECT TOP 1 * FROM inserted FOR JSON AUTO), 'I')
			END
		ELSE IF EXISTS (SELECT * FROM deleted)
			BEGIN
				SET @ChangeType = 'D'; -- Delete
				SELECT @ChangedRecord = COALESCE( (SELECT TOP 1 * FROM deleted FOR JSON AUTO), 'D')
			END
		ELSE
			BEGIN
				SET @ChangeType = 'E'; -- Error
				SET @ChangedRecord = ''
			END

	END

	INSERT INTO [dbo].[ChangedRecordsQueue] ( [ChangeType], [RecordTable], [RecordValue] )
		VALUES ( @ChangeType, '"TABLE"', @ChangedRecord );
END

GO