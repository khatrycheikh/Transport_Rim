CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Companies` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Phone` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Address` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Pending',
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Companies` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Buses` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CompanyId` int NOT NULL,
    `BusNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Capacity` int NOT NULL,
    CONSTRAINT `PK_Buses` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Buses_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `PhoneNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Role` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `CompanyId` int NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Users_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Trips` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `BusId` int NOT NULL,
    `DepartureCity` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `ArrivalCity` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Date` datetime(6) NOT NULL,
    `Time` time(6) NOT NULL,
    `Price` decimal(18,2) NOT NULL,
    `AvailableSeats` int NOT NULL,
    CONSTRAINT `PK_Trips` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Trips_Buses_BusId` FOREIGN KEY (`BusId`) REFERENCES `Buses` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Reservations` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `TripId` int NOT NULL,
    `SeatNumber` int NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Pending',
    `BookingDate` datetime(6) NOT NULL,
    CONSTRAINT `PK_Reservations` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reservations_Trips_TripId` FOREIGN KEY (`TripId`) REFERENCES `Trips` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Payments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ReservationId` int NOT NULL,
    `Amount` decimal(18,2) NOT NULL,
    `Method` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `TransactionId` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Pending',
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Payments_Reservations_ReservationId` FOREIGN KEY (`ReservationId`) REFERENCES `Reservations` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Tickets` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ReservationId` int NOT NULL,
    `QrCodeData` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `GeneratedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Tickets` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Tickets_Reservations_ReservationId` FOREIGN KEY (`ReservationId`) REFERENCES `Reservations` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Buses_BusNumber` ON `Buses` (`BusNumber`);

CREATE INDEX `IX_Buses_CompanyId` ON `Buses` (`CompanyId`);

CREATE UNIQUE INDEX `IX_Payments_ReservationId` ON `Payments` (`ReservationId`);

CREATE UNIQUE INDEX `IX_Payments_TransactionId` ON `Payments` (`TransactionId`);

CREATE INDEX `IX_Reservations_TripId` ON `Reservations` (`TripId`);

CREATE INDEX `IX_Reservations_UserId` ON `Reservations` (`UserId`);

CREATE UNIQUE INDEX `IX_Tickets_ReservationId` ON `Tickets` (`ReservationId`);

CREATE INDEX `IX_Trips_BusId` ON `Trips` (`BusId`);

CREATE INDEX `IX_Users_CompanyId` ON `Users` (`CompanyId`);

CREATE UNIQUE INDEX `IX_Users_PhoneNumber` ON `Users` (`PhoneNumber`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260624192625_InitialCreateWithPhoneSchema', '8.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE `Reservations` RENAME COLUMN `SeatNumber` TO `ReservedSeats`;

ALTER TABLE `Reservations` RENAME COLUMN `BookingDate` TO `CreatedAt`;

ALTER TABLE `Reservations` ADD `TotalPrice` decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260625134541_UpdateReservationTableSchema', '8.0.10');

COMMIT;

START TRANSACTION;

CREATE TABLE `Notifications` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Recipient` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Message` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
    `SentAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Notifications_UserId` ON `Notifications` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260625161457_AddNotificationsTable', '8.0.10');

COMMIT;

