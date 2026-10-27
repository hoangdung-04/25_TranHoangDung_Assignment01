using System;
using System.IO;
using System.Linq;
using BusinessObjects.DTOs;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace _25_TranHoangDung_Assignment01_FrontEnd.Services
{
    public class WordExportService
    {
        public byte[] GenerateReportWord(ReportStatisticsDto stats, DateTime? startDate, DateTime? endDate, string adminName)
        {
            using var memStream = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();

                // 1. Document Title
                body.AppendChild(CreateParagraph("FPT UNIVERSITY - FUNewsManagement System", "Calibri", 32, true, "1E3A8A", JustificationValues.Center, 0, 100));
                body.AppendChild(CreateParagraph("ARTICLES AUDIT & STATISTICS REPORT", "Calibri", 24, true, "2563EB", JustificationValues.Center, 0, 240));

                // 2. Metadata Section (Box / Details)
                var periodText = (startDate.HasValue || endDate.HasValue)
                    ? $"From: {(startDate.HasValue ? startDate.Value.ToString("dd/MM/yyyy") : "Earliest")}  To: {(endDate.HasValue ? endDate.Value.ToString("dd/MM/yyyy") : "Latest")}"
                    : "Period: All Recorded Periods";

                body.AppendChild(CreateParagraph(periodText, "Calibri", 20, false, "4B5563", JustificationValues.Center, 0, 80));
                body.AppendChild(CreateParagraph($"Generated On: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Exported By: {adminName}", "Calibri", 18, false, "6B7280", JustificationValues.Center, 0, 300));

                // 3. Section 1: Executive KPI Summary
                body.AppendChild(CreateParagraph("1. Executive Summary & KPIs", "Calibri", 22, true, "1E3A8A", JustificationValues.Left, 200, 100));

                var kpiTable = CreateTable();
                var kpiHeaderRow = CreateTableRow(new[] { "Metric", "Value", "Status / Notes" }, true, "2563EB", "FFFFFF");
                kpiTable.AppendChild(kpiHeaderRow);

                kpiTable.AppendChild(CreateTableRow(new[] { "Total Articles", stats.TotalArticles.ToString(), "Total articles counted within selected period" }, false, null, "111827"));
                kpiTable.AppendChild(CreateTableRow(new[] { "Active (Published)", stats.ActiveArticles.ToString(), "Visible to public readers" }, false, "ECFDF5", "065F46"));
                kpiTable.AppendChild(CreateTableRow(new[] { "Inactive (Drafts)", stats.InactiveArticles.ToString(), "Drafts or pending review" }, false, "FFFBEB", "92400E"));
                kpiTable.AppendChild(CreateTableRow(new[] { "Categories Represented", (stats.ArticlesByCategory?.Count ?? 0).ToString(), "Active distinct categories" }, false, null, "111827"));

                body.AppendChild(kpiTable);
                body.AppendChild(CreateParagraph("", "Calibri", 12, false, "000000", JustificationValues.Left, 0, 200));

                // 4. Section 2: Distribution by Category
                if (stats.ArticlesByCategory != null && stats.ArticlesByCategory.Any())
                {
                    body.AppendChild(CreateParagraph("2. Distribution by Category", "Calibri", 22, true, "1E3A8A", JustificationValues.Left, 200, 100));
                    var catTable = CreateTable();
                    catTable.AppendChild(CreateTableRow(new[] { "#", "Category Name", "Articles Count", "Percentage" }, true, "3B82F6", "FFFFFF"));

                    int catIdx = 1;
                    int total = stats.TotalArticles > 0 ? stats.TotalArticles : 1;
                    foreach (var cat in stats.ArticlesByCategory)
                    {
                        double pct = ((double)cat.Value / total) * 100.0;
                        catTable.AppendChild(CreateTableRow(new[]
                        {
                            catIdx++.ToString(),
                            cat.Key,
                            cat.Value.ToString(),
                            $"{pct:F1}%"
                        }, false, (catIdx % 2 == 0) ? "F9FAFB" : null, "111827"));
                    }
                    body.AppendChild(catTable);
                    body.AppendChild(CreateParagraph("", "Calibri", 12, false, "000000", JustificationValues.Left, 0, 200));
                }

                // 5. Section 3: Detailed News Articles Table
                body.AppendChild(CreateParagraph("3. Detailed News Articles (Sorted Descending by Created Date)", "Calibri", 22, true, "1E3A8A", JustificationValues.Left, 200, 100));

                if (stats.Articles == null || !stats.Articles.Any())
                {
                    body.AppendChild(CreateParagraph("No news articles found for the specified period.", "Calibri", 20, false, "6B7280", JustificationValues.Left, 100, 100));
                }
                else
                {
                    var artTable = CreateTable();
                    artTable.AppendChild(CreateTableRow(new[] { "#", "ID", "Created Date", "Headline", "Category", "Author", "Status", "Tags" }, true, "1E3A8A", "FFFFFF"));

                    int artIdx = 1;
                    foreach (var a in stats.Articles)
                    {
                        var statusStr = a.NewsStatus == true ? "Active" : "Inactive";
                        var tagsStr = (a.TagNames != null && a.TagNames.Any()) ? string.Join(", ", a.TagNames) : "-";
                        var bgFill = (artIdx % 2 == 0) ? "F8FAFC" : null;

                        artTable.AppendChild(CreateTableRow(new[]
                        {
                            artIdx++.ToString(),
                            a.NewsArticleID ?? "",
                            a.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                            a.Headline ?? "",
                            a.CategoryName ?? "General",
                            a.CreatedByName ?? "Staff",
                            statusStr,
                            tagsStr
                        }, false, bgFill, "111827"));
                    }
                    body.AppendChild(artTable);
                }

                // Footer Note
                body.AppendChild(CreateParagraph("", "Calibri", 12, false, "000000", JustificationValues.Left, 0, 200));
                body.AppendChild(CreateParagraph("--- End of Report | Generated by FUNewsManagement System ---", "Calibri", 16, false, "9CA3AF", JustificationValues.Center, 100, 100));

                mainPart.Document.AppendChild(body);
                mainPart.Document.Save();
            }

            return memStream.ToArray();
        }

        private static Paragraph CreateParagraph(string text, string fontName, int fontSizeHalfPt, bool isBold, string hexColor, JustificationValues align, int spaceBefore = 0, int spaceAfter = 0)
        {
            var p = new Paragraph();
            var pProp = new ParagraphProperties(
                new Justification { Val = align },
                new SpacingBetweenLines { Before = spaceBefore.ToString(), After = spaceAfter.ToString() }
            );
            p.AppendChild(pProp);

            var run = new Run();
            var rProp = new RunProperties(
                new RunFonts { Ascii = fontName, HighAnsi = fontName },
                new FontSize { Val = fontSizeHalfPt.ToString() },
                new Color { Val = hexColor }
            );
            if (isBold) rProp.AppendChild(new Bold());
            run.AppendChild(rProp);
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            p.AppendChild(run);
            return p;
        }

        private static Table CreateTable()
        {
            var table = new Table();
            var tblPr = new TableProperties(
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4, Color = "D1D5DB" },
                    new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "D1D5DB" },
                    new LeftBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "E5E7EB" },
                    new InsideVerticalBorder { Val = BorderValues.None }
                ),
                new TableCellMarginDefault(
                    new TopMargin { Width = "120" },
                    new BottomMargin { Width = "120" },
                    new TableCellLeftMargin { Width = 160 },
                    new TableCellRightMargin { Width = 160 }
                )
            );
            table.AppendChild(tblPr);
            return table;
        }

        private static TableRow CreateTableRow(string[] cells, bool isHeader, string? bgHexColor, string textHexColor)
        {
            var row = new TableRow();
            if (isHeader)
            {
                row.AppendChild(new TableRowProperties(new TableHeader()));
            }

            foreach (var cellText in cells)
            {
                var tc = new TableCell();
                var tcPr = new TableCellProperties();
                if (!string.IsNullOrEmpty(bgHexColor))
                {
                    tcPr.AppendChild(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = bgHexColor });
                }
                tc.AppendChild(tcPr);

                var p = new Paragraph();
                var pProp = new ParagraphProperties(
                    new SpacingBetweenLines { Before = "60", After = "60" }
                );
                p.AppendChild(pProp);

                var run = new Run();
                var rProp = new RunProperties(
                    new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" },
                    new FontSize { Val = isHeader ? "20" : "18" },
                    new Color { Val = textHexColor }
                );
                if (isHeader) rProp.AppendChild(new Bold());
                run.AppendChild(rProp);
                run.AppendChild(new Text(cellText) { Space = SpaceProcessingModeValues.Preserve });

                p.AppendChild(run);
                tc.AppendChild(p);
                row.AppendChild(tc);
            }

            return row;
        }
    }
}
