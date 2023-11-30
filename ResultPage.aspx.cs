using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.UI;

namespace SearchEngineExcel
{
    public partial class ResultPage : Page
    {
        List<DataTable> searchResults;
        protected void Page_Load(object sender, EventArgs e)
        {
            searchResults = Session["SearchResults"] as List<DataTable>;
            DataTable resultDataTable = searchResults[0];

            if (resultDataTable.Rows.Count != 0)
            {

                // Display the results in the GridView
                if (resultDataTable.Columns.Contains("Error"))
                {
                    GridView1.DataSource = resultDataTable;
                    GridView1.DataBind();
                }
                else
                {
                    // Display the results in the GridView
                    GridView1.DataSource = resultDataTable;
                    GridView1.DataBind();
                    RegisterStartupScript();
                }
            }

            else
            {
                errorLabel1.Visible = true;
                errorLabel1.Text = "--No Results found--";
            }
        }

        protected void ExistingSheet_Click(object sender, EventArgs e)
        {
            string destinationFilePath = destinationFile.Text;
            if (!string.IsNullOrEmpty(destinationFilePath))
            {
                bool destinationError = !Directory.Exists(Path.GetDirectoryName(destinationFilePath)) || !Path.IsPathRooted(destinationFilePath);

                destinationErrorLabel.Visible = destinationError;
                destinationErrorLabel.Text = "Invalid or inaccessible destination path.";

            }

            else
            {
                destinationErrorLabel.Visible = false;
                destinationErrorLabel.Text = string.Empty;
                return;
            }
            using (ExcelPackage package = new ExcelPackage(new FileInfo(destinationFilePath)))
            {
                string sheetName = sheetNameTextBox.Text.Trim(); // Get the value from the TextBox
                foreach (DataTable combinedResults in searchResults)
                {

                    ExcelWorksheet existingWorksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);

                    if (existingWorksheet == null)
                    {
                        if (!string.IsNullOrEmpty(sheetNameTextBox.Text))
                        {
                            errorLabel.Visible = false;
                            existingWorksheet = package.Workbook.Worksheets.Add(sheetName);
                        }
                        else
                        {
                            errorLabel.Visible = true;
                            errorLabel.Text = "Please add the 'Sheet name'";
                            return;

                        }
                    }

                    if (existingWorksheet.Dimension != null)
                    {
                        int startRow = existingWorksheet.Dimension.End.Row + 2;

                        existingWorksheet.Cells["A" + startRow].LoadFromDataTable(combinedResults, true);
                    }
                    else
                    {
                        // If no existing data, simply add the data to the first row
                        existingWorksheet.Cells["A1"].LoadFromDataTable(combinedResults, true);
                    }
                }
                package.Save();
            }
        }

        protected void NewSheet_Click(object sender, EventArgs e)
        {
            string destinationFilePath = destination.Text;
            if (!string.IsNullOrEmpty(destinationFilePath))
            {
                bool destinationErrors = !Directory.Exists(Path.GetDirectoryName(destinationFilePath)) || !Path.IsPathRooted(destinationFilePath);

                destinationError.Visible = destinationErrors;
                destinationError.Text = "Invalid or inaccessible destination path.";

            }

            else
            {
                destinationError.Visible = false;
                destinationError.Text = string.Empty;
                return;
            }
            using (ExcelPackage package = new ExcelPackage(new FileInfo(destinationFilePath)))
            {
                foreach (DataTable combinedResults in searchResults)
                {
                    // Save each result in a different sheet
                    string sheetName = combinedResults.TableName + DateTime.Now.ToString("MMddHHmmss");

                    ExcelWorksheet existingWorksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);

                    if (existingWorksheet == null)
                    {
                        existingWorksheet = package.Workbook.Worksheets.Add(sheetName);
                    }

                    // Append the data to the existing worksheet
                    if (combinedResults.Columns.Count > 0)
                    {
                        existingWorksheet.Cells["A1"].LoadFromDataTable(combinedResults, true);
                    }
                    else
                    {
                        // If the data table has no columns, load data without column names
                        for (int row = 0; row < combinedResults.Rows.Count; row++)
                        {
                            for (int col = 0; col < combinedResults.Columns.Count; col++)
                            {
                                existingWorksheet.Cells[row + 1, col + 1].Value = combinedResults.Rows[row][col];
                            }
                        }
                    }


                }
                package.Save();
            }
        }
        private void RegisterStartupScript()
        {
            // Render the JavaScript code only if there's no error
            string scriptBlock = @"
        $(document).ready(function () {
            $('.table').prepend($('<thead></thead>').append($(this).find('tr:first'))).dataTable({
                fixedHeader: {
            header: true,
            footer: false
        },
        scrollX: true,
        scrollY: '400px', 
                scrollCollapse: true,
                paging: true // Disable pagination if needed
            });
        });

        $(document).ready(function () {
            $('#existingSheet').click(function () {
                $('#existingModal').modal();
            });
        });

        $(document).ready(function () {
            $('#newSheet').click(function () {
                $('#newModal').modal();
            });
        });
    ";

            Page.ClientScript.RegisterStartupScript(this.GetType(), "MyScript", scriptBlock, true);
        }

    }
}
