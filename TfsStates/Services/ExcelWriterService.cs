using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Excel;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class ExcelWriterService : IExcelWriterService
    {
        public void Write(string filename, List<TfsInfo> items, string projectBaseUrl)
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.AddWorksheet("TFS");
                SetupHeaderRow(worksheet, XLColor.LightGoldenrodYellow);

                var csvConfig = new CsvConfiguration { QuoteNoFields = true };

                using (var writer = new CsvWriter(new ExcelSerializer(worksheet, csvConfig)))
                {
                    writer.WriteRecords(items);
                }

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var urlCell = row.Cell(1);
                    var tfsId = urlCell.Value.ToString();

                    if (!string.IsNullOrEmpty(tfsId))
                    {
                        var url = $"{projectBaseUrl}/_workitems/edit/{tfsId}";
                        urlCell.Hyperlink = new XLHyperlink(url, url);
                    }

                    var transitionCell = row.Cell(5);
                    transitionCell.Style.Font.FontSize = 9;

                    row.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Column(2).Width = 75;
                worksheet.Column(7).Width = 105;
                worksheet.Rows().AdjustToContents();

                workbook.SaveAs(filename);
            }
        }

        private static void SetupHeaderRow(IXLWorksheet worksheet, XLColor color)
        {
            var headerRow = worksheet.FirstRow();
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = color;
            worksheet.SheetView.FreezeRows(1);
        }
    }
}
