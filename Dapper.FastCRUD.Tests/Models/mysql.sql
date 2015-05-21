
CREATE TABLE `Employee` (
	UserId int NOT NULL AUTO_INCREMENT,
    EmployeeId CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	KeyPass CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	LastName nvarchar(50) NOT NULL,
	FirstName nvarchar(50) NOT NULL,
	BirthDate datetime NOT NULL,
    WorkstationId int NULL,
	PRIMARY KEY (UserId, EmployeeId)
);

ALTER TABLE Employee auto_increment=2;

CREATE TRIGGER `Employee_Assign_UUID`
  BEFORE INSERT ON Employee
  FOR EACH ROW
  SET NEW.EmployeeId = UUID(),
  New.KeyPass = UUID();
