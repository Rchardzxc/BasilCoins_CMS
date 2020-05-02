using InSys.Worker.Library.Classes;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using z.Data;

namespace Service.ETF.Classes
{
    public class ETF : IDisposable
    {
        private string basePath { get; set; }
        private string newPath { get; set; }
        private string processingPath { get; set; }
        private string successfulPath { get; set; }
        private string unprocessPath { get; set; }
        private int ID_Company { get; set; }
        private int ID_EtfMigration { get; set; }
        private BaseConfig baseCfg { get; set; }
        private EventLog myLog { get; set; }
        public List<ETFSchema> PersonaSchema { get; set; }
        public List<ETFSchema> EmployeeSchema { get; set; }

        private string ConStr { get; set; }
        public ETF(string excelSourcePath, BaseConfig _baseCfg, EventLog _myLog)
        {
            basePath = excelSourcePath;
            newPath = Path.Combine(basePath, "New");
            processingPath = Path.Combine(basePath, "Processing");
            successfulPath = Path.Combine(basePath, "Successful");
            unprocessPath = Path.Combine(basePath, "Unprocess");
            baseCfg = _baseCfg;
            myLog = _myLog;
            ConStr = $"Server={baseCfg.Server},{baseCfg.Port};Database={baseCfg.Database};User ID={baseCfg.UserID};Password={baseCfg.Password};Trusted_Connection=False;MultipleActiveResultSets=True;Connection Timeout=60;Max Pool Size=1000;";
        }
        public bool StartProcessing()
        {
            Logger.LogMessage("Checking for new ETF Template", "StartProcessing", myLog, EventLogEntryType.Information);
            //MoveToProcessing();

            //Create Folder if not exists
            //New Folder
            if (!Directory.Exists(newPath)) {
                Directory.CreateDirectory(newPath);
            }
            //Unprocess Folder
            if (!Directory.Exists(unprocessPath))
            {
                Directory.CreateDirectory(unprocessPath);
            }
            //Process Folder
            if (!Directory.Exists(processingPath))
            {
                Directory.CreateDirectory(processingPath);
            }
            //Successful Folder
            if (!Directory.Exists(successfulPath))
            {
                Directory.CreateDirectory(successfulPath);
            }

            var files = Directory.GetFiles(newPath);

            using (var sqlcon = new SqlConnection(ConStr))
            {
                try
                {
                    if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();

                    //Get Schema Of the Table

                    PersonaSchema = ExecETFSchema("SELECT Name, IsRequired, DataType, Length FROM dbo.fETFSchema('tPersona')", sqlcon);
                    EmployeeSchema = ExecETFSchema("SELECT Name, IsRequired, DataType, Length FROM dbo.fETFSchema('tEmployee')", sqlcon);

                    foreach (var file in files)
                    {

                        ID_Company = 0;
                        ID_EtfMigration = 0;

                        var fi = new FileInfo(file);
                        string dspName = fi.Name.Split(new string[] { ".CDITI." }, StringSplitOptions.None)[0];
                        var dt = new DataTable();

                        //execute all unfinished excel
                        //using (var sqlcmd = new SqlCommand("SELECT * FROM dbo.tEtfMigration WHERE FileName = @name AND ProcessFinish IS NULL", sqlcon))
                        using (var sqlcmd = new SqlCommand("SELECT * FROM dbo.tEtfMigration WHERE FileName = @name", sqlcon))
                        {
                            sqlcmd.Parameters.AddWithValue("@name", fi.Name);
                            using (var sqlda = new SqlDataAdapter(sqlcmd))
                            {
                                sqlda.Fill(dt);
                            }
                        }
                        DataRow record = null;
                        if (dt.Rows.Count > 0)
                        {
                            record = dt.Rows[0];
                            Logger.LogMessage($"Moving {dspName} to processing", "MoveToProcessing", myLog, EventLogEntryType.Information);
                            File.Move(file, Path.Combine(processingPath, fi.Name));

                            fi = new FileInfo(Path.Combine(processingPath, fi.Name));
                            var errorList = new List<string>();
                            Logger.LogMessage($"Starting to process {dspName}", "StartProcessing", myLog, EventLogEntryType.Information);


                            if (fi.Extension.ToLower() == ".xlsx" || fi.Extension.ToLower() == ".xls")
                            {
                                var tmp = Path.GetTempFileName().ToString().Replace(".tmp", "") + ".xlsx";
                                IWorkbook wb = new XSSFWorkbook();
                                try
                                {
                                    wb = new XSSFWorkbook(fi);
                                    if (wb == null) throw new Exception($"There is a problem loading the {dspName}");
                                    ID_Company = record["ID_Company"].ToInt32();
                                    ID_EtfMigration = record["ID"].ToInt32();
                                    ValidateEmployee(ref wb, fi.DirectoryName, fi.Name);
                                }
                                catch (Exception ex)
                                {
                                    if (wb != null) wb.Close();
                                    Logger.LogError(ex, "ValidatingExcel", myLog);
                                }
                                finally
                                {
                                    if (wb != null) wb.Close();
                                    File.Move(fi.FullName, Path.Combine(successfulPath, fi.Name));
                                    if (record["ID_UserCreatedBy"].IsNull(0).ToInt32() != 0)
                                    {
                                        try
                                        {
                                            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                                            using (var insertNotification = new SqlCommand(@"INSERT INTO dbo.tWebNotification(Title , Content , ID_Receiver , ID_Sender , ID_WebNotificationTypes , ReferenceID, LinkOnClick)
                                                                                    VALUES(@Title,@Content,@ID_Receiver,@ID_Sender,@ID_WebNotificationTypes,@ReferenceID,@link)", sqlcon))
                                            {
                                                insertNotification.Parameters.AddWithValue("@Title", "Initial Migration");
                                                insertNotification.Parameters.AddWithValue("@Content", $"File {dspName} has been uploaded.");
                                                insertNotification.Parameters.AddWithValue("@ID_Receiver", record["ID_UserCreatedBy"].ToInt32());
                                                insertNotification.Parameters.AddWithValue("@ID_Sender", 1);
                                                insertNotification.Parameters.AddWithValue("@ID_WebNotificationTypes", 1004);
                                                insertNotification.Parameters.AddWithValue("@ReferenceID", record["ID"].ToInt32());
                                                insertNotification.Parameters.AddWithValue("@link", "PeopleInitialMigration.Form");
                                                insertNotification.ExecuteNonQuery();
                                                Logger.LogMessage($"Notification sent to uploader.", "SendNotification", myLog, EventLogEntryType.Information);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.LogError(ex, "SendNotification", myLog);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogError("File not supported. Should be in excel format.", "ValidatingExcel", myLog);
                                if (record["ID_UserCreatedBy"].IsNull(0).ToInt32() != 0)
                                {
                                    try
                                    {
                                        if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                                        using (var insertNotification = new SqlCommand(@"INSERT INTO dbo.tWebNotification(Title , Content , ID_Receiver , ID_Sender , ID_WebNotificationTypes , ReferenceID, LinkOnClick)
                                                                                    VALUES(@Title,@Content,@ID_Receiver,@ID_Sender,@ID_WebNotificationTypes,@ReferenceID,@link)", sqlcon))
                                        {
                                            insertNotification.Parameters.AddWithValue("@Title", "Initial Migration");
                                            insertNotification.Parameters.AddWithValue("@Content", $"File {dspName} not supported, Should be in excel format.");
                                            insertNotification.Parameters.AddWithValue("@ID_Receiver", record["ID_UserCreatedBy"].ToInt32());
                                            insertNotification.Parameters.AddWithValue("@ID_Sender", 1);
                                            insertNotification.Parameters.AddWithValue("@ID_WebNotificationTypes", 1004);
                                            insertNotification.Parameters.AddWithValue("@ReferenceID", record["ID"].ToInt32());
                                            insertNotification.Parameters.AddWithValue("@link", "PeopleInitialMigration.Form");
                                            insertNotification.ExecuteNonQuery();
                                            Logger.LogMessage($"Notification sent to uploader.", "SendNotification", myLog, EventLogEntryType.Information);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogError(ex, "SendNotification", myLog);
                                    }
                                }
                            }

                            try
                            {
                                using (var sqlcmd = new SqlCommand("update dbo.tEtfMigration set ProcessFinish = GETDATE() where ID = @ID", sqlcon))
                                {
                                    sqlcmd.Parameters.AddWithValue("@ID", record["ID"].ToInt32());
                                    sqlcmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "UpdateFinishDate", myLog);
                            }
                        }
                        else
                        {
                            Logger.LogError($"Could not find record with file name {dspName}.", "FileChecking", myLog);
                            File.Move(fi.FullName, Path.Combine(unprocessPath, fi.Name));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "StartProcessing", myLog);
                }
                finally
                {
                    sqlcon.Close();
                }
            }
            return true;
        }
        
        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        private void ValidateEmployee(ref IWorkbook wb, string fullPath, string fileName)
        {
            Logger.LogMessage($"Validating Employee", "ValidateEmployee", myLog, EventLogEntryType.Information);
            List<ExcelRowError> ErrorList = new List<ExcelRowError>();
            List<ExcelColumn> Columns = new List<ExcelColumn>();
            DataTable dt = new DataTable();

            try
            {
                using (var ms = new MemoryStream())
                {
                    XSSFFormulaEvaluator formula = new XSSFFormulaEvaluator(wb);

                    //This will be used to store all final values
                    //This will be used to store all columns
                    Dictionary<string, List<DataList>> DataSets = new Dictionary<string, List<DataList>>();
                    Dictionary<string, int> ExcelColumns = new Dictionary<string, int>();
                    Dictionary<int, string> RowCellValues = new Dictionary<int, string>();

                    //ClearCreateErrorFields("Company Info", ref wb);

                    var employeeSheet = wb.GetSheet("Company Info");
                    var column = employeeSheet.GetRow(0);
                    var _columns = column.Where(x =>
                                   x.StringCellValue != "Required Columns"
                                   && x.StringCellValue != "Invalid Values"
                                   && x.StringCellValue != "Exception Error"
                                   && x.StringCellValue != "Inserted"
                                   && x.StringCellValue.Replace(" ", "") != ""
                       ).ToArray();

                    //Get and Validate Persona data
                    //var dtPersona = ValidatePersona(ref wb, ErrorList);

                    foreach (var col in _columns) {

                        var _EmployeeSchema = EmployeeSchema.Where(e => e.Name.ToLower() == col.StringCellValue.ToLower()).SingleOrDefault();
                        
                        Columns.Add(new ExcelColumn() {
                            Index = col.ColumnIndex,
                            ColName = col.StringCellValue,
                            IsRequired = col.CellStyle.FillPattern == FillPattern.SolidForeground && col.CellStyle.FillForegroundColorColor.RGB.SequenceEqual(new XSSFColor(System.Drawing.Color.Red).RGB) ? true : false,
                            DataType = EmployeeSchema.Where(x => x.Name == col.StringCellValue).SingleOrDefault().DataType
                        });
                        ExcelColumns.Add(col.StringCellValue, col.ColumnIndex);
                        RowCellValues.Add(col.ColumnIndex, "");
                    }
                    
                    //Set DataSets To Be LookUp
                    string[] BlankCharList = new string[] { "0", "null", "" };
                    using (var sqlCon = new SqlConnection(ConStr))
                    {
                        DataSets.Add("ID_EmployeeStatus", ExecDataList("SELECT Id, Name FROM tEmployeeStatus", sqlCon));
                        DataSets.Add("ID_PayrollScheme", ExecDataList("SELECT Id, Name FROM tPayrollScheme", sqlCon));
                        DataSets.Add("ID_PayrollFrequency", ExecDataList("SELECT Id, Name FROM tPayrollFrequency", sqlCon));
                        DataSets.Add("ID_TaxExemption", ExecDataList("SELECT Id, Name FROM tTaxExemption", sqlCon));
                        DataSets.Add("ID_PaymentMode", ExecDataList("SELECT Id, Name FROM tPaymentMode", sqlCon));
                        DataSets.Add("ID_Parameter", ExecDataList("SELECT Id, Name FROM tParameter", sqlCon));
                        DataSets.Add("ID_LeaveParameter", ExecDataList("SELECT Id, Name FROM tLeaveParameter", sqlCon));
                        DataSets.Add("ID_Currency", ExecDataList("SELECT Id, Name FROM tCurrency", sqlCon));
                        DataSets.Add("ID_PayrollStatus", ExecDataList("SELECT Id, Name FROM tPayrollStatus", sqlCon));
                        DataSets.Add("ID_PayrollClassification", ExecDataList("SELECT Id, Name FROM tPayrollClassification", sqlCon));
                        DataSets.Add("ID_AccountNumberType", ExecDataList("SELECT Id, Name FROM tAccountNumberType", sqlCon));
                        DataSets.Add("ID_Branch", ExecDataList("SELECT Id, Name FROM tBranch", sqlCon));
                        DataSets.Add("ID_Division", ExecDataList("SELECT Id, Name FROM tDivision", sqlCon));
                        DataSets.Add("ID_Department", ExecDataList("SELECT Id, Name FROM tDepartment", sqlCon));
                        DataSets.Add("ID_Section", ExecDataList("SELECT Id, Name FROM tSection", sqlCon));
                        DataSets.Add("ID_JobClassGroup", ExecDataList("SELECT Id, Name FROM tJobClassGroup", sqlCon));
                        DataSets.Add("ID_JobClass", ExecDataList("SELECT Id, Name FROM tJobClass", sqlCon));
                        DataSets.Add("ID_Designation", ExecDataList("SELECT Id, Name FROM tDesignation", sqlCon));

                        //Truncate Import Persona
                        string CommandText = "DELETE FROM tImportEmployee WHERE ID_EtfMigration = " + ID_EtfMigration.ToString();
                        using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlCon))
                        {
                            sqlcmd.ExecuteNonQuery();
                        }

                    }

                    try
                    {
                        //Initialize Dataset Lists
                        int idx = 1;
                        bool search = true;
                        string CurrentColumn = "";

                        string InsertStatement = "INSERT INTO tImportEmployee(ID_Company, ID_EtfMigration,";
                        
                        //Compose Insert Statement
                        foreach (var col in Columns) InsertStatement += col.ColName + ",";
                        InsertStatement = InsertStatement.Substring(0, InsertStatement.Length - 1) + ")";

                        try
                        {
                            while (search)
                            {
                                var row = employeeSheet.GetRow(idx);
                                string SelectStatement = String.Format("SELECT {0} AS ID_Company, {1} AS ID_EtfMigration, ", ID_Company, ID_EtfMigration);
                                if (row == null) { search = false; continue; }
                                Logger.LogMessage($"Validating row {idx}...", "ValidateEmployee", myLog, EventLogEntryType.Information);

                                //Plot Row Cells w/ Initial Data Based On Sheet Rows
                                foreach (var Cell in row.Cells)
                                {
                                    if (Cell.CellType == CellType.String || Cell.CellType == CellType.Formula)
                                        RowCellValues[Cell.ColumnIndex] = Cell.StringCellValue;
                                    else if (Cell.CellType == CellType.Numeric)
                                        RowCellValues[Cell.ColumnIndex] = Cell.NumericCellValue.ToString();
                                }

                                string ErrorMessage = "";
                                CurrentColumn = "";

                                foreach (var col in Columns)
                                {
                                    dynamic rawValue = null;
                                    var rowCellValue = RowCellValues[col.Index];
                                    CurrentColumn = col.ColName;

                                    if (rowCellValue != null)
                                    {
                                        //Validate required
                                        if (col.IsRequired)
                                        {
                                            if (Array.FindAll(BlankCharList, e => e.Equals(rowCellValue)).Length > 0)
                                                ErrorMessage += col.ColName + " is required.";
                                        }
                                        switch (col.DataType)
                                        {
                                            case "int":
                                                //Validate all Lookup fields
                                                //Hard Code value for ID_Company and ETF_Migration
                                                //Get all fields starts with ID_, indication that it is lookup
                                                if (col.ColName.Substring(0, 3).ToLower() == "id_" && rowCellValue.Length > 0 && DataSets.ContainsKey(col.ColName))
                                                {
                                                    rawValue = DataSets[col.ColName].Where(x => x.Name.ToLower() == rowCellValue.ToLower()).SingleOrDefault().Id;

                                                    if (rawValue == null)
                                                        ErrorMessage += rowCellValue + " is not existing on " + col.ColName.Substring(3, col.ColName.Length - 3) + " lists.";
                                                }

                                                if (col.ColName.Substring(0, 3).ToLower() != "id_")
                                                {
                                                    if (rowCellValue != "null")
                                                        rawValue = rowCellValue.ToInt32();
                                                    else rawValue = null;
                                                }
                                                break;
                                            case "decimal":
                                            case "money":

                                                //If money field is null, zero, space or blank then convert into 0.
                                                if (Array.FindAll(BlankCharList, e => e.Equals(rowCellValue)).Length > 0 ||
                                                    Convert.ToDecimal(rowCellValue) < 0)
                                                    rawValue = 0;
                                                else rawValue = Convert.ToDecimal(rowCellValue);
                                                break;
                                            case "bit":

                                                string val = rowCellValue.ToLower();
                                                if (val == "yes" || val == "true" || val == "1")
                                                    rawValue = 1; //True Value in MSSQL
                                                else if (val == "no" || val == "false" || val == "0")
                                                    rawValue = 0; //False Value in MSSQL
                                                else rawValue = null;

                                                break;
                                            case "varchar":
                                                rawValue = rowCellValue == "null" ? null : "'" + rowCellValue + "'";

                                                break;
                                            case "datetime":
                                                try
                                                {
                                                    if (rowCellValue == "")
                                                        rawValue = null;
                                                    else
                                                    {
                                                        //Check if Value is correct date format.
                                                        rawValue = Convert.ToDateTime(rowCellValue);
                                                        rawValue = "'" + rawValue.ToString() + "'";
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorMessage += ex.Message + " [" + col.ColName + "]";
                                                }
                                                break;
                                        }
                                    } else rawValue = "NULL";
                                    
                                    if (ErrorMessage.Length > 0)
                                        ErrorMessage += ErrorMessage;
                                    else
                                    {
                                        //Convert string if null value
                                        if (rawValue == null) rawValue = "";

                                        if (rawValue.ToString().Length == 0)
                                            rawValue = "NULL";

                                        SelectStatement += rawValue + " AS " + col.ColName + ",";
                                    }
                                }

                                if (ErrorMessage != "")
                                {
                                    ErrorList.Add(new ExcelRowError()
                                    {
                                        Sheet = SheetType.Company,
                                        ColName = "",
                                        Index = 0,
                                        Message = "Row #" + idx.ToString() + " on column [" + CurrentColumn + "]:" + ErrorMessage,
                                        Type = ErrorType.ExceptionError
                                    });
                                }
                                else
                                {
                                    SelectStatement = SelectStatement.Substring(0, SelectStatement.Length - 1);

                                    //Insert on import table 
                                    using (var sqlcon = new SqlConnection(ConStr))
                                    {
                                        if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                                        string CommandText = InsertStatement + " " + SelectStatement;
                                        using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlcon))
                                        {
                                            sqlcmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                idx += 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            string exMessage = (ex.InnerException ?? ex).Message.ToString();
                            Logger.LogError(exMessage + " on row " + idx, "ValidateEmployee_WhileSearch", myLog);
                            search = false;
                            ErrorList.Add(new ExcelRowError()
                            {
                                Sheet = SheetType.Company,
                                ColName = "",
                                Index = 0,
                                Message = "ValidateEmployee_WhileSearch:" + exMessage + " on row " + idx,
                                Type = ErrorType.ExceptionError
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = (ex.InnerException ?? ex).Message.ToString();
                        Logger.LogError(ex, "ValidateEmployee", myLog);
                        ErrorList.Add(new ExcelRowError() { Sheet = SheetType.Company, ColName = "", Index = 0, Message = message, Type = ErrorType.ExceptionError });
                    }
                    finally
                    {
                        //Execute Import Of ETF
                        using (var sqlcon = new SqlConnection(ConStr))
                        {
                            Logger.LogMessage($"Finalizing ETF...", "Finalizing ETF", myLog, EventLogEntryType.Information);
                            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                            string CommandText = "EXEC pImportETF " + ID_EtfMigration.ToString() + ", " + ID_Company.ToString() + ", 2";
                            using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlcon))
                            {
                                sqlcmd.ExecuteNonQuery();
                            }
                        }
                        //var colHeader = employeeSheet.GetRow(0);
                        //var exCol = colHeader.Where(x => x.StringCellValue == "Exception Error").FirstOrDefault();
                        //ICell cell = null;

                        //if (ErrorList.Any(x => x.Type == ErrorType.ExceptionError && x.Sheet == SheetType.Company))
                        //{
                        //    var exErrorList = ErrorList.Where(x => x.Type == ErrorType.ExceptionError && x.Sheet == SheetType.Company).ToArray();
                        //    for (var xx = 0; xx < exErrorList.Count(); xx++)
                        //    {
                        //        var row = employeeSheet.GetRow(xx + 1);
                        //        if (row == null) row = employeeSheet.CreateRow(xx + 1);
                        //        cell = row.GetCell(exCol.ColumnIndex);
                        //        if (cell == null) cell = row.CreateCell(exCol.ColumnIndex);
                        //        cell.SetCellValue(exErrorList[xx].Message);
                        //    }
                        //}
                        //try
                        //{
                        //    if (ms.Length > 0)
                        //        using (var fs = new FileStream(Path.Combine(fullPath, fileName), FileMode.Create, FileAccess.Write))
                        //        {
                        //            ms.Seek(0, SeekOrigin.Begin);
                        //            ms.CopyTo(fs);
                        //        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Logger.LogError(ex, "ValidateEmployee", myLog);
                        //}

                        wb.Write(ms);
                        wb.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ValidateEmployee", myLog);
            }

        }


        /// <summary>
        /// This is used to validate persona data.
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="ErrorList"></param>
        /// <returns></returns>
        private DataTable ValidatePersona(ref IWorkbook wb, List<ExcelRowError> ErrorList)
        {
            DataTable dt = new DataTable();
            try
            {
                //Create Options for Updating Data w/ the same code
                //Same Lastname, Firstname shouldn't allow
                //Initialized dataset lists and objects
                XSSFFormulaEvaluator formula = new XSSFFormulaEvaluator(wb);
                List<ExcelColumn> Columns = new List<ExcelColumn>();
                
                Dictionary<string, int> ExcelColumns = new Dictionary<string, int>();
                Dictionary<string, List<DataList>> DataSets = new Dictionary<string, List<DataList>>();
                List<EmployeeList> EmployeeList = new List<EmployeeList>();
                Dictionary<int, string> RowCellValues = new Dictionary<int, string>();

                string[] BlankCharList = new string[] { "0", "null", "" }; //This will be used for possible values of blank.

                //Initialized columns of Data Table
                //ClearCreateErrorFields("Personal Info", ref wb);

                var personaSheet = wb.GetSheet("Personal Info");
                var column = personaSheet.GetRow(0);
                var _columns = column.Where(x =>
                                    x.StringCellValue != "Required Columns"
                                    && x.StringCellValue != "Invalid Values"
                                    && x.StringCellValue != "Exception Error"
                                    && x.StringCellValue != "Inserted"
                                    //&& x.StringCellValue != ""
                        ).ToArray();
                
                foreach (var x in _columns)
                {
                    var _PersonaSchema = PersonaSchema.Where(col => col.Name.ToLower() == x.StringCellValue.ToLower()).SingleOrDefault();

                    //Set fields for DataTable
                    dt.Columns.Add(x.StringCellValue.Replace(" ", ""), typeof(string));
                    Columns.Add(new ExcelColumn() {
                        Index = x.ColumnIndex,
                        ColName = x.StringCellValue,
                        IsRequired = _PersonaSchema.IsRequired,
                        DataType = _PersonaSchema.DataType
                    });                    
                    ExcelColumns.Add(x.StringCellValue, x.ColumnIndex);
                    RowCellValues.Add(x.ColumnIndex, "");
                }
                
                using (var sqlcon = new SqlConnection(ConStr))
                {
                    if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                    //Initialize List
                    DataSets.Add("ID_Gender", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tGender", sqlcon));
                    DataSets.Add("ID_SSSStatus", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tSSSStatus", sqlcon));
                    DataSets.Add("ID_Nationality", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tNationality", sqlcon));
                    DataSets.Add("ID_Citizenship", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tCitizenship", sqlcon));
                    DataSets.Add("ID_CivilStatus", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tCivilStatus", sqlcon));
                    DataSets.Add("ID_Religion", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tReligion", sqlcon));
                    DataSets.Add("ID_BloodType", ExecDataList("SELECT Id, dbo.RemoveSpecialCharacters(Name) AS Name FROM dbo.tBloodType", sqlcon));
                    EmployeeList = ExecEmployeeList(@"SELECT p.Id AS ID_Persona, p.Code
                                                    , FirstName, LastName, ISNULL(BirthDate, '01/01/1901 12:00:00 AM') AS BirthDate, p.SSSNo
                                                    , p.PhilHealthNo, p.HDMFNo, p.TINNo, c.Name AS Company, p.Name
                                                    FROM tPersona p
                                                    LEFT OUTER JOIN tCompany c ON c.ID = p.ID_Company", sqlcon);
                    //Truncate Import Persona
                    string CommandText = "DELETE FROM tImportPersona WHERE ID_EtfMigration = " + ID_EtfMigration.ToString();
                    using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlcon))
                    {
                        sqlcmd.ExecuteNonQuery();
                    }

                }

                int idx = 1;
                bool search = true;
                string CurrentColumn = "";
                string InsertStatement = "INSERT INTO tImportPersona (ID_Company, ID_EtfMigration,";
                
                //Compose Insert Statement
                foreach (var col in Columns) InsertStatement += col.ColName + ",";
                InsertStatement = InsertStatement.Substring(0, InsertStatement.Length - 1) + ")";

                try
                {
                    while (search)
                    {
                        Logger.LogMessage($"Validating row {idx}", "Validate_Persona", myLog, EventLogEntryType.Information);
                        //Check Required Fields                        
                        var row = personaSheet.GetRow(idx);
                        string SelectStatement = "SELECT " + ID_Company.ToString() + " AS ID_Company, " + ID_EtfMigration.ToString() + " AS ID_Migration,";
                        //Check if record has no value
                        if (row == null) { search = false; continue; }

                        //Plot Row Cells w/ Initial Data Based On Sheet Rows
                        foreach (var Cell in row.Cells) {
                            if (Cell.CellType == CellType.String || Cell.CellType == CellType.Formula)
                                RowCellValues[Cell.ColumnIndex] = Cell.StringCellValue;
                            else if (Cell.CellType == CellType.Numeric)
                                RowCellValues[Cell.ColumnIndex] = Cell.NumericCellValue.ToString();
                        }

                        //Initialize variables ErrorMessage and CommandText string to used in Execution
                        string ErrorMessage = "";
                        CurrentColumn = ""; // To be used to check where the error occured.

                        //Get Values Of Code, Lastname, Firstname, Middlename and etc. TO BE USED IN VALIDATION
                        string Code = RowCellValues[ExcelColumns["Code"]];
                        string LastName = RowCellValues[ExcelColumns["LastName"]];
                        string FirstName = RowCellValues[ExcelColumns["FirstName"]];
                        string MiddleName = RowCellValues[ExcelColumns["MiddleName"]];
                        DateTime? BirthDate = Convert.ToDateTime(RowCellValues[ExcelColumns["BirthDate"]]);
                        string SSSNo = RemoveInvalidCharacters(RowCellValues[ExcelColumns["SSSNo"]].ToString());
                        string PhilHealthNo = RemoveInvalidCharacters(RowCellValues[ExcelColumns["PhilHealthNo"]].ToString());
                        string HDMFNo = RemoveInvalidCharacters(RowCellValues[ExcelColumns["HDMFNo"]].ToString());
                        string TINNo = RemoveInvalidCharacters(RowCellValues[ExcelColumns["TINNo"]].ToString());

                        //Special Validation, Validate Employee's Data
                        //Check if Firstname, Lastname, Birthdate has the same value but differenct Code in DB
                        var DuplicateEmployee = EmployeeList.Where(x => x.Code != Code
                                                                && (x.FirstName == FirstName
                                                                && x.LastName == LastName
                                                                && x.BirthDate == BirthDate)).FirstOrDefault();
                        if (DuplicateEmployee != null)
                        {
                            //Check if there's already an existing employee
                            ErrorMessage += "Duplicate record with employee Code: [" + DuplicateEmployee.Code
                                    + "], same value {Firstname: " + DuplicateEmployee.FirstName
                                    + ", Lastname: " + DuplicateEmployee.LastName
                                    + ", Birthdate: " + DuplicateEmployee.BirthDate + "}";
                        }

                        var DuplicateSSSNo = EmployeeList.Where(x => x.Code != Code && x.SSSNo == SSSNo && SSSNo.Length > 0).FirstOrDefault();
                        var DuplicateHDMFNo = EmployeeList.Where(x => x.Code != Code && x.HDMFNo == HDMFNo && HDMFNo.Length > 0).FirstOrDefault();
                        var DuplicateTinNo = EmployeeList.Where(x => x.Code != Code && x.TINNo == TINNo && TINNo.Length > 0).FirstOrDefault();
                        var DuplicatePhicNo = EmployeeList.Where(x => x.Code != Code && x.PhilHealthNo == PhilHealthNo && PhilHealthNo.Length > 0).FirstOrDefault();

                        if (DuplicateSSSNo != null)
                            ErrorMessage += $"SSSNo. already exists on [{DuplicateSSSNo.Code}-{DuplicateSSSNo.Name}].";
                        
                        if (DuplicateHDMFNo != null)
                            ErrorMessage += $"HDMFNo. already exists on [{DuplicateHDMFNo.Code}-{DuplicateHDMFNo.Name}].";

                        if (DuplicateTinNo != null)
                            ErrorMessage += $"TINNo. already exists on [{DuplicateTinNo.Code}-{DuplicateTinNo.Name}].";

                        if (DuplicatePhicNo != null)
                            ErrorMessage += $"PHICNo. already exists on [{DuplicatePhicNo.Code}-{DuplicatePhicNo.Name}].";
                        
                        //Validate Employee's FirstName and LastName
                        if (!Regex.IsMatch(RowCellValues[ExcelColumns["LastName"]], @"^[a-zA-Z]+$"))
                            ErrorMessage += "LastName should contain letters only.";

                        if (!Regex.IsMatch(RowCellValues[ExcelColumns["FirstName"]], @"^[a-zA-Z]+$"))
                            ErrorMessage += "FirstName should contain letters only.";

                        foreach (var col in Columns) {

                            //Set value on rawValue
                            dynamic rawValue = null;

                            var rowCellValue = RowCellValues[col.Index]; //Get Value
                            CurrentColumn = col.ColName;
                            if (rowCellValue != null)
                            {
                                if (col.IsRequired)
                                {
                                    if (Array.FindAll(BlankCharList, e => e.Equals(rowCellValue)).Length > 0)
                                        ErrorMessage += col.ColName + " is required.";
                                }

                                switch (col.DataType)
                                {
                                    case "int":
                                        //Validate all Lookup fields
                                        //Hard Code value for ID_Company and ETF_Migration
                                        //Get all fields starts with ID_, indication that it is lookup
                                        if (col.ColName.Substring(0, 3).ToLower() == "id_" && rowCellValue.Length > 0 && DataSets.ContainsKey(col.ColName))
                                        {
                                            rawValue = DataSets[col.ColName].Where(x => x.Name.ToLower() == rowCellValue.ToLower()).SingleOrDefault().Id;

                                            if (rawValue == null)
                                                ErrorMessage += rowCellValue + " is not existing on " + col.ColName.Substring(3, col.ColName.Length - 3) + " lists.";
                                        }

                                        if (col.ColName.Substring(0, 3).ToLower() != "id_")
                                        {
                                            if (rowCellValue != "null")
                                                rawValue = rowCellValue.ToInt32();
                                            else rawValue = null;
                                        }

                                        break;
                                    case "decimal":
                                    case "money":

                                        //If money field is null, zero, space or blank then convert into 0.
                                        
                                        if (Array.FindAll(BlankCharList, e => e.Equals(rowCellValue)).Length > 0 ||
                                            Convert.ToDecimal(rowCellValue) < 0)
                                            rawValue = 0;
                                        else rawValue = Convert.ToDecimal(rowCellValue);

                                        break;

                                    case "bit":

                                        string val = rowCellValue.ToLower();
                                        if (val == "yes" || val == "true" || val == "1")
                                            rawValue = 1; //True Value in MSSQL
                                        else if (val == "no" || val == "false" || val == "0")
                                            rawValue = 0; //False Value in MSSQL
                                        else rawValue = null;

                                        break;
                                    case "varchar":
                                        rawValue = rowCellValue == "null" ? null : "'" + rowCellValue + "'";

                                        break;
                                    case "datetime":
                                        try
                                        {
                                            if (rowCellValue == "")
                                                rawValue = null;
                                            else {
                                                //Check if Value is correct date format.
                                                rawValue = Convert.ToDateTime(rowCellValue);
                                                rawValue = "'" + rawValue.ToString() + "'";
                                            }

                                            
                                        }
                                        catch (Exception ex) {
                                            ErrorMessage += ex.Message + "["+col.ColName+"]";
                                        }
                                        break;
                                }
                            }
                            else {
                                rawValue = "NULL";
                            }


                            if (ErrorMessage.Length > 0)
                            {
                                ErrorMessage += ErrorMessage;
                            }
                            else
                            {
                                //Convert string if null value
                                if (rawValue == null) rawValue = "";

                                if (rawValue.ToString().Length == 0)
                                    rawValue = "NULL";

                                SelectStatement += rawValue + " AS " + col.ColName + ",";
                            }
                        }

                        if (ErrorMessage != "")
                        {
                            ErrorList.Add(new ExcelRowError()
                            {
                                Sheet = SheetType.Company,
                                ColName = "",
                                Index = 0,
                                Message = "Row #" + idx.ToString() + " on column [" + CurrentColumn + "]:" + ErrorMessage,
                                Type = ErrorType.ExceptionError
                            });
                        }
                        else
                        {
                            SelectStatement = SelectStatement.Substring(0, SelectStatement.Length - 1);

                            //Insert on import table 
                            using (var sqlcon = new SqlConnection(ConStr))
                            {
                                if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                                string CommandText = InsertStatement + " " + SelectStatement;
                                using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlcon))
                                {
                                    sqlcmd.ExecuteNonQuery();
                                }
                            }
                        }
                        idx += 1;
                    }
                }
                catch (Exception ex) {
                    string exMessage = (ex.InnerException ?? ex).Message.ToString();
                    Logger.LogError(exMessage + " on row " + idx + " column [" + CurrentColumn + "]", "ValidatePersona_WhileSearch", myLog);
                    search = false;
                    ErrorList.Add( new ExcelRowError()
                    {
                        Sheet = SheetType.Persona,
                        ColName = "",
                        Index = 0,
                        Message = "Persona Search:" + exMessage + " on row " + idx,
                        Type = ErrorType.ExceptionError
                    });
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(message, "ValidatePersona", myLog);
            }
            finally
            {
                //Execute Import Of ETF
                using (var sqlcon = new SqlConnection(ConStr))
                {
                    if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
                    string CommandText = "EXEC pImportETF " + ID_EtfMigration.ToString() + ", " + ID_Company.ToString() + ", 1";
                    using (SqlCommand sqlcmd = new SqlCommand(CommandText, sqlcon))
                    {
                        sqlcmd.ExecuteNonQuery();
                    }
                }
                //var personaSheet = wb.GetSheet("Personal Info");
                //var colHeader = personaSheet.GetRow(0);
                //var exCol = colHeader.Where(x => x.StringCellValue == "Exception Error").FirstOrDefault();
                //ICell cell = null;

                //if (ErrorList.Any(xx => xx.Type == ErrorType.ExceptionError))
                //{
                //    var exErrorList = ErrorList.Where(xx => xx.Type == ErrorType.ExceptionError).ToArray();
                //    for (var x = 0; x < exErrorList.Count(); x++)
                //    {
                //        var row = personaSheet.GetRow(x + 1);
                //        if (row == null) row = personaSheet.CreateRow(x + 1);
                //        cell = row.GetCell(exCol.ColumnIndex);
                //        if (cell == null) cell = row.CreateCell(exCol.ColumnIndex);
                //        cell.SetCellValue(exErrorList[x].Message);
                //    }
                //}
            }
            return dt;
        }
        //private void ClearCreateErrorFields(string SheetName, ref IWorkbook wb)
        //{
        //    var sheet = wb.GetSheet(SheetName);
        //    ICell ErrorHeader = null;

        //    ClearColumn("Inserted", sheet);
        //    ClearColumn("Invalid Values", sheet);
        //    ClearColumn("Required Columns", sheet);
        //    ClearColumn("Exception Error", sheet);

        //    var column = sheet.GetRow(0);
        //    if (!column.Any(x => x.StringCellValue == "Inserted"))
        //    {
        //        ErrorHeader = column.CreateCell(column.LastCellNum);
        //        ErrorHeader.SetCellValue("Inserted");
        //        ICellStyle errorStyle = wb.CreateCellStyle();
        //        errorStyle.FillForegroundColor = HSSFColor.Red.Index;
        //        errorStyle.FillPattern = FillPattern.SolidForeground;
        //        ErrorHeader.CellStyle = errorStyle;
        //    }
        //    column = sheet.GetRow(0);
        //    if (!column.Any(x => x.StringCellValue == "Invalid Values"))
        //    {
        //        ErrorHeader = column.CreateCell(column.LastCellNum);
        //        ErrorHeader.SetCellValue("Invalid Values");
        //        ICellStyle errorStyle = wb.CreateCellStyle();
        //        errorStyle.FillForegroundColor = HSSFColor.Red.Index;
        //        errorStyle.FillPattern = FillPattern.SolidForeground;
        //        ErrorHeader.CellStyle = errorStyle;
        //    }
        //    column = sheet.GetRow(0);
        //    if (!column.Any(x => x.StringCellValue == "Required Columns"))
        //    {
        //        ErrorHeader = column.CreateCell(column.LastCellNum);
        //        ErrorHeader.SetCellValue("Required Columns");
        //        ICellStyle errorStyle = wb.CreateCellStyle();
        //        errorStyle.FillForegroundColor = HSSFColor.Red.Index;
        //        errorStyle.FillPattern = FillPattern.SolidForeground;
        //        ErrorHeader.CellStyle = errorStyle;
        //    }
        //    column = sheet.GetRow(0);
        //    if (!column.Any(x => x.StringCellValue == "Exception Error"))
        //    {
        //        ErrorHeader = column.CreateCell(column.LastCellNum);
        //        ErrorHeader.SetCellValue("Exception Error");
        //        ICellStyle errorStyle = wb.CreateCellStyle();
        //        errorStyle.FillForegroundColor = HSSFColor.Red.Index;
        //        errorStyle.FillPattern = FillPattern.SolidForeground;
        //        ErrorHeader.CellStyle = errorStyle;
        //    }
        //}
        private void ClearColumn(string ErrorColName, ISheet sheet)
        {
            var column = sheet.GetRow(0);
            var col = column.Where(x => x.StringCellValue == ErrorColName).FirstOrDefault();
            if (col != null)
            {
                ICell colCell = column.GetCell(col.ColumnIndex);
                if (colCell != null) colCell.SetCellValue("");

                int idx = 1;
                bool search = true;
                while (search)
                {
                    var row = sheet.GetRow(idx);
                    if (row == null) { search = false; continue; }
                    if (row.Cells.All(x => x.CellType == CellType.Blank)) { search = false; continue; }
                    else
                    {
                        var cells = new List<ICell>();
                        for (int i = 0; i < column.Count(); i++) cells.Add(row.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK));
                        if (row.GetCell(col.ColumnIndex) != null) row.RemoveCell(row.GetCell(col.ColumnIndex));
                    }
                    idx += 1;
                }
                column.RemoveCell(column.GetCell(col.ColumnIndex));
            }
        }
        private void LoadEmployeeColumns(ref DataTable dt)
        {
            dt.Columns.Add("Code", typeof(string)); //0
            dt.Columns.Add("AccessNo", typeof(string));
            dt.Columns.Add("StartDate", typeof(DateTime));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Columns.Add("RegularizationDate", typeof(DateTime));
            dt.Columns.Add("HiredDate", typeof(DateTime)); //5
            dt.Columns.Add("ID_EmployeeStatus", typeof(int));
            dt.Columns.Add("ID_PayrollScheme", typeof(int));
            dt.Columns.Add("ID_PayrollFrequency", typeof(int));
            dt.Columns.Add("ID_TaxExemption", typeof(int));
            dt.Columns.Add("ID_PaymentMode", typeof(int)); //10
            dt.Columns.Add("ID_Parameter", typeof(int));
            dt.Columns.Add("ID_LeaveParameter", typeof(int));
            dt.Columns.Add("MonthlyRate", typeof(decimal));
            dt.Columns.Add("DailyRate", typeof(decimal));
            dt.Columns.Add("ID_Currency", typeof(int)); //15
            dt.Columns.Add("ID_PayrollStatus", typeof(int));
            dt.Columns.Add("ID_PayrollClassification", typeof(int));
            dt.Columns.Add("ID_AccountNumberType", typeof(int));
            dt.Columns.Add("BankAcctNo", typeof(string));
            dt.Columns.Add("ID_CompanyBankAcct", typeof(int)); //20
            dt.Columns.Add("CardNo", typeof(string));
            dt.Columns.Add("TaxRate", typeof(decimal));
            dt.Columns.Add("PrevEmpTaxableAmt", typeof(decimal));
            dt.Columns.Add("PrevEmpWitholdingTax", typeof(decimal));
            dt.Columns.Add("PrevEmpEndDate", typeof(DateTime)); //25
            dt.Columns.Add("Prev13thMonth", typeof(decimal));
            dt.Columns.Add("PrevCompensation", typeof(decimal));
            dt.Columns.Add("NonTaxPrevContribution", typeof(decimal));
            dt.Columns.Add("NonTaxPrevCompensation", typeof(decimal));
            dt.Columns.Add("DailySMW", typeof(decimal)); //30
            dt.Columns.Add("MonthlySMW", typeof(decimal));
            dt.Columns.Add("ID_Company", typeof(string));
            dt.Columns.Add("ID_Branch", typeof(string));
            dt.Columns.Add("ID_Division", typeof(string));
            dt.Columns.Add("ID_Department", typeof(string)); //35
            dt.Columns.Add("ID_Section", typeof(string));
            dt.Columns.Add("ID_JobClassGroup", typeof(string));
            dt.Columns.Add("ID_JobClass", typeof(string));
            dt.Columns.Add("ID_Designation", typeof(string));
            dt.Columns.Add("CompanyMobileNo", typeof(string)); //40
            dt.Columns.Add("CompanyPhoneNo", typeof(string));
            dt.Columns.Add("CompanyEmail", typeof(string)); //42
        }
        private void LoadPersonaColumns(ref DataTable dt)
        {
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("LastName", typeof(string));
            dt.Columns.Add("FirstName", typeof(string));
            dt.Columns.Add("MiddleName", typeof(string));
            dt.Columns.Add("BirthDate", typeof(DateTime));
            dt.Columns.Add("SSSNo", typeof(string));
            dt.Columns.Add("ID_SSSStatus", typeof(int));
            dt.Columns.Add("PhilHealthNo", typeof(string));
            dt.Columns.Add("HDMFNo", typeof(string));
            dt.Columns.Add("TINNo", typeof(string));
            dt.Columns.Add("EmailAddress", typeof(string));
            dt.Columns.Add("AlternateEmailAddress", typeof(string));
            dt.Columns.Add("MobileNo", typeof(string));
            dt.Columns.Add("PhoneNo", typeof(string));
            dt.Columns.Add("ID_Gender", typeof(int));
        }
        private int ExecScalar(string cmd, SqlConnection sqlcon)
        {
            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
            using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon))
            {
                var ret = sqlcmd.ExecuteScalar();
                if (ret == null) return 0;
                else return ret.ToInt32();
            }
        }

        /// <summary>
        /// Use to execute statement "SELECT" that will fetch list of Id and Name
        /// Make sure that selected fields are Id and Name only.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sqlcon"></param>
        /// <returns></returns>
        private List<DataList> ExecDataList(string cmd, SqlConnection sqlcon)
        {
            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
            using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon))
            {
                SqlDataReader DataListReader = sqlcmd.ExecuteReader();
                List<DataList> ReturnDataList = new List<DataList>();

                while (DataListReader.Read()) {
                    ReturnDataList.Add(
                        new DataList()
                        {
                            Id = DataListReader.GetInt32(0),
                            Name = DataListReader.GetString(1)
                        }
                    );
                }

                if (ReturnDataList != null)
                    return ReturnDataList;
                else return null;
            }
        }

        private List<ETFSchema> ExecETFSchema(string cmd, SqlConnection sqlcon)
        {
            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
            using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon))
            {
                SqlDataReader SchemaListReader = sqlcmd.ExecuteReader();
                List<ETFSchema> ReturnSchemaList = new List<ETFSchema>();

                while (SchemaListReader.Read())
                {
                    ReturnSchemaList.Add(
                        new ETFSchema()
                        {
                            Name = SchemaListReader.GetString(0),
                            IsRequired = SchemaListReader.GetBoolean(1),
                            DataType = SchemaListReader.GetString(2),
                            Length = SchemaListReader.GetInt32(3)
                        }
                    );
                }

                if (ReturnSchemaList != null)
                    return ReturnSchemaList;
                else return null;
            }
        }

        private List<EmployeeList> ExecEmployeeList(string cmd, SqlConnection sqlcon) {
            if (sqlcon.State == ConnectionState.Closed) sqlcon.Open();
            using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon))
            {
                SqlDataReader DataListReader = sqlcmd.ExecuteReader();
                List<EmployeeList> ReturnDataList = new List<EmployeeList>();

                while (DataListReader.Read())
                {

                    ReturnDataList.Add(
                            new EmployeeList()
                            {
                                ID_Persona = DataListReader.GetValue(0).ToInt32(),
                                Code = DataListReader.GetValue(1).ToString(),
                                FirstName = DataListReader.GetValue(2).ToString(),
                                LastName = DataListReader.GetValue(3).ToString(),
                                BirthDate = Convert.ToDateTime(DataListReader.GetValue(4).ToString()),
                                SSSNo = DataListReader.GetValue(5).ToString(),
                                PhilHealthNo = DataListReader.GetValue(6).ToString(),
                                HDMFNo = DataListReader.GetValue(7).ToString(),
                                TINNo = DataListReader.GetValue(8).ToString(),
                                Company = DataListReader.GetValue(9).ToString(),
                                Name = DataListReader.GetValue(10).ToString()
                            }
                        );

                    //Code = DataListReader.GetValue(1).IsNull("").ToString(),
                    //FirstName = DataListReader.GetValue(2).IsNull("").ToString(),
                    //LastName = DataListReader.GetValue(3).IsNull("").ToString(),
                    //BirthDate = DataListReader.GetValue(4).ToDate(),
                    //SSSNo = DataListReader.GetValue(5).IsNull("").ToString(),
                    //PhilHealthNo = DataListReader.GetValue(6).IsNull("").ToString(),
                    //HDMFNo = DataListReader.GetValue(7).IsNull("").ToString(),
                    //TINNo = DataListReader.GetValue(8).IsNull("").ToString(),
                    //Company = DataListReader.GetValue(9).IsNull("").ToString(),
                    //Name = DataListReader.GetValue(10).IsNull("").ToString(),

                    //Debug.WriteLine(DataListReader.GetName(0) + " - " + DataListReader.GetValue(0));
                    //Debug.WriteLine(DataListReader.GetName(1) + " - " + DataListReader.GetValue(1));
                    //Debug.WriteLine(DataListReader.GetName(2) + " - " + DataListReader.GetValue(2));
                    //Debug.WriteLine(DataListReader.GetName(3) + " - " + DataListReader.GetValue(3));
                    //Debug.WriteLine(DataListReader.GetName(4) + " - " + Convert.ToDateTime(DataListReader.GetValue(4).ToString()));
                    //Debug.WriteLine(DataListReader.GetName(5) + " - " + DataListReader.GetValue(5));
                    //Debug.WriteLine(DataListReader.GetName(6) + " - " + DataListReader.GetValue(6));
                    //Debug.WriteLine(DataListReader.GetName(7) + " - " + DataListReader.GetValue(7));
                    //Debug.WriteLine(DataListReader.GetName(8) + " - " + DataListReader.GetValue(8));
                    //Debug.WriteLine(DataListReader.GetName(9) + " - " + DataListReader.GetValue(9));
                    //Debug.WriteLine(DataListReader.GetName(10) + " - " + DataListReader.GetValue(10));
                }

                DataListReader.Close();

                if (ReturnDataList != null)
                    return ReturnDataList;
                else return null;
            }
        }

        private string RemoveInvalidCharacters(string text)
        {
            return Regex.Replace(Regex.Replace(text, @"[^a-zA-Z0-9 .]", ""), @"\s{2,}", " ");
        }

    }
    public class ExcelColumn
    {
        public int Index { get; set; }
        public string ColName { get; set; }
        public bool IsRequired { get; set; }
        public string DataType { get; set; }
        public bool IsDateTime { get; set; }
        public bool IsNumber { get; set; }
    }

    public class EmployeeList {
        public int? ID_Persona { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string SSSNo { get; set; }
        public string PhilHealthNo { get; set; }
        public string HDMFNo { get; set; }
        public string TINNo { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
    }

    public class ETFSchema {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }

    }


    public class ExcelRowError
    {
        public int Index { get; set; }
        public string ColName { get; set; }
        public string Message { get; set; }
        public ErrorType Type { get; set; }
        public SheetType Sheet { get; set; }
    }

    /// <summary>
    /// Use as template for selecting values, the fields are Id and Name
    /// </summary>
    public class DataList {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum ErrorType
    {
        Required = 1,
        InvalidValue = 2,
        ExceptionError = 3
    }
    public enum SheetType
    {
        Company = 1,
        Persona = 2
    }
}
