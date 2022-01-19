using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace pdf_test
{
    class Program
    {
        private static List<string> dataResults = new List<string> { "Counter, File, DOI/Error" };

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var paths = Directory.GetFiles(@"C:\Code\DOI\DOI_Extraction_CS\DOI_Extraction_CS\DOI_Pdf\");

            Console.WriteLine(paths.Length);

            Thread.Sleep(5000);

            var counter = 1;

            foreach (var path in paths)
            {
                try
                {
                    var text = ExtractTextFromPDF(path, counter);

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        dataResults.Add($"{counter}, {path}, Error File is empty");
                        counter++;
                        continue;
                    }

                    SearchInTheText(path, text, counter);
                    counter++;
                }
                catch (Exception ex)
                {
                    counter++;
                    dataResults.Add($"{counter}, {path}, Exception on th eprocess loop");
                    Console.WriteLine(ex.Message);
                }
            }

            File.WriteAllText(@"C:\Code\DOI\DOI_Extraction_CS\DOI_Extraction_CS\pdf_data_and_errors.csv", string.Join(Environment.NewLine, dataResults));
        }

        private static void SearchInTheText(string path, string text, int counter)
        {
            var result = "Not Found";

            try
            {
                var lines = text.Split(
                    new string[] { "\n\r", "\n", "\r", ",", ";", ")", "(" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                var resultList = lines
                    .Where(line => line.IndexOf("DOI", StringComparison.InvariantCultureIgnoreCase) != -1)
                    .Select(line => line.Replace("https://doi.org/", "DOI "))
                    .Select(line => line.Replace("http://dx.doi.org/", "DOI "))
                    .Select(line => line.Trim())
                    .Where(line => line.Length > 4)
                    .Where(line => !line.Contains("doing", StringComparison.InvariantCultureIgnoreCase))
                    .Select(line => {
                        var index = line.IndexOf("DOI", StringComparison.InvariantCultureIgnoreCase);
                        if (index > 0)
                            return line.Substring(index, line.Length - index);
                        else
                            return line;
                    })
                    .Select(line => line.Replace("DOI: ", "", StringComparison.InvariantCultureIgnoreCase))
                    .Select(line => line.Replace("DOI:", "", StringComparison.InvariantCultureIgnoreCase))
                    .Select(line => line.Replace("DOI ", "", StringComparison.InvariantCultureIgnoreCase))
                    .Select(line => line.Replace("DOI.", "", StringComparison.InvariantCultureIgnoreCase))
                    .Distinct()
                    .ToList();

                result = string.Join(";", resultList);
            }
            catch (Exception)
            {
                result = "Error";
            }

            var message = $"{counter}, {path}, {result}";

            if (string.IsNullOrWhiteSpace(result))
            {
                message = $"{counter}, {path}, Error DIO text not found";
                dataResults.Add(message);
            }
            else
            {
                dataResults.Add(message);
            }

            Console.WriteLine(message);
        }

        public static string ExtractTextFromPDF(string filePath, int counter)
        {
            var result = "";

            try
            {

                PdfReader pdfReader = new PdfReader(filePath);
                PdfDocument pdfDoc = new PdfDocument(pdfReader);
                for (int page = 1; page <= Math.Min(3, pdfDoc.GetNumberOfPages()); page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    result += Environment.NewLine + PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                }
                pdfDoc.Close();
                pdfReader.Close();

            }
            catch (Exception)
            {
                var message = $"{counter}, {filePath}, File Reading Error";
                Console.WriteLine(message);
                dataResults.Add(message);
            }

            return result;
        }

    }
}

