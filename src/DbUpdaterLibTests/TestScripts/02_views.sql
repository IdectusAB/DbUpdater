############################################################################################
# VIEWS
############################################################################################

USE DatabaseName;

# Just a view for testing
DROP VIEW IF EXISTS vTestTable;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW vTestTable AS 
SELECT
  tt.TestTableID,
  tt.SomeText,
  tt.Created
FROM 
  TestTable tt;
