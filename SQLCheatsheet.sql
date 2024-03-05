select [ComputerId],
    [Motherboard],
    ISNULL([CPUCores],4) as CPUCores,
    [HasWifi],
    [HasLTE],
    [ReleaseDate],
    [Price],
    [VideoCard]
from TutorialAppSchema.Computer
ORDER BY HasWifi DESC, ReleaseDate DESC
;

SELECT [Users].[UserId],
    [Users].[FirstName] + ' ' + [Users].[LastName] as FullName,
    [Users].[Email],
    [Users].[Gender],
    [Users].[Active],
    [UserJobInfo].[JobTitle],
    [UserJobInfo].[Department]
FROM TutorialAppSchema.Users as Users
    JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
    ON UserJobInfo.UserId = Users.UserId
WHERE Users.Active = 1
ORDER BY Users.UserId DESC

SELECT [Users].[UserId],
    [Users].[FirstName] + ' ' + [Users].[LastName] as FullName,
    [Users].[Email],
    [Users].[Gender],
    [Users].[Active],
    [UserJobInfo].[JobTitle],
    [UserJobInfo].[Department]
FROM TutorialAppSchema.Users as Users
    JOIN TutorialAppSchema.UserSalary as UserSalary
    ON UserSalary.UserId = Users.UserId
    LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
    ON UserJobInfo.UserId = Users.UserId
WHERE Users.Active = 1
ORDER BY Users.UserId DESC

SELECT ISNULL([UserJobInfo].[Department], 'No department listed') as Department,
    SUM([UserSalary].[Salary]) as Salary,
    MIN([UserSalary].[Salary]) as minSalary,
    MAX([UserSalary].[Salary]) as maxSalary,
    AVG([UserSalary].[Salary]) as AvgSalary,
    COUNT(*) as PeopleFromDepartment,
    --list of all users ids in a department
    STRING_AGG(Users.UserId, ', ') AS UserIds
FROM TutorialAppSchema.Users as Users
    JOIN TutorialAppSchema.UserSalary as UserSalary
    ON UserSalary.UserId = Users.UserId
    LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
    ON UserJobInfo.UserId = Users.UserId
WHERE Users.Active = 1
GROUP BY [UserJobInfo].[Department]
ORDER BY UserJobInfo.Department DESC;

SELECT [Users].[FirstName] + ' ' + [Users].[LastName] as FullName,
    [Users].[Email],
    [Users].[Gender],
    [Users].[Active],
    [UserJobInfo].[JobTitle],
    [UserJobInfo].[Department],
    DepartmentAverage.AvgSalary
FROM TutorialAppSchema.Users as Users
    JOIN TutorialAppSchema.UserSalary as UserSalary
    ON UserSalary.UserId = Users.UserId
    LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
    ON UserJobInfo.UserId = Users.UserId
    --OUTER APPLY( --Similar to left join
        CROSS APPLY
( --Similar to join
        SELECT ISNULL([UserJobInfo2].[Department], 'No department listed') as Department,
        AVG([UserSalary2].[Salary]) as AvgSalary
    FROM TutorialAppSchema.UserSalary as UserSalary2
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo2
        ON UserJobInfo2.UserId = UserSalary2.UserId
    WHERE ISNULL([UserJobInfo2].[Department], 'No department listed') = ISNULL([UserJobInfo].[Department], 'No department listed')
    GROUP BY [UserJobInfo2].[Department]
    )
AS DepartmentAverage
WHERE Users.Active = 1
ORDER BY Users.UserId DESC

--physically store table in the order of this id
--makes the query run faster
CREATE CLUSTERED INDEX cix_UserSalary_UserId ON TutorialAppSchema.UserSalary(UserId)

--a new listing of just the salary in order and includes our clustered index
CREATE NONCLUSTERED INDEX cix_UserSalary_UserId ON TutorialAppSchema.UserSalary(UserId)

--also faster find department
CREATE NONCLUSTERED INDEX ix_UserJobInfo_JobTitle ON TutorialAppSchema.UserJobInfo(JobTitle) INCLUDE (Department)

--index with a where clause. When we search on the <active> field it includes <Email>, <FirstName> and <LastName>
--it always includes the clustered index
--Primary key is always a clustered index
CREATE NONCLUSTERED INDEX ix_Users_JobTitle ON TutorialAppSchema.Users(Active) INCLUDE ([Email],[FirstName], [LastName])

--cutout everything where active is 0 from the index
CREATE NONCLUSTERED INDEX ix_Users_JobTitle ON TutorialAppSchema.Users(Active) 
INCLUDE ([Email],[FirstName], [LastName])
WHERE Active = 1;

DELETE FROM TutorialAppSchema.UserSalary
WHERE UserId BETWEEN 250 AND 750;

--not equal: UserId <> 7

--same as join
SELECT *
FROM TutorialAppSchema.UserSalary AS UserSalary
WHERE EXISTS (SELECT *
FROM TutorialAppSchema.UserJobInfo as UserJobInfo
WHERE UserJobInfo.UserId = UserSalary.UserId AND UserId <> 7)

--select only unique values from both datasets
    SELECT [UserId],
        [Salary]
    from TutorialAppSchema.UserSalary
UNION
    SELECT [UserId],
        [Salary]
    from TutorialAppSchema.UserSalary

--select all records from both tables
    SELECT [UserId],
        [Salary]
    from TutorialAppSchema.UserSalary
UNION ALL
    SELECT [UserId],
        [Salary]
    from TutorialAppSchema.UserSalary


SELECT GETDATE()
SELECT DATEADD(YEAR, -5, GETDATE())

SELECT DATEDIFF(MINUTE, DATEADD(YEAR, -5, GETDATE()), GETDATE());
--Returns positive
SELECT DATEDIFF(MINUTE, GETDATE(),DATEADD(YEAR, -5, GETDATE()));
--Returns negative

ALTER TABLE TutorialAppSchema.UserSalary ADD AvgSalary DECIMAL(18,4);
-- add a blank field in a table

/*
basic stored procedure to select a user
exec is to execute it
*/
CREATE PROCEDURE TutorialAppSchema.spUsers_Get
    /*EXEC TutorialAppSchema.spUsers_Get @UserId=3*/
    @UserId INT = NULL
/*can be defaulted*/
AS
BEGIN
    SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active]
    FROM TutorialAppSchema.Users AS Users
    /*if @UserId is null, then return all users*/
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
END


--Using outer apply
--Use alter in case it already exists
ALTER PROCEDURE TutorialAppSchema.spUsers_Get
    /*EXEC TutorialAppSchema.spUsers_Get @UserId=3*/
    @UserId INT = NULL
AS
BEGIN
    SELECT [Users].[UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
        OUTER APPLY (
            SELECT AVG(UserSalary2.Salary) AvgSalary
        FROM TutorialAppSchema.Users AS Users
            LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary2
            ON UserSalary2.UserId = Users.UserId
            LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo2
            ON UserJobInfo2.UserId = Users.UserId
        WHERE UserJobInfo2.Department = UserJobInfo.Department
        GROUP BY UserJobInfo2.Department
        ) AS AvgSalary
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
END


--store data in a temp table and join it to the query
ALTER PROCEDURE TutorialAppSchema.spUsers_Get
    /*EXEC TutorialAppSchema.spUsers_Get @UserId=3*/
    @UserId INT = NULL
AS
BEGIN

    SELECT Department,
        AVG(UserSalary.Salary) AvgSalary
    INTO #AverageDeptSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
    GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary ON #AverageDeptSalary(Department);

    SELECT [Users].[UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary as AvgSalary
        ON AvgSalary.Department = UserJobInfo.Department
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
END


--drop the temp table if it already exists
    IF OBJECT_ID('tempdb..#AverageSalary', 'U') IS NOT NULL
    BEGIN
    DROP TABLE #AverageDeptSalary
END
--works in newer mssql
    DROP TABLE IF EXISTS #AverageDeptSalary


--whole users get procedure: 
ALTER PROCEDURE TutorialAppSchema.spUsers_Get
    /*EXEC TutorialAppSchema.spUsers_Get @UserId=3*/
    @UserId INT = NULL,
    @Active BIT = NULL
AS
BEGIN
    DROP TABLE IF EXISTS #AverageDeptSalary
    SELECT Department,
        AVG(UserSalary.Salary) AvgSalary
    INTO #AverageDeptSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
    GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary ON #AverageDeptSalary(Department);

    SELECT [Users].[UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary as AvgSalary
        ON AvgSalary.Department = UserJobInfo.Department
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
        AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END


--we insert a new user or update an existing one
--create or alter works on newer versions
CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_Upsert
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Gender NVARCHAR(50),
    @JobTitle NVARCHAR(50),
    @Department NVARCHAR(50),
    @Salary DECIMAL(18,4),
    @Active BIT = 1,
    @UserId INT = NULL
AS
BEGIN
    SELECT *
    FROM TutorialAppSchema.UserSalary
    SELECT *
    FROM TutorialAppSchema.UserJobInfo

    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Users
    WHERE UserId = @UserId)
        BEGIN
        DECLARE @OutputUserId INT
        IF NOT EXISTS (SELECT *
        FROM TutorialAppSchema.Users
        WHERE Email = @Email)
                BEGIN
            INSERT INTO TutorialAppSchema.Users
                (
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
                )
            VALUES
                (
                    @FirstName,
                    @LastName,
                    @Email,
                    @Gender,
                    @Active
                    )
            --identity of the row inserted in the user table
            SET @OutputUserId = @@IDENTITY

            INSERT INTO TutorialAppSchema.UserSalary
                (
                UserId,
                Salary
                )
            VALUES
                (
                    @OutputUserId,
                    @Salary
                    )

            INSERT INTO TutorialAppSchema.UserJobInfo
                (
                UserId,
                Department,
                JobTitle
                )
            VALUES
                (
                    @OutputUserId,
                    @Department,
                    @JobTitle
                    )
        END
    END
    ELSE 
        BEGIN
        UPDATE TutorialAppSchema.Users 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId

        UPDATE TutorialAppSchema.UserSalary 
                SET Salary = @Salary
                WHERE UserId = @UserId

        UPDATE TutorialAppSchema.UserJobInfo 
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
    END
END


--delete a user procedure
CREATE PROCEDURE TutorialAppSchema.spUser_Delete
    @userId INT
AS
BEGIN
    DELETE FROM TutorialAppSchema.Users
        WHERE UserId = @userId
    DELETE FROM TutorialAppSchema.UserSalary
        WHERE UserId = @userId
    DELETE FROM TutorialAppSchema.UserJobInfo
        WHERE UserId = @userId
END

--1. get all posts of user and matching by SearchValue
--2. get post by post id
CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_Get
    -- EXEC TutorialAppSchema.spPosts_Get @UserId = 1006, @SearchValue = 'hello'
    -- EXEC TutorialAppSchema.spPosts_Get @PostId = 2

    @UserId INT = NULL,-- =NULL means that it is nullable
    @SearchValue NVARCHAR(MAX) = NULL,
    @PostId INT = NULL
AS
BEGIN
    SELECT [Posts].[PostId],
        [Posts].[UserId],
        [Posts].[PostTitle],
        [Posts].[PostContent],
        [Posts].[PostCreated],
        [Posts].[PostUpdated]
    FROM TutorialAppSchema.Posts AS Posts
    WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
        AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
        AND (@SearchValue IS NULL
        OR Posts.PostContent LIKE '%'+ @SearchValue +'%'
        OR Posts.PostTitle LIKE '%'+ @SearchValue +'%'
        )
END



CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_Upsert
    @UserId INT,
    @PostTitle NVARCHAR(255),
    @PostContent NVARCHAR(MAX),
    @PostId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Posts
    WHERE PostId = @PostId)
        BEGIN
        INSERT INTO TutorialAppSchema.Posts
            (
            [UserId],
            [PostTitle],
            [PostContent],
            [PostCreated],
            [PostUpdated]
            )
        VALUES
            (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
    END
    ELSE 
        BEGIN
        UPDATE TutorialAppSchema.Posts
                SET PostTitle = @PostTitle,
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId
    END
END


CREATE OR ALTER PROCEDURE TutorialAppSchema.spPost_Delete
    @PostId INT,
    @UserId INT
AS
BEGIN
    DELETE FROM TutorialAppSchema.Posts 
    WHERE PostId = @PostId
        AND UserId = @UserId
END


CREATE OR ALTER PROCEDURE TutorialAppSchema.spRegistration_Upsert
    @Email NVARCHAR(50),
    @PasswordHash VARBINARY(MAX),
    @PasswordSalt VARBINARY(MAX)
AS
BEGIN
    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Auth
    WHERE Email = @Email)
        BEGIN
        INSERT INTO TutorialAppSchema.Auth
            (
            [Email],
            [PasswordHash],
            [PasswordSalt]
            )
        VALUES
            (
                @Email,
                @PasswordHash,
                @PasswordSalt
            )
    END
    ELSE
        BEGIN
        UPDATE TutorialAppSchema.Auth
            SET PasswordHash = @PasswordHash,
                PasswordSalt = @PasswordSalt
            WHERE Email = @Email
    END
END
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.sp_LoginConfirmation_Get
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT [Auth].[PasswordHash],
        [Auth].[PasswordSalt]
    FROM TutorialAppSchema.Auth AS Auth
    WHERE Email = @Email
END







--production structure
CREATE SCHEMA TutorialAppSchema;
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUserId_Get
    @UserId INT = NULL,
    @Email NVARCHAR(50) = NULL
AS
BEGIN
    SELECT UserId
    FROM TutorialAppSchema.Users
    WHERE
        (@UserId IS NULL OR UserId = @UserId)
        AND
        Email = ISNULL(@Email, Email)
END

CREATE TABLE TutorialAppSchema.Users
(
    UserId INT IDENTITY(1, 1) PRIMARY KEY
    ,
    FirstName NVARCHAR(50)
    ,
    LastName NVARCHAR(50)
    ,
    Email NVARCHAR(50)
    ,
    Gender NVARCHAR(50)
    ,
    Active BIT
);
GO

CREATE NONCLUSTERED INDEX fix_Users_Active
    ON TutorialAppSchema.Users (Active)
    INCLUDE (Email, FirstName, LastName, Gender)
    WHERE active = 1;

CREATE TABLE TutorialAppSchema.UserSalary
(
    UserId INT
    ,
    Salary DECIMAL(18, 4)
);
GO

CREATE CLUSTERED INDEX cix_UserSalary_UserId
    ON TutorialAppSchema.UserSalary (UserId);
GO

CREATE TABLE TutorialAppSchema.UserJobInfo
(
    UserId INT
    ,
    JobTitle NVARCHAR(50)
    ,
    Department NVARCHAR(50),
);
GO

CREATE NONCLUSTERED INDEX ix_UserJobInfo_JobTitle
    ON TutorialAppSchema.UserJobInfo (JobTitle)
    INCLUDE (Department);
GO

CREATE TABLE TutorialAppSchema.Auth
(
    Email NVARCHAR(50) PRIMARY KEY,
    PasswordHash VARBINARY(MAX),
    PasswordSalt VARBINARY(MAX)
)
GO

CREATE TABLE TutorialAppSchema.Posts
(
    PostId INT IDENTITY(1,1),
    UserId INT,
    PostTitle NVARCHAR(255),
    PostContent NVARCHAR(MAX),
    PostCreated DATETIME,
    PostUpdated DATETIME
)

CREATE CLUSTERED INDEX cix_Posts_UserId_PostId ON TutorialAppSchema.Posts(UserId, PostId)
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUsers_Get
    /*EXEC TutorialAppSchema.spUsers_Get @UserId=3*/
    @UserId INT = NULL
    ,
    @Active BIT = NULL
AS
BEGIN
    DROP TABLE IF EXISTS #AverageDeptSalary
    -- IF OBJECT_ID('tempdb..#AverageDeptSalary') IS NOT NULL
    --     DROP TABLE #AverageDeptSalary;

    SELECT UserJobInfo.Department
        , AVG(UserSalary.Salary) AvgSalary
    INTO #AverageDeptSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
    GROUP BY UserJobInfo.Department

    CREATE CLUSTERED INDEX cix_AverageDeptSalary_Department ON #AverageDeptSalary(Department)

    SELECT [Users].[UserId],
        [Users].[FirstName],
        [Users].[LastName],
        [Users].[Email],
        [Users].[Gender],
        [Users].[Active],
        UserSalary.Salary,
        UserJobInfo.Department,
        UserJobInfo.JobTitle,
        AvgSalary.AvgSalary
    FROM TutorialAppSchema.Users AS Users
        LEFT JOIN TutorialAppSchema.UserSalary AS UserSalary
        ON UserSalary.UserId = Users.UserId
        LEFT JOIN TutorialAppSchema.UserJobInfo AS UserJobInfo
        ON UserJobInfo.UserId = Users.UserId
        LEFT JOIN #AverageDeptSalary AS AvgSalary
        ON AvgSalary.Department = UserJobInfo.Department
    WHERE Users.UserId = ISNULL(@UserId, Users.UserId)
        AND ISNULL(Users.Active, 0) = COALESCE(@Active, Users.Active, 0)
END
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_Upsert
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(50),
    @Gender NVARCHAR(50),
    @JobTitle NVARCHAR(50),
    @Department NVARCHAR(50),
    @Salary DECIMAL(18, 4),
    @Active BIT = 1,
    @UserId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Users
    WHERE UserId = @UserId)
        BEGIN
        IF NOT EXISTS (SELECT *
        FROM TutorialAppSchema.Users
        WHERE Email = @Email)
            BEGIN
            DECLARE @OutputUserId INT

            INSERT INTO TutorialAppSchema.Users
                (
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
                )
            VALUES
                (
                    @FirstName,
                    @LastName,
                    @Email,
                    @Gender,
                    @Active
                )

            SET @OutputUserId = @@IDENTITY

            INSERT INTO TutorialAppSchema.UserSalary
                (
                UserId,
                Salary
                )
            VALUES
                (
                    @OutputUserId,
                    @Salary
                )

            INSERT INTO TutorialAppSchema.UserJobInfo
                (
                UserId,
                Department,
                JobTitle
                )
            VALUES
                (
                    @OutputUserId,
                    @Department,
                    @JobTitle
                )
        END
    END
    ELSE 
        BEGIN
        UPDATE TutorialAppSchema.Users 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Gender = @Gender,
                    Active = @Active
                WHERE UserId = @UserId

        UPDATE TutorialAppSchema.UserSalary
                SET Salary = @Salary
                WHERE UserId = @UserId

        UPDATE TutorialAppSchema.UserJobInfo
                SET Department = @Department,
                    JobTitle = @JobTitle
                WHERE UserId = @UserId
    END
END
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spUser_Delete
    @UserId INT
AS
BEGIN
    DECLARE @Email NVARCHAR(50);

    SELECT @Email = Users.Email
    FROM TutorialAppSchema.Users
    WHERE  Users.UserId = @UserId;

    DELETE  FROM TutorialAppSchema.UserSalary
     WHERE  UserSalary.UserId = @UserId;

    DELETE  FROM TutorialAppSchema.UserJobInfo
     WHERE  UserJobInfo.UserId = @UserId;

    DELETE  FROM TutorialAppSchema.Users
     WHERE  Users.UserId = @UserId;

    DELETE  FROM TutorialAppSchema.Auth
     WHERE  Auth.Email = @Email;
END;
GO



CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_Get
    /*EXEC TutorialAppSchema.spPosts_Get @UserId = 1003, @SearchValue='Second'*/
    /*EXEC TutorialAppSchema.spPosts_Get @PostId = 2*/
    @UserId INT = NULL
    ,
    @SearchValue NVARCHAR(MAX) = NULL
    ,
    @PostId INT = NULL
AS
BEGIN
    SELECT [Posts].[PostId],
        [Posts].[UserId],
        [Posts].[PostTitle],
        [Posts].[PostContent],
        [Posts].[PostCreated],
        [Posts].[PostUpdated]
    FROM TutorialAppSchema.Posts AS Posts
    WHERE Posts.UserId = ISNULL(@UserId, Posts.UserId)
        AND Posts.PostId = ISNULL(@PostId, Posts.PostId)
        AND (@SearchValue IS NULL
        OR Posts.PostContent LIKE '%' + @SearchValue + '%'
        OR Posts.PostTitle LIKE '%' + @SearchValue + '%')
END
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spPosts_Upsert
    @UserId INT
    ,
    @PostTitle NVARCHAR(255)
    ,
    @PostContent NVARCHAR(MAX)
    ,
    @PostId INT = NULL
AS
BEGIN
    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Posts
    WHERE PostId = @PostId)
        BEGIN
        INSERT INTO TutorialAppSchema.Posts
            (
            [UserId],
            [PostTitle],
            [PostContent],
            [PostCreated],
            [PostUpdated]
            )
        VALUES
            (
                @UserId,
                @PostTitle,
                @PostContent,
                GETDATE(),
                GETDATE()
            )
    END
    ELSE
        BEGIN
        UPDATE TutorialAppSchema.Posts 
                SET PostTitle = @PostTitle,
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId
    END
END
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spPost_Delete
    @PostId INT
    ,
    @UserId INT
AS
BEGIN
    DELETE FROM TutorialAppSchema.Posts 
        WHERE PostId = @PostId
        AND UserId = @UserId
END
GO



CREATE OR ALTER PROCEDURE TutorialAppSchema.spLoginConfirmation_Get
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT [Auth].[PasswordHash],
        [Auth].[PasswordSalt]
    FROM TutorialAppSchema.Auth AS Auth
    WHERE Auth.Email = @Email
END;
GO

CREATE OR ALTER PROCEDURE TutorialAppSchema.spRegistration_Upsert
    @Email NVARCHAR(50),
    @PasswordHash VARBINARY(MAX),
    @PasswordSalt VARBINARY(MAX)
AS
BEGIN
    IF NOT EXISTS (SELECT *
    FROM TutorialAppSchema.Auth
    WHERE Email = @Email)
        BEGIN
        INSERT INTO TutorialAppSchema.Auth
            (
            [Email],
            [PasswordHash],
            [PasswordSalt]
            )
        VALUES
            (
                @Email,
                @PasswordHash,
                @PasswordSalt
            )
    END
    ELSE
        BEGIN
        UPDATE TutorialAppSchema.Auth 
                SET PasswordHash = @PasswordHash,
                    PasswordSalt = @PasswordSalt
                WHERE Email = @Email
    END
END
GO