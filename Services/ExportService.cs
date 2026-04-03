using APM.API.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;

        public ExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportToExcelAsync()
        {
            var actions = await _context.ActionItems
                .Include(a => a.ActionPlan)
                .Include(a => a.Responsible)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Actions APM");

            // En-têtes
            sheet.Cell(1, 1).Value = "ID";
            sheet.Cell(1, 2).Value = "Thème";
            sheet.Cell(1, 3).Value = "Plan d'action";
            sheet.Cell(1, 4).Value = "Responsable";
            sheet.Cell(1, 5).Value = "Statut";
            sheet.Cell(1, 6).Value = "Type";
            sheet.Cell(1, 7).Value = "Criticité";
            sheet.Cell(1, 8).Value = "Échéance";
            sheet.Cell(1, 9).Value = "Avancement %";

            // Style en-têtes
            var headerRow = sheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2B5FA3");
            headerRow.Style.Font.FontColor = XLColor.White;

            // Données
            for (int i = 0; i < actions.Count; i++)
            {
                var a = actions[i];
                sheet.Cell(i + 2, 1).Value = a.Id;
                sheet.Cell(i + 2, 2).Value = a.Theme;
                sheet.Cell(i + 2, 3).Value = a.ActionPlan?.Title ?? "";
                sheet.Cell(i + 2, 4).Value = a.Responsible?.FullName ?? "";
                sheet.Cell(i + 2, 5).Value = a.Status;
                sheet.Cell(i + 2, 6).Value = a.Type;
                sheet.Cell(i + 2, 7).Value = a.Criticity;
                sheet.Cell(i + 2, 8).Value = a.Deadline.ToString("dd/MM/yyyy");
                sheet.Cell(i + 2, 9).Value = a.ProgressPercentage;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}