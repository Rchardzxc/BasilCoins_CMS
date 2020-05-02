using InSys.Helper;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using z.Data;

namespace InSys.ITI.InitialMigration
{
    public class ETF
    {
        public int lastColumnIndex { get; private set; } = 0;
        public List<string> ColumnLetters { get; private set; }
        public string FileName { get; private set; }
        public ETF() { }
        public ETF(string _fileName)
        {
            FileName = _fileName;
        }
        
        public virtual string GenerateTemplate(int ID_Company)
        {
            IWorkbook workbook = new XSSFWorkbook();
            //var tmp = Path.GetTempFileName().ToString().Replace(".tmp", "") + ".xls";
            try
            {
                using (var strg = new Storage.Storage())
                {
                    var cntr = strg.Container("ExcelTemplates");
                    using (var ms = new MemoryStream())
                    {
                        if (!strg.Exists(cntr, FileName.IsNull("ETF.xlsx").ToString()))
                            throw new Exception("Excel template not found.");
                        strg.DownloadToStream(cntr, FileName.IsNull("ETF.xlsx").ToString(), ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        workbook = WorkbookFactory.Create(ms);
                    }
                }
                if (workbook == null) throw new Exception("There is a problem on loading ETF Template.");

                //GetExcelAddress();

                using (var ms = new MemoryStream())
                {
                    try
                    {
                        workbook.Write(ms);
                        workbook.Close();
                    }
                    catch (Exception ex)
                    {
                        workbook.Close();
                        throw ex;
                    }
                    string guid = "ETF.xlsx"; //Guid.NewGuid().ToString().Replace("-", "") + DateTime.Now.ToString("yyyy-MM-dd_HH:mm")
                    using (var strg = new Storage.Storage())
                    {
                        var cntr = strg.Container("Files");
                        //while (strg.Exists(cntr, guid)) guid = Guid.NewGuid().ToString().Replace("-", "") + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xlsx";
                        ms.Seek(0, SeekOrigin.Begin);
                        strg.Upload(cntr, guid, ms);
                    }
                    return guid;
                }
            }
            catch (Exception ex)
            {
                if(workbook != null) workbook.Close();
                throw ex;
            }
        }
        public string RemoveInvalidCharacters(string text)
        {
            return Regex.Replace(Regex.Replace(text, @"[^a-zA-Z0-9 .]", ""), @"\s{2,}", " ");
        }
        public List<string> GetAplhaList(string current, int ite, int max, string alphabet)
        {
            if (ite >= max) return new List<string>();

            var nk = new List<string>();
            for (var i = 0; i < alphabet.Length; i++)
            {
                var g = $"{current}{ alphabet[i] }";
                nk.Add(g);

                nk.AddRange(GetAplhaList(g, ite + 1, max, alphabet));
            }
            return nk;
        }
        public void GetExcelAddress()
        {
            var iterations = 2;
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            ColumnLetters = new List<string>();

            for (var i = 0; i < alphabet.Length; i++)
            {
                var g = alphabet[i].ToString();
                ColumnLetters.Add(g);
                ColumnLetters.AddRange(GetAplhaList(g, 0, iterations, alphabet));
            }
            ColumnLetters = ColumnLetters.OrderBy(x => x.Length).ThenBy(x => x).ToList();
        }

        public void GenerateLookup(ref IWorkbook wb, ISheet sheet, DataTable dt, int cellIdx, string label)
        {
            IRow row = null;

            lastColumnIndex = cellIdx;

            int rowIdx = 0;

            int rowCount = sheet.LastRowNum;
            if (rowCount <= rowIdx) row = sheet.CreateRow(rowIdx);
            else row = sheet.GetRow(rowIdx);

            ICell cell = row.CreateCell(cellIdx);

            ICellStyle cs = wb.CreateCellStyle();
            cs.FillForegroundColor = HSSFColor.Yellow.Index;
            cs.FillPattern = FillPattern.SolidForeground;

            cell.SetCellValue(label);
            cell.CellStyle = cs;

            rowIdx += 1;

            if (dt.Rows.Count == 0) return;
            foreach (DataRow dr in dt.Rows)
            {
                int rc = sheet.LastRowNum + 1;
                if (rowCount <= rowIdx) row = sheet.CreateRow(rowIdx);
                else row = sheet.GetRow(rowIdx);

                ICell cc = row.CreateCell(cellIdx);

                cc.SetCellValue(RemoveInvalidCharacters(dr["Name"].ToString()));
                rowIdx += 1;
            }
        }
    }
}
