using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace SearchEngineExcel
{
    public partial class SearchByName : System.Web.UI.Page
    {
        string searchValue;
        DataTable excelDataTable;
        DataTable resultDataTable;
        public List<DataTable> allResults { get; private set; } = new List<DataTable>();

        protected void SearchAndProcess_Click(object sender, EventArgs e)
        {
            // Set the LicenseContext to suppress the license exception
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            searchValue = searchQuery.Text;

            // Validation to check if source, destination, or search value is empty
            bool searchError = string.IsNullOrEmpty(searchQuery.Text);
            bool columnError = string.IsNullOrEmpty(columnSearchTextBox.Text);
            bool fileUploadError = false;

            // Validate file format
            foreach (HttpPostedFile postedFile in fileUpload.PostedFiles)
            {
                if (Path.GetExtension(postedFile.FileName) != ".xlsx")
                {
                    fileUploadError = true;
                    break; // Break the loop as soon as an invalid file format is found
                }
            }

            // Handle error messages based on validation
            searchErrorLabel.Visible = searchError;
            searchErrorLabel.Text = searchError ? "Please enter a search term." : string.Empty;

            columnErrorLabel.Visible = columnError;
            columnErrorLabel.Text = columnError ? "Please enter a column name." : string.Empty;

            sourceErrorLabel.Visible = fileUploadError;
            sourceErrorLabel.Text = fileUploadError ? "Please upload only Excel files (.xlsx)." : string.Empty;

            // Check for any errors and return if found
            if (fileUploadError || columnError || searchError)
            {
                return; // Return if any of the validations failed
            }

            //List to store the end search result

            //If you check the search by Name checkbox

            foreach (HttpPostedFile postedFile in fileUpload.PostedFiles)
            {
                //DataTable resultDataTable = ProcessFile(postedFile);
                List<DataTable> resultDataTable = ProcessMultiSheetFile(postedFile);
                foreach (DataTable result in resultDataTable)
                {
                    allResults.Add(result);
                }
                
            }

           

            Session["SearchResults"] = allResults;

            // Redirect to the other webpage
            Response.Redirect("ResultPage.aspx");

        }

        // Function to process each individual file and sheets and return its DataTables result seperately
        private List<DataTable> ProcessMultiSheetFile(HttpPostedFile postedFile)
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
                    DataTable filteredData = ApplySearchByNameCriteria();


                    // Add filtered data to the list for each sheet
                    filteredSheets.Add(filteredData);
                }
            }

            return filteredSheets;
        }

        // Function to apply search criteria on each sheet's data
        private DataTable ApplySearchByNameCriteria()
        {
            string[] columnNames = columnSearchTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(c => c.Trim())
                                            .ToArray();
            HashSet<object> uniqueRows = new HashSet<object>();
            resultDataTable = excelDataTable.Clone();

            if (columnNames.Any(name => !excelDataTable.Columns.Contains(name)))
            {
                return GetErrorDataTable("Column name does not exist.");
            }

            else
            {
                

                string filter = string.Join(" OR ", columnNames.Where(name => excelDataTable.Columns.Contains(name)).Select(name =>
        $"([{name}] LIKE '%{searchValue}%' OR [{name}] LIKE '%{searchValue}.%')"));

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
                    resultDataTable = excelDataTable.Clone();

                    foreach (var columnName in columnNames)
                    {
                        if (excelDataTable.Columns.Contains(columnName))
                        {
                            // Modify the search terms to remove the first character if it is a special character
                            var searchTerms = SplitWords(searchValue.ToLower()).Select(term =>
                            {
                                if (term.Length > 1 && !char.IsLetterOrDigit(term[0]))
                                {
                                    return term.Substring(1);
                                }
                                return term;
                            }).ToList();

                            var matchingRows = excelDataTable.AsEnumerable().Where(row =>
                            {
                                string cellValue = row.Field<string>(columnName);
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    // Normalize the cell value by making it lowercase
                                    string normalizedCellValue = cellValue.ToLower();

                                    // Calculate similarity using FuzzySharp's FuzzySearch method
                                    // You can adjust the threshold (e.g., 50) based on your needs
                                    double similarity = FuzzySharp.Fuzz.PartialRatio(normalizedCellValue, searchValue.ToLower());

                                    // You can adjust the threshold (e.g., 50) based on your needs
                                    if (similarity > 90)
                                        return similarity > 90;    // Adjust the threshold as needed
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