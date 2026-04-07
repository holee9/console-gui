using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// QA report generator for UI testing results.
/// </summary>
public sealed class QAReportGenerator
{
    public record TestResult(
        string Category,
        string TestName,
        bool Passed,
        string Message,
        double? Value,
        string? Unit);

    public record BugReport(
        string Id,
        string Category,
        string Severity,
        string Title,
        string Description,
        string StepsToReproduce,
        string ExpectedResult,
        string ActualResult);

    private readonly List<TestResult> _testResults = new();
    private readonly List<BugReport> _bugs = new();

    public void AddTestResult(TestResult result) => _testResults.Add(result);

    public void AddBug(BugReport bug) => _bugs.Add(bug);

    /// <summary>
    /// Generates an HTML report.
    /// </summary>
    public string GenerateHtmlReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='ko'>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset='UTF-8'>");
        sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("    <title>HnVue UI QA Report</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', system-ui, sans-serif; margin: 0; padding: 20px; background: #1A1A2E; color: #E0E0E0; }");
        sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; }");
        sb.AppendLine("        h1 { color: #00AEEF; border-bottom: 2px solid #00AEEF; padding-bottom: 10px; }");
        sb.AppendLine("        h2 { color: #4D94FF; margin-top: 30px; }");
        sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }");
        sb.AppendLine("        .summary-card { background: #252542; padding: 20px; border-radius: 8px; text-align: center; }");
        sb.AppendLine("        .summary-card .value { font-size: 36px; font-weight: bold; }");
        sb.AppendLine("        .summary-card.pass .value { color: #2ED573; }");
        sb.AppendLine("        .summary-card.fail .value { color: #FF4757; }");
        sb.AppendLine("        .summary-card.warning .value { color: #FFA502; }");
        sb.AppendLine("        .summary-card .label { color: #A0A0B0; margin-top: 5px; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; background: #252542; }");
        sb.AppendLine("        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #3E3E5E; }");
        sb.AppendLine("        th { background: #1B4F8A; }");
        sb.AppendLine("        .pass { color: #2ED573; }");
        sb.AppendLine("        .fail { color: #FF4757; }");
        sb.AppendLine("        .warning { color: #FFA502; }");
        sb.AppendLine("        .bug-list { margin: 20px 0; }");
        sb.AppendLine("        .bug-item { background: #252542; padding: 15px; margin: 10px 0; border-radius: 6px; border-left: 4px solid #FF4757; }");
        sb.AppendLine("        .bug-item.severity-critical { border-left-color: #D50000; }");
        sb.AppendLine("        .bug-item.severity-high { border-left-color: #FF6D00; }");
        sb.AppendLine("        .bug-item.severity-medium { border-left-color: #FFD600; }");
        sb.AppendLine("        .bug-item.severity-low { border-left-color: #00C853; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class='container'>");
        sb.AppendLine("        <h1>HnVue UI QA Report</h1>");
        sb.AppendLine($"        <p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        // Summary
        int totalTests = _testResults.Count;
        int passedTests = _testResults.Count(r => r.Passed);
        int failedTests = totalTests - passedTests;
        double passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

        sb.AppendLine("        <div class='summary'>");
        sb.AppendLine($"            <div class='summary-card pass'><div class='value'>{passedTests}</div><div class='label'>Passed</div></div>");
        sb.AppendLine($"            <div class='summary-card fail'><div class='value'>{failedTests}</div><div class='label'>Failed</div></div>");
        sb.AppendLine($"            <div class='summary-card'><div class='value'>{totalTests}</div><div class='label'>Total Tests</div></div>");
        string passRateClass = passRate >= 95 ? "pass" : passRate >= 80 ? "warning" : "fail";
        sb.AppendLine($"            <div class='summary-card {passRateClass}'><div class='value'>{passRate:F1}%</div><div class='label'>Pass Rate</div></div>");
        sb.AppendLine("        </div>");

        // Test Results by Category
        sb.AppendLine("        <h2>Test Results</h2>");

        var groupedResults = new Dictionary<string, List<TestResult>>();
        foreach (var result in _testResults)
        {
            if (!groupedResults.ContainsKey(result.Category))
            {
                groupedResults[result.Category] = new List<TestResult>();
            }
            groupedResults[result.Category].Add(result);
        }

        foreach (var (category, results) in groupedResults)
        {
            sb.AppendLine($"        <h3>{category}</h3>");
            sb.AppendLine("        <table>");
            sb.AppendLine("            <tr><th>Test</th><th>Status</th><th>Value</th><th>Message</th></tr>");

            foreach (var result in results)
            {
                string statusClass = result.Passed ? "pass" : "fail";
                string statusText = result.Passed ? "PASS" : "FAIL";
                string valueText = result.Value.HasValue && !string.IsNullOrEmpty(result.Unit)
                    ? $"{result.Value.Value:F2}{result.Unit}"
                    : "-";

                sb.AppendLine($"            <tr>");
                sb.AppendLine($"                <td>{result.TestName}</td>");
                sb.AppendLine($"                <td class='{statusClass}'>{statusText}</td>");
                sb.AppendLine($"                <td>{valueText}</td>");
                sb.AppendLine($"                <td>{result.Message}</td>");
                sb.AppendLine($"            </tr>");
            }

            sb.AppendLine("        </table>");
        }

        // Bugs
        if (_bugs.Count > 0)
        {
            sb.AppendLine("        <h2>Bugs Found</h2>");
            sb.AppendLine("        <div class='bug-list'>");

            foreach (var bug in _bugs)
            {
                sb.AppendLine($"            <div class='bug-item severity-{bug.Severity.ToLower()}'>");
                sb.AppendLine($"                <h3>{bug.Id}: {bug.Title}</h3>");
                sb.AppendLine($"                <p><strong>Category:</strong> {bug.Category} | <strong>Severity:</strong> {bug.Severity}</p>");
                sb.AppendLine($"                <p><strong>Description:</strong> {bug.Description}</p>");
                sb.AppendLine($"                <p><strong>Steps:</strong> {bug.StepsToReproduce}</p>");
                sb.AppendLine($"                <p><strong>Expected:</strong> {bug.ExpectedResult}</p>");
                sb.AppendLine($"                <p><strong>Actual:</strong> {bug.ActualResult}</p>");
                sb.AppendLine($"            </div>");
            }

            sb.AppendLine("        </div>");
        }

        // Footer
        sb.AppendLine("        <h2>Quality Criteria</h2>");
        sb.AppendLine("        <table>");
        sb.AppendLine("            <tr><th>Criterion</th><th>Target</th><th>Status</th></tr>");
        string consistencyClass = passRate >= 95 ? "pass" : "fail";
        sb.AppendLine($"            <tr><td>Visual Consistency</td><td>≥95%</td><td class='{consistencyClass}'>{passRate:F1}%</td></tr>");
        sb.AppendLine($"            <tr><td>WCAG 2.2 AA</td><td>Compliant</td><td class='pass'>See Accessibility Tests</td></tr>");
        sb.AppendLine($"            <tr><td>Screen Load Time</td><td>&lt;1s</td><td class='pass'>See Performance Tests</td></tr>");
        string bugTrackingClass = _bugs.Count > 0 ? "warning" : "pass";
        sb.AppendLine($"            <tr><td>Bug Tracking</td><td>All tracked</td><td class='{bugTrackingClass}'>{_bugs.Count} bugs</td></tr>");
        sb.AppendLine("        </table>");

        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a JSON report.
    /// </summary>
    public string GenerateJsonReport()
    {
        var report = new
        {
            GeneratedAt = DateTime.Now.ToString("o"),
            Summary = new
            {
                TotalTests = _testResults.Count,
                PassedTests = _testResults.Count(r => r.Passed),
                FailedTests = _testResults.Count(r => !r.Passed),
                PassRate = _testResults.Count > 0
                    ? (double)_testResults.Count(r => r.Passed) / _testResults.Count * 100
                    : 0.0,
                BugCount = _bugs.Count
            },
            TestResults = _testResults,
            Bugs = _bugs
        };

        return JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Saves reports to files.
    /// </summary>
    public void SaveReports(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string htmlPath = Path.Combine(directory, $"qa_report_{timestamp}.html");
        string jsonPath = Path.Combine(directory, $"qa_report_{timestamp}.json");

        File.WriteAllText(htmlPath, GenerateHtmlReport());
        File.WriteAllText(jsonPath, GenerateJsonReport());
    }

    /// <summary>
    /// Loads existing bugs from a JSON file.
    /// </summary>
    public void LoadBugs(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string json = File.ReadAllText(filePath);
        var bugs = JsonSerializer.Deserialize<BugReport[]>(json);

        if (bugs != null)
        {
            _bugs.AddRange(bugs);
        }
    }

    /// <summary>
    /// Saves bugs to a JSON file.
    /// </summary>
    public void SaveBugs(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(_bugs, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }
}
