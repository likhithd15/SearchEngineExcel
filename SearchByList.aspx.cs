using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SearchEngineExcel
{
    public partial class SearchByList : System.Web.UI.Page
    {
        DataTable excelDataTable;
        DataTable resultDataTable;
        public List<DataTable> allResults { get; private set; } = new List<DataTable>();

        protected void SearchAndProcess_Click(object sender, EventArgs e)
        {
            // Set the LicenseContext to suppress the license exception
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


            bool sourceFileUploadError = false;
            bool searchFileUploadError = false;
            bool searchColumnError = string.IsNullOrEmpty(columnSearchInSearchFileTextBox.Text); 
            bool sourceColumnError = string.IsNullOrEmpty(columnSearchTextBox.Text); 


            // Validate file format
            foreach (HttpPostedFile postedFile in fileUpload.PostedFiles)
            {
                if (Path.GetExtension(postedFile.FileName) != ".xlsx")
                {
                    sourceFileUploadError = true;
                    break; // Break the loop as soon as an invalid file format is found
                }
            }

            foreach (HttpPostedFile postedFile in searchFileUpload.PostedFiles)
            {
                if (Path.GetExtension(postedFile.FileName) != ".xlsx")
                {
                    searchFileUploadError = true;
                    break; // Break the loop as soon as an invalid file format is found
                }
            }

            // Handle error messages based on validation
            searchColumnErrorLabel.Visible = searchColumnError;
            searchColumnErrorLabel.Text = searchColumnError ? "Please enter a column name." : string.Empty;

            sourceColumnErrorLabel.Visible = sourceColumnError;
            sourceColumnErrorLabel.Text = sourceColumnError ? "Please enter a column name." : string.Empty;

            sourceErrorLabel.Visible = sourceFileUploadError;
            sourceErrorLabel.Text = sourceFileUploadError ? "Please upload only Excel files (.xlsx)." : string.Empty;

            searchErrorLabel.Visible = searchFileUploadError;
            searchErrorLabel.Text = searchFileUploadError ? "Please upload only Excel files (.xlsx)." : string.Empty;

            // Check for any errors and return if found
            if (searchColumnError || sourceColumnError || sourceFileUploadError || searchFileUploadError)
            {
                return; // Return if any of the validations failed
            }

            //List to store the end search result

            //If you check the search by Name checkbox

            DataTable searchDataTable = null;
            if (searchFileUpload.HasFiles)
            {
                HttpPostedFile postedSearchFile = searchFileUpload.PostedFile;
                searchDataTable = ProcessSearchFile(postedSearchFile);

            }


            foreach (HttpPostedFile postedFile in fileUpload.PostedFiles)
            {
                List<DataTable> resultDataTable = ProcessFile(postedFile, searchDataTable);
                foreach (DataTable result in resultDataTable)
                {
                    allResults.Add(result);
                }
            }
            Session["SearchResults"] = allResults;

            // Redirect to the other webpage
            Response.Redirect("ResultPage.aspx");

        }

        private DataTable ProcessSearchFile(HttpPostedFile postedSearchFile)
        {
            DataTable searchDataTable = new DataTable();
            using (Stream stream = postedSearchFile.InputStream)
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    searchDataTable.Columns.Add(firstRowCell.Text);
                }

                for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    DataRow row = searchDataTable.Rows.Add();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        row[col - 1] = worksheet.Cells[rowNumber, col].Text;
                    }
                }
            }
            return searchDataTable;
        }

        private List<DataTable> ProcessFile(HttpPostedFile postedFile, DataTable searchDataTable)
        {
            List<DataTable> filteredSheets = new List<DataTable>();

            using (Stream stream = postedFile.InputStream)
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    excelDataTable = new DataTable();

                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        string cellValue = worksheet.Cells[1, col].Text;
                        excelDataTable.Columns.Add(cellValue);
                    }

                    for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var rowCells = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];

                        DataRow row = excelDataTable.Rows.Add();

                        for (int cellIndex = 0; cellIndex < rowCells.Count(); cellIndex++)
                        {
                            if (cellIndex < excelDataTable.Columns.Count)
                            {
                                row[cellIndex] = rowCells.ElementAt(cellIndex).Text;
                            }
                        }
                    }


                    excelDataTable.TableName = worksheet.Name; // Preserve the sheet name
                    DataTable filteredData = ApplySearchByNameCriteria(searchDataTable);


                    // Add filtered data to the list for each sheet
                    filteredSheets.Add(filteredData);
                }
            }

            return filteredSheets;
        }

        private DataTable ApplySearchByNameCriteria(DataTable searchCriteria)
        {
            string[] columnNames = columnSearchTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(c => c.Trim())
                                            .ToArray();
            string searchColumnName = columnSearchInSearchFileTextBox.Text.Trim();
            if(columnNames.Any(name => !excelDataTable.Columns.Contains(name)) && !searchCriteria.Columns.Contains(searchColumnName))
            {
                return GetErrorDataTable("Column name does not exist in both uploaded source file and search list.");
            }
            else if (columnNames.Any(name => !excelDataTable.Columns.Contains(name)))
            {
                return GetErrorDataTable("Column name does not exist in uploaded source file.");
            }
            else if (!searchCriteria.Columns.Contains(searchColumnName))
            {
                return GetErrorDataTable("Column name does not exist in uploaded search list.");
            }
            else
            {

                resultDataTable = excelDataTable.Clone();

                if (excelDataTable == null || excelDataTable.Rows.Count == 0 || searchCriteria == null || searchCriteria.Rows.Count < 2 || string.IsNullOrEmpty(columnSearchTextBox.Text) || string.IsNullOrEmpty(columnSearchInSearchFileTextBox.Text))
                {
                    // Return the source data if there are missing or inadequate criteria
                    return excelDataTable;
                }
                HashSet<object> uniqueRows = new HashSet<object>();

                int nameColumnIndex = -1;
                for (int col = 0; col < searchCriteria.Columns.Count; col++)
                {
                    if (string.Equals(searchCriteria.Columns[col].ColumnName.Trim(), searchColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        nameColumnIndex = col;
                        break;
                    }
                    //col = 0;
                }

                if (nameColumnIndex == -1)
                {
                    // Handle scenario if 'Name' column is not found in searchCriteria
                    return excelDataTable;
                }
                resultDataTable = excelDataTable.Clone();
                var rowsToImport = searchCriteria.AsEnumerable().Skip(0) // Skip the first row (column names)
              .Where(row => row.Field<string>(nameColumnIndex) != null)
              .Select(row => row.Field<string>(nameColumnIndex).Trim().ToLower());


                foreach (var criteria in rowsToImport)
                {

                    string filter = string.Join(" OR ", columnNames
        .Where(name => excelDataTable.Columns.Contains(name))
        .Select(name =>
        {
            string escapedCriteria = criteria.Replace("'", "''"); // Escape single quotes
        return $"[{name}] = '{escapedCriteria}'";

        }));



                    // Perform the search using the combined filter across multiple columns
                    DataRow[] matchingRow = excelDataTable.Select(filter);

                    if (matchingRow.Length > 0)
                    {
                        // Create a resultDataTable with the same structure as excelDataTable


                        // Import matching rows into resultDataTable
                        foreach (DataRow row in matchingRow)
                        {
                            int rowIndex = excelDataTable.Rows.IndexOf(row);

                            if (!uniqueRows.Contains(rowIndex))
                            {
                                resultDataTable.ImportRow(row);
                                uniqueRows.Add(rowIndex);
                            }
                        }
                    }

                    else
                    {
                        foreach (var columnName in columnNames)
                        {
                            if (excelDataTable.Columns.Contains(columnName))
                            {
                                // Modify the search terms to remove the first character if it is a special character


                                var matchingRows = excelDataTable.AsEnumerable().Where(row =>
                                {
                                    string cellValue = row.Field<string>(columnName);
                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                    // Normalize the cell value by making it lowercase
                                    string normalizedCellValue = cellValue.ToLower();
                                    // string normalizedCriteria = criteria.ToLower();
                                    string[] words = normalizedCellValue.Split(' ');

                                    // Check if any of the words start with the search criteria
                                    //bool startsWithCriteria = words.Any(word => word.StartsWith(normalizedCriteria));

                                    // Calculate similarity using FuzzySharp's FuzzySearch method
                                    double similarity = FuzzySharp.Fuzz.PartialRatio(normalizedCellValue, criteria);

                                    // Adjust the thresholds based on your needs
                                    if (similarity > 98)
                                        {
                                            var searchTerms = SplitWords(criteria.ToLower()).Select(term =>
                                            {
                                                if (term.Length > 1 && !char.IsLetterOrDigit(term[0]))
                                                {
                                                    return term.Substring(1);
                                                }
                                                return term;
                                            }).ToList();
                                            string[] cellValueWords = normalizedCellValue.Split(' ');
                                            foreach (string part in searchTerms)
                                            {
                                            // Calculate similarity for each substring
                                            foreach (string cellValueWord in cellValueWords)
                                                {
                                                // Adjust the comparison based on your needs (e.g., case-insensitive comparison)
                                                if (cellValueWord.Equals(part, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                    // If any word has a partial match, return true
                                                    return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    return false;
                                }).ToArray();

                                if (matchingRows.Length > 0)
                                {
                                    // Import matching rows into resultDataTable
                                    foreach (DataRow row in matchingRows)
                                    {
                                        int rowIndex = excelDataTable.Rows.IndexOf(row);

                                        if (!uniqueRows.Contains(rowIndex))
                                        {
                                            resultDataTable.ImportRow(row);
                                            uniqueRows.Add(rowIndex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return resultDataTable;
            }
        }

            private DataTable GetErrorDataTable(string errorMessage)
            {
                // Create a DataTable with a single column "Error" to store the error message
                DataTable errorDataTable = new DataTable();
                errorDataTable.Columns.Add("Error", typeof(string));

                // Add a row with the error message
                DataRow errorRow = errorDataTable.NewRow();
                errorRow["Error"] = errorMessage;
                errorDataTable.Rows.Add(errorRow);

                return errorDataTable;
            }

            // Function to split a string into words based on spaces and special characters
            IEnumerable<string> SplitWords(string input)
        {
            string pattern = "[\\s\\W]+"; // Matches one or more whitespace or non-word characters
            string[] wordParts = Regex.Split(input, pattern);

            foreach (string wordPart in wordParts)
            {
                if (!string.IsNullOrWhiteSpace(wordPart) && !ShouldExcludeWord(wordPart))
                {
                    yield return wordPart;
                }
            }
        }

        bool ShouldExcludeWord(string word)
        {
            // List of words to exclude
            string[] exclusionList = { "llp", "com", "inc", "lp", "ltd", "llc" };

            // Check if the word is in the exclusion list
            return exclusionList.Contains(word.ToLower());
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}