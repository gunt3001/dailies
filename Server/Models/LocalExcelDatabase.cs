using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dailies.Shared;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace dailies.Server.Models
{
    public class LocalExcelDatabase : IDatabase
    {
        public string Path { get; }
        public string Worksheet { get; }
        public string TableName { get; }

        /// <summary>
        /// Opens Excel file-based database
        /// The file must exist.
        /// </summary>
        /// <param name="path">Path to the Excel file</param>
        /// <param name="worksheet">Name of the worksheet containing entries table</param>
        /// <param name="tableName">Name of the table containing entries</param>
        public LocalExcelDatabase(string path, string worksheet = "Dailies", string tableName = "DailiesTable")
        {
            Path = path;
            Worksheet = worksheet;
            TableName = tableName;
        }

        public Entry GetEntry(DateTime date)
        {
            using var table = new ExcelWorksheetWrapper(Path, Worksheet, TableName);

            // Look for entry with specified date
            foreach (var row in table.EnumerateRows())
            {
                var dateCell = table.GetCellValue(row, table.DateColIndex);
                if (dateCell is DateTime entryDate && entryDate.Date == date.Date)
                {
                    return CreateEntryAtRowIndex(table, entryDate, row);
                }
            }

            // Return null if not found
            return null;
        }

        public Entry GetRandomEntry()
        {
            using var table = new ExcelWorksheetWrapper(Path, Worksheet, TableName);
            var rows = table.EnumerateRows().ToList();
            var rowCount = rows.Count;
            var randomRowIndex = new Random().Next(0, rowCount);
            var dateCell = table.GetCellValue(randomRowIndex, table.DateColIndex);
            return CreateEntryAtRowIndex(table, (DateTime)dateCell, randomRowIndex);
        }

        public IEnumerable<Entry> GetEntries(DateTime startDate, DateTime endDate)
        {
            using var table = new ExcelWorksheetWrapper(Path, Worksheet, TableName);

            // Look for entry with specified date
            foreach (var row in table.EnumerateRows())
            {
                var dateCell = table.GetCellValue(row, table.DateColIndex);
                if (dateCell is DateTime entryDate && entryDate.Date >= startDate.Date && entryDate.Date <= endDate)
                {
                    var entry = CreateEntryAtRowIndex(table, entryDate, row);
                    if (entry != null) {
                        yield return entry;
                    }
                }
            }
        }

        public IEnumerable<Entry> GetEntries(int year, int month)
        {
            return GetEntries(new DateTime(year, month, 1),
                new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        }

        public AddEntryResult AddEntry(Entry newEntry)
        {
            throw new NotImplementedException();
        }

        public bool UpdateEntry(Entry entry)
        {
            throw new NotImplementedException();
        }

        private Entry CreateEntryAtRowIndex(ExcelWorksheetWrapper table, DateTime entryDate, int row)
        {
            var content = table.GetCellValue(row, table.ContentColIndex)?.ToString();
            // Ignore cells with empty contents
            if (string.IsNullOrWhiteSpace(content)) return null;

            return new Entry
            {
                Date = entryDate,
                Content = content,
                Keyword = table.GetCellValue(row, table.KeywordColIndex)?.ToString(),
                Mood = table.GetCellValue(row, table.MoodColIndex)?.ToString(),
                Remarks = table.GetCellValue(row, table.RemarksColIndex)?.ToString(),
            };
        }

        private class ExcelWorksheetWrapper : IDisposable
        {
            private ExcelPackage ExcelPackage { get; }
            private ExcelTable ExcelTable { get; }

            private ExcelCellAddress TableOrigin { get; }
            private ExcelCellAddress TableEnd { get; }

            public int DateColIndex { get; }
            public int ContentColIndex { get; }
            public int KeywordColIndex { get; }
            public int MoodColIndex { get; }
            public int RemarksColIndex { get; }

            public ExcelWorksheetWrapper(string path, string worksheet, string tableName)
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException();
                }

                ExcelPackage = new ExcelPackage(fileInfo);
                var excelWorksheet = ExcelPackage.Workbook.Worksheets[worksheet];
                ExcelTable = excelWorksheet.Tables[tableName];

                // Identify columns
                TableOrigin = ExcelTable.Address.Start;
                TableEnd = ExcelTable.Address.End;

                DateColIndex = GetColIndex("Date");
                ContentColIndex = GetColIndex("Activity");
                KeywordColIndex = GetColIndex("Keyword");
                MoodColIndex = GetColIndex("Mood");
                RemarksColIndex = GetColIndex("Remarks");
            }

            public IEnumerable<int> EnumerateRows()
            {
                for (var rowIndex = TableOrigin.Row + 1; rowIndex <= TableEnd.Row; rowIndex++)
                {
                    yield return rowIndex;
                }
            }

            public object GetCellValue(int row, int col)
            {
                return ExcelTable.WorkSheet.Cells[row, col].Value;
            }

            private int GetColIndex(string colName)
            {
                return TableOrigin.Column + ExcelTable.Columns[colName].Position;
            }

            public void Dispose()
            {
                ExcelPackage?.Dispose();
            }
        }
    }
}
