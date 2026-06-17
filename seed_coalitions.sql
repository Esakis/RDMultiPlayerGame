SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @era int = 1;
DECLARE @max int = 17;

-- Definicje koalicji (BaseLand steruje sumarycznym obszarem -> kolejnosc w rankingu)
DECLARE @coals TABLE (Seq int, Name nvarchar(200), Tag nvarchar(10), BaseLand int);
INSERT INTO @coals (Seq,Name,Tag,BaseLand) VALUES
 (1, N'Imperium Czerwonego Smoka', N'SMOK',   4000),
 (2, N'Liga Wolnych Miast',        N'LIGA',   3200),
 (3, N'Przymierze Polnocy',        N'POLNOC', 2600),
 (4, N'Bractwo Cienia',            N'CIEN',   2000);

-- Pula 17 nazw panstw
DECLARE @names TABLE (Idx int, Realm nvarchar(100));
INSERT INTO @names (Idx,Realm) VALUES
 (1,N'Avalon'),(2,N'Eldoria'),(3,N'Valmoria'),(4,N'Wichrowy Tron'),(5,N'Srebrnogard'),
 (6,N'Drakonia'),(7,N'Mglista Dolina'),(8,N'Zelazna Twierdza'),(9,N'Wysogrod'),(10,N'Czarnobor'),
 (11,N'Sloneczny Brzeg'),(12,N'Krwawy Step'),(13,N'Lodowy Szczyt'),(14,N'Zlota Przystan'),(15,N'Cierniogrod'),
 (16,N'Swit'),(17,N'Zmierzch');

-- 11 ras gry (cykl)
DECLARE @races TABLE (Idx int, Race nvarchar(50));
INSERT INTO @races (Idx,Race) VALUES
 (0,N'Czlowiek'),(1,N'Elf'),(2,N'Krasnolud'),(3,N'Hobbit'),(4,N'Nekromant'),(5,N'Dzin'),
 (6,N'Goblin'),(7,N'Ent'),(8,N'Olbrzym'),(9,N'Gnom'),(10,N'Br-Oug');

DECLARE @g int = (SELECT COUNT(*) FROM Users);   -- offset gwarantujacy unikalne loginy

DECLARE @cseq int, @cname nvarchar(200), @ctag nvarchar(10), @cbase int;
DECLARE @coalId int, @i int, @firstKingdom int;
DECLARE @uname nvarchar(100), @email nvarchar(255), @uid int;
DECLARE @realm nvarchar(100), @race nvarchar(50), @land int, @role nvarchar(50), @kid int;

DECLARE coal_cur CURSOR LOCAL FAST_FORWARD FOR
  SELECT Seq,Name,Tag,BaseLand FROM @coals ORDER BY Seq;
OPEN coal_cur;
FETCH NEXT FROM coal_cur INTO @cseq,@cname,@ctag,@cbase;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO Coalitions (Name,Tag,EraId,MaxMembers,PSOProgress,CreatedAt)
      VALUES (@cname,@ctag,@era,@max,0,SYSUTCDATETIME());
    SET @coalId = SCOPE_IDENTITY();

    SET @i = 1;
    SET @firstKingdom = NULL;
    WHILE @i <= @max
    BEGIN
        SET @g = @g + 1;
        SET @uname = N'seed_' + @ctag + N'_' + CAST(@i AS nvarchar(10)) + N'_' + CAST(@g AS nvarchar(10));
        SET @email = @uname + N'@seed.local';
        INSERT INTO Users (Email,PasswordHash,Username,CreatedAt)
          VALUES (@email, N'SEED_NO_LOGIN', @uname, SYSUTCDATETIME());
        SET @uid = SCOPE_IDENTITY();

        SET @realm = (SELECT Realm FROM @names WHERE Idx=@i);
        SET @race  = (SELECT Race  FROM @races WHERE Idx=(@i % 11));
        SET @land  = @cbase + (ABS(CHECKSUM(NEWID())) % 1500);
        SET @role  = CASE WHEN @i=1 THEN N'Imperator'
                          WHEN @i=2 THEN N'MainCommander'
                          ELSE N'Member' END;

        INSERT INTO Kingdoms
          (UserId,Name,Race,IsMagicRace,Land,Gold,Food,Stone,Budulec,BudulecStored,Weapons,Mana,
           Population,Popularity,Wages,Education,TurnsAvailable,TurnsPerDay,MaxTurns,TurnNumber,Age,
           SpecialBuildingProgress,SpecialBuildingCost,EraId,IsProtected,ProtectionDaysLeft,CreatedAt,LastActive,
           CoalitionId,CoalitionRole)
        VALUES
          (@uid, @realm + N' [' + @ctag + N']', @race, CASE WHEN @race=N'Goblin' THEN 0 ELSE 1 END, @land,
           500000, 100000, 20000, 0, 0, 0, 0,
           @land*8, 100, 50, 0, 15, 15, 49, 0, 0,
           0, 0, @era, 0, 0, SYSUTCDATETIME(), SYSUTCDATETIME(),
           @coalId, @role);
        SET @kid = SCOPE_IDENTITY();
        IF @i = 1 SET @firstKingdom = @kid;

        SET @i = @i + 1;
    END

    UPDATE Coalitions SET LeaderKingdomId = @firstKingdom WHERE Id = @coalId;

    FETCH NEXT FROM coal_cur INTO @cseq,@cname,@ctag,@cbase;
END
CLOSE coal_cur;
DEALLOCATE coal_cur;

COMMIT;

SELECT c.Id, c.Name, c.Tag,
       (SELECT COUNT(*) FROM Kingdoms k WHERE k.CoalitionId=c.Id) AS Czlonkow,
       (SELECT SUM(CAST(k.Land AS bigint)) FROM Kingdoms k WHERE k.CoalitionId=c.Id) AS LacznaZiemia
FROM Coalitions c
WHERE c.EraId=1
ORDER BY LacznaZiemia DESC;
