SELECT toSec.*, ts.Name Section FROM dbo.tOrg_Section toSec
LEFT JOIN dbo.tSection ts ON toSec.ID_Section = ts.ID