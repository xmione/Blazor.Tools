-- ================================================
-- Template generated from Template Explorer using:
-- Create Trigger (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- See additional Create Trigger templates for more
-- examples of different Trigger statements.
--
-- This block of comments will not be included in
-- the definition of the function.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ========================================================
-- Author:		Solomio S. sisante
-- Create date: May 16, 2024
-- Description:	Evaluates the minimum quantity by location
-- ========================================================
CREATE TRIGGER tr_PS_DOC_LIN_After_Insert_Update_Delete
ON PS_DOC_LIN
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DECLARE @minQty INT, @soldQty INT, @orderQty INT

    -- Loop through all the rows in the inserted table
    DECLARE cur CURSOR FOR SELECT STK_LOC_ID, DOC_ID, LIN_SEQ_NO, QTY_SOLD FROM inserted
    DECLARE @stk_loc_id INT, @doc_id INT, @lin_seq_no INT, @qty_sold INT
    OPEN cur
    FETCH NEXT FROM cur INTO @stk_loc_id, @doc_id, @lin_seq_no, @qty_sold
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @minQty = MIN_QTY FROM IM_INV WHERE LOC_ID = @stk_loc_id
        SET @soldQty = @qty_sold;

        IF @soldQty > @minQty
        BEGIN
            SET @orderQty = @soldQty - @minQty

            INSERT INTO USER_SUGGESTED_REPLENISHMENT (DOC_ID, LIN_SEQ_NO, SUGGESTED_QTY)
            VALUES (@doc_id, @lin_seq_no, @orderQty)
        END

        FETCH NEXT FROM cur INTO @stk_loc_id, @doc_id, @lin_seq_no, @qty_sold
    END
    CLOSE cur
    DEALLOCATE cur
END

GO
