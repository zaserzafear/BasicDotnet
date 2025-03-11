START TRANSACTION;

DROP TABLE `RolePermissions`;

DROP TABLE `Permissions`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '20250311015455_SeedRolePermissions';

COMMIT;

START TRANSACTION;

DROP TABLE `Admins`;

DROP TABLE `Customers`;

DROP TABLE `SuperAdmins`;

DELETE FROM `Roles`
WHERE `Id` = 1;
SELECT ROW_COUNT();


DELETE FROM `Roles`
WHERE `Id` = 2;
SELECT ROW_COUNT();


DELETE FROM `Roles`
WHERE `Id` = 3;
SELECT ROW_COUNT();


CREATE TABLE `Users` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UserName` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserRoles` (
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    `RoleId` int NOT NULL,
    CONSTRAINT `PK_UserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '20250306023844_SeparateTableUserByRole';

COMMIT;

