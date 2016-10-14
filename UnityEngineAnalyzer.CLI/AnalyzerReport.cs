﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityEngineAnalyzer.CLI
{
    public class AnalyzerReport
    {
        public string ProjectName { get; set; }

        //TODO: Add support for Solutions with multiple Projects
        private readonly List<DiagnosticInfo> _diagnostics = new List<DiagnosticInfo>();

        private readonly List<IAnalyzerExporter> _exporters = new List<IAnalyzerExporter>();

        public void AddExporter(IAnalyzerExporter exporter)
        {
            _exporters.Add(exporter);
        }

        public void AppendDiagnostics(IEnumerable<Diagnostic> diagnosticResults)
        {
            if (_exporters.Count == 0)
            {
                return;
            }

            foreach (var diagnostic in diagnosticResults)
            {
                var locationSpan = diagnostic.Location.SourceSpan;
                var lineSpan = diagnostic.Location.SourceTree.GetLineSpan(locationSpan);

                var diagnosticInfo = new DiagnosticInfo
                {
                    Id = diagnostic.Id,
                    Message = diagnostic.GetMessage(),
                    FileName = diagnostic.Location.SourceTree.FilePath,
                    LineNumber = lineSpan.StartLinePosition.Line
                };

                foreach (var exporter in _exporters)
                {
                    exporter.AppendDiagnostic(diagnosticInfo);
                }
            }
        }

        public void FinalizeReport()
        {
            foreach (var exporter in _exporters)
            {
                exporter.Finish();
            }
        }
    }

    public interface IAnalyzerExporter
    {
        void AppendDiagnostic(DiagnosticInfo diagnosticInfo);
        void Finish();
    }

    public class DiagnosticInfo
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
    }

    public class JsonAnalyzerExporter : IAnalyzerExporter
    {
        private readonly List<DiagnosticInfo> _diagnostics = new List<DiagnosticInfo>();

        public void AppendDiagnostic(DiagnosticInfo diagnosticInfo)
        {
            _diagnostics.Add(diagnosticInfo);
        }

        public void Finish()
        {
            Console.WriteLine("This is where we write the json file : " + _diagnostics.Count);
        }
    }

    public class ConsoleAnalyzerExporter : IAnalyzerExporter
    {
        private const string ConsoleSeparator = "\t";

        public void AppendDiagnostic(DiagnosticInfo diagnosticInfo)
        {
            Console.Write(diagnosticInfo.Id);
            Console.Write(ConsoleSeparator);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(diagnosticInfo.Message);
            Console.ResetColor();
            Console.Write(ConsoleSeparator);
            Console.WriteLine("{0}({1})",diagnosticInfo.FileName,diagnosticInfo.LineNumber);
        }

        public void Finish()
        {
            Console.WriteLine("Console Export Finished");
        }
    }


}
