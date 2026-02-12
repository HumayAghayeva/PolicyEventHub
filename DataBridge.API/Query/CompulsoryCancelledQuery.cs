namespace DataBridge.API.Query
{
    public static class CompulsoryCancelledQuery
    {
       public static string GetCancelledCompulsoryPolicies => @"SELECT * FROM (
SELECT ROW_NUMBER () OVER ( PARTITION BY v.CarNumber,p.PIN ORDER BY c.CreationDate DESC ) RN,
    cmpt.ID AS Id,
    v.CarNumber AS Plate,
    p.FullName AS InsuredFullName,
    p.PIN AS PIN,
    p.IDDocument AS DocNumber,
    p.Email AS Email,
    p.Phone AS PhoneNumber,
    c.ContractNumber AS ContractNumber,
    cd.DriverLicense AS DriverLicense,
    v.CertificationNumber AS CertificationNumber,
    c.CreationDate AS StartDate,
    DATEADD(YEAR, 1, c.CreationDate) AS EndDate,
    CASE 
        WHEN cmpt.CreatedOn >= DATEADD(DAY, -2, GETDATE())
             THEN '1'  -- Waiting
        ELSE '0'       -- Active
    END AS ContractStatus
FROM CIBM.CIBM.CMTPL AS cmpt
INNER JOIN CIBM.CIBM.Contract AS c
    ON cmpt.ContractID = c.ID
LEFT JOIN CIBM.CIBM.Vehicle AS v
    ON cmpt.VehicleID = v.ID
LEFT JOIN CIBM.CIBM.ContractDetail AS cd
    ON c.ContractDetailID = cd.ID
LEFT JOIN CIBM.CIBM.InsuredPerson AS p
    ON c.InsuredPersonID = p.ID  AND p.InsuredPersonTypeID = 1
LEFT JOIN CIBM.CIBM.ContractPayment AS cp
    ON cp.ContractID = cmpt.ContractID AND cp.CreatedOn >= DATEADD(DAY, -365,  GETDATE())
INNER JOIN CIBM.CIBM.Operator AS op
    ON op.ID = c.OperatorID
   AND op.PinCode = @operatorPinCode
WHERE cmpt.CreatedOn BETWEEN  @startDate AND @endDate
 
 -- Optional filters
 AND (@plate IS NULL OR v.CarNumber LIKE '%' + @plate + '%')
 AND (@certificationNumber IS NULL OR v.CertificationNumber LIKE '%' + @certificationNumber + '%')
 AND (@insuredFullname IS NULL OR p.FullName LIKE '%' + @insuredFullname + '%')
 AND (@pin IS NULL OR p.PIN LIKE '%' + @pin + '%')
ORDER BY 
    cmpt.CreatedOn DESC

-- Pagination
OFFSET @pageSize * (@pageNumber - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY) result
WHERE RN = 1";


            public static string GetUserPincodeByUserId => @"
select pr.PIN from hcm.pt_hcm_profile.profiles  pr
LEFT JOIN  core.pt_security.USERS u ON pr.NT_ACCOUNT = u.NTACCOUNT where u.UID = @userId";

            public static string GetCancelledCompulsoryPolicyById => @"
SELECT cmpt.ID AS Id, 
v.CarNumber AS Plate, 
v.CertificationNumber AS CertificationNumber,
c.ContractNumber AS ContractNumber,
p.IDDocument As DocNumber,
       p.FullName AS InsuredFullName, p.PIN AS PIN,
       p.Email AS Email, p.Phone AS PhoneNumber,
       c.CreationDate AS StartDate,
     cd.DriverLicense AS DriverLicense,
       DATEADD(YEAR, 1, c.CreationDate) AS EndDate,
-- Added status logic
    CASE 
        WHEN cmpt.CreatedOn >= DATEADD(DAY, -2, GETDATE())
             THEN '1'  --waiting
        ELSE '0'       --Active  
    END AS ContractStatus
FROM CIBM.CIBM.CMTPL AS cmpt
LEFT JOIN CIBM.CIBM.Contract AS c
       ON cmpt.ContractID = c.ID
LEFT JOIN CIBM.CIBM.ContractDetail cd
   on  c.ContractDetailID=cd.id
LEFT JOIN CIBM.CIBM.Vehicle AS v
       ON cmpt.VehicleID = v.ID
LEFT JOIN CIBM.CIBM.InsuredPerson AS p
       ON c.InsuredPersonID = p.ID
WHERE cmpt.ID = @id";


            public static string GetCancelledCompulsoryPoliciesCount =>
                @"SELECT COUNT(1)
FROM (
    SELECT
        ROW_NUMBER() OVER (
            PARTITION BY v.CarNumber, p.PIN
            ORDER BY c.CreationDate DESC
        ) AS RN
    FROM CIBM.CIBM.CMTPL AS cmpt
    INNER JOIN CIBM.CIBM.Contract AS c
        ON cmpt.ContractID = c.ID
    LEFT JOIN CIBM.CIBM.Vehicle AS v
        ON cmpt.VehicleID = v.ID
    LEFT JOIN CIBM.CIBM.InsuredPerson AS p
        ON c.InsuredPersonID = p.ID
       AND p.InsuredPersonTypeID = 1
    LEFT JOIN CIBM.CIBM.ContractPayment AS cp
        ON cp.ContractID = cmpt.ContractID
       AND cp.CreatedOn >= DATEADD(DAY, -365, GETDATE())
    INNER JOIN CIBM.CIBM.Operator AS op
        ON op.ID = c.OperatorID
       AND op.PinCode = @operatorPinCode
    WHERE cmpt.CreatedOn BETWEEN @startDate AND @endDate
      AND (@plate IS NULL OR v.CarNumber LIKE '%' + @plate + '%')
      AND (@certificationNumber IS NULL OR v.CertificationNumber LIKE '%' + @certificationNumber + '%')
      AND (@insuredFullname IS NULL OR p.FullName LIKE '%' + @insuredFullname + '%')
      AND (@pin IS NULL OR p.PIN LIKE '%' + @pin + '%')
) t
WHERE t.RN = 1;
";

            public static string UpdateCompulsaryPolicy =>
             @"dbo.sp_CancelledCompulsoryPolicy_Update";
        }
    }

