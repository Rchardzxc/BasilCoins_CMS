SELECT tmm.*, tg.Name AS Gender FROM dbo.tMyModule tmm
LEFT JOIN dbo.tGender tg ON tmm.ID_Gender = tg.ID