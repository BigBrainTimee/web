using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelService.Dtos;

namespace TravelService.Services;

public class PlanReportPdfGenerator : IPlanReportPdfGenerator
{
    private static readonly Dictionary<string, string> ExpenseCategoryLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Transport"] = "Prevoz",
        ["Accommodation"] = "Smeštaj",
        ["Food"] = "Hrana",
        ["Tickets"] = "Karte",
        ["Shopping"] = "Kupovina",
        ["Other"] = "Ostalo",
    };

    public byte[] Generate(TravelPlanReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, report.Plan));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Planer putovanja — ");
                    text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")).FontColor(Colors.Grey.Medium);
                    text.Span("  |  Strana ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, TravelPlanResponseDto plan)
    {
        container.Column(column =>
        {
            column.Item().Text("Izveštaj plana putovanja")
                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(4).Text(plan.Name).FontSize(14).SemiBold();
            column.Item().Text($"{plan.StartDate:dd.MM.yyyy} — {plan.EndDate:dd.MM.yyyy}")
                .FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, TravelPlanReportDto report)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(16);
            ComposePlanOverview(column, report.Plan);
            ComposeBudget(column, report.BudgetSummary, report.Expenses);
            ComposeDestinations(column, report.Destinations);
            ComposeActivities(column, report.Activities);
            ComposeChecklist(column, report.ChecklistItems);
        });
    }

    private static void ComposePlanOverview(ColumnDescriptor column, TravelPlanResponseDto plan)
    {
        column.Item().Element(c => SectionTitle(c, "Pregled plana"));
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(120);
                columns.RelativeColumn();
            });

            AddInfoRow(table, "Planirani budžet", $"{plan.PlannedBudget:N2} €");

            if (!string.IsNullOrWhiteSpace(plan.Description))
            {
                AddInfoRow(table, "Opis", plan.Description);
            }

            if (!string.IsNullOrWhiteSpace(plan.Notes))
            {
                AddInfoRow(table, "Napomene", plan.Notes);
            }
        });
    }

    private static void ComposeBudget(
        ColumnDescriptor column,
        BudgetSummaryDto? summary,
        IReadOnlyList<ExpenseResponseDto> expenses)
    {
        column.Item().Element(c => SectionTitle(c, "Budžet i troškovi"));

        if (summary is null)
        {
            column.Item().Text("Nema podataka o budžetu.").Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => StatBox(c, "Planirano", $"{summary.PlannedBudget:N2} €"));
            row.RelativeItem().Element(c => StatBox(c, "Sigurno", $"{summary.TotalSpent:N2} €"));
            row.RelativeItem().Element(c => StatBox(c, "Procenjeno", $"{summary.TotalEstimated:N2} €"));
            row.RelativeItem().Element(c => StatBox(c, "Preostalo", $"{summary.Remaining:N2} €"));
        });

        if (summary.ByCategory.Count > 0)
        {
            column.Item().PaddingTop(8).Text("Po kategorijama").SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Kategorija");
                    header.Cell().Element(CellHeader).AlignRight().Text("Iznos");
                });

                foreach (var category in summary.ByCategory)
                {
                    table.Cell().Element(CellBody).Text(FormatExpenseCategory(category.Category));
                    table.Cell().Element(CellBody).AlignRight().Text($"{category.Amount:N2} €");
                }
            });
        }

        if (expenses.Count > 0)
        {
            column.Item().PaddingTop(8).Text("Lista troškova").SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.ConstantColumn(60);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Datum");
                    header.Cell().Element(CellHeader).Text("Naziv");
                    header.Cell().Element(CellHeader).Text("Kategorija");
                    header.Cell().Element(CellHeader).AlignRight().Text("Iznos");
                });

                foreach (var expense in expenses)
                {
                    table.Cell().Element(CellBody).Text(expense.ExpenseDate.ToString("dd.MM.yyyy"));
                    table.Cell().Element(CellBody).Text(expense.Name);
                    table.Cell().Element(CellBody).Text(FormatExpenseCategory(expense.Category));
                    table.Cell().Element(CellBody).AlignRight().Text($"{expense.Amount:N2} €");
                }
            });
        }
        else
        {
            column.Item().PaddingTop(4).Text("Nema unetih troškova.").Italic().FontColor(Colors.Grey.Medium);
        }
    }

    private static void ComposeDestinations(ColumnDescriptor column, IReadOnlyList<DestinationResponseDto> destinations)
    {
        column.Item().Element(c => SectionTitle(c, "Destinacije"));

        if (destinations.Count == 0)
        {
            column.Item().Text("Nema destinacija.").Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellHeader).Text("Naziv");
                header.Cell().Element(CellHeader).Text("Lokacija");
                header.Cell().Element(CellHeader).Text("Dolazak");
                header.Cell().Element(CellHeader).Text("Odlazak");
            });

            foreach (var destination in destinations)
            {
                table.Cell().Element(CellBody).Text(destination.Name);
                table.Cell().Element(CellBody).Text(destination.Location);
                table.Cell().Element(CellBody).Text(destination.ArrivalDate.ToString("dd.MM.yyyy"));
                table.Cell().Element(CellBody).Text(destination.DepartureDate.ToString("dd.MM.yyyy"));
            }
        });
    }

    private static void ComposeActivities(ColumnDescriptor column, IReadOnlyList<ActivityResponseDto> activities)
    {
        column.Item().Element(c => SectionTitle(c, "Aktivnosti"));

        if (activities.Count == 0)
        {
            column.Item().Text("Nema aktivnosti.").Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        var grouped = activities
            .GroupBy(a => a.ActivityDate)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            column.Item().PaddingTop(4).Text(group.Key.ToString("dd.MM.yyyy (dddd)"))
                .SemiBold().FontColor(Colors.Blue.Darken1);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(55);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Vreme");
                    header.Cell().Element(CellHeader).Text("Naziv");
                    header.Cell().Element(CellHeader).Text("Lokacija");
                    header.Cell().Element(CellHeader).Text("Status");
                    header.Cell().Element(CellHeader).AlignRight().Text("Trošak");
                });

                foreach (var activity in group.OrderBy(a => a.ActivityTime))
                {
                    table.Cell().Element(CellBody).Text(FormatTime(activity.ActivityTime));
                    table.Cell().Element(CellBody).Text(activity.Name);
                    table.Cell().Element(CellBody).Text(activity.Location ?? "—");
                    table.Cell().Element(CellBody).Text(activity.Status);
                    table.Cell().Element(CellBody).AlignRight().Text(
                        activity.EstimatedCost.HasValue ? $"{activity.EstimatedCost:N2} €" : "—");
                }
            });
        }
    }

    private static void ComposeChecklist(ColumnDescriptor column, IReadOnlyList<ChecklistItemResponseDto> items)
    {
        column.Item().Element(c => SectionTitle(c, "Lista za pakovanje"));

        if (items.Count == 0)
        {
            column.Item().Text("Lista za pakovanje je prazna.").Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        var defaultItems = items
            .Where(item => IsDefaultPackingItem(item.Title))
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        var customItems = items
            .Where(item => !IsDefaultPackingItem(item.Title))
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        column.Item().PaddingTop(4).Text("Osnovne stavke").SemiBold().FontSize(10);
        column.Item().Element(c => ComposeChecklistTable(c, defaultItems));

        if (customItems.Count > 0)
        {
            column.Item().PaddingTop(8).Text("Ostalo").SemiBold().FontSize(10);
            column.Item().Element(c => ComposeChecklistTable(c, customItems));
        }
    }

    private static void ComposeChecklistTable(IContainer container, IReadOnlyList<ChecklistItemResponseDto> items)
    {
        if (items.Count == 0)
        {
            container.Text("—").Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(36);
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(CellHeader).AlignCenter().Text("OK");
                header.Cell().Element(CellHeader).Text("Stavka");
            });

            foreach (var item in items)
            {
                table.Cell().Element(cell => ChecklistStatusCell(cell, item.IsCompleted));
                table.Cell().Element(cell => ChecklistTitleCell(cell, item));
            }
        });
    }

    private static void ChecklistStatusCell(IContainer container, bool isCompleted)
    {
        container
            .Border(1)
            .BorderColor(isCompleted ? Colors.Green.Darken2 : Colors.Grey.Medium)
            .Background(isCompleted ? Colors.Green.Lighten4 : Colors.White)
            .PaddingVertical(3)
            .AlignCenter()
            .AlignMiddle()
            .Text(isCompleted ? "X" : string.Empty)
            .FontSize(10)
            .SemiBold()
            .FontColor(Colors.Green.Darken3);
    }

    private static void ChecklistTitleCell(IContainer container, ChecklistItemResponseDto item)
    {
        var text = container.PaddingVertical(4).PaddingHorizontal(6);

        if (item.IsCompleted)
        {
            text.Text(item.Title).Strikethrough().FontColor(Colors.Grey.Darken1);
            return;
        }

        text.Text(item.Title);
    }

    private static bool IsDefaultPackingItem(string title) =>
        ChecklistDefaults.Titles.Any(defaultTitle =>
            defaultTitle.Equals(title, StringComparison.OrdinalIgnoreCase));

    private static void SectionTitle(IContainer container, string title)
    {
        container.PaddingBottom(4).Text(title).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
    }

    private static void StatBox(IContainer container, string label, string value)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
        {
            col.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().Text(value).SemiBold();
        });
    }

    private static void AddInfoRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(CellBody).Text(label).SemiBold();
        table.Cell().Element(CellBody).Text(value);
    }

    private static IContainer CellHeader(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(6);

    private static IContainer CellBody(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(6);

    private static string FormatExpenseCategory(string category) =>
        ExpenseCategoryLabels.TryGetValue(category, out var label) ? label : category;

    private static string FormatTime(TimeOnly? time) =>
        time.HasValue ? time.Value.ToString("HH:mm") : "—";
}
