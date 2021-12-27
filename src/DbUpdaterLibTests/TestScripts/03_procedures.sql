USE DatabaseName;

/*
A multiline comment
*/

# CALL TestTableGet(21);
# SELECT * FROM UsrUserSessions;
DELIMITER $$
DROP PROCEDURE IF EXISTS TestTableGet;
$$
CREATE PROCEDURE TestTableGet(
  _TestTableID MEDIUMINT UNSIGNED
)
proc_label:BEGIN
  DECLARE something VARBINARY(16);
  SELECT * FROM vTestTable WHERE TestTableID=_TestTableID;
END
$$
DELIMITER ;

DELIMITER $$
DROP PROCEDURE IF EXISTS TestTableGet2;
$$
CREATE PROCEDURE TestTableGet2(
  _TestTableID MEDIUMINT UNSIGNED
)
proc_label:BEGIN
  DECLARE something VARBINARY(16);
  SELECT * FROM vTestTable WHERE TestTableID=_TestTableID;
  SELECT * FROM vTestTable WHERE TestTableID=_TestTableID;
  SELECT * FROM vTestTable WHERE TestTableID=_TestTableID;
END
$$
DELIMITER ;
