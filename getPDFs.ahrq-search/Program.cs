using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace search.ahrq.gov
{
    internal class Program
    {
        public static string SourceUrl = "https://www.hcup-us.ahrq.gov";
        public static string SourcePath = "/reports/statbriefs";
        public static string SourceFile = "/statbriefs.jsp";
        public static string DestinationUrl = @"c:\temp";
        public static WebClient Client;

        public static async Task Main()
        {
            // htmlString = "<h1>Some example source</h1><p>This is a paragraph element";
            Client = new WebClient();
            var htmlString = Client.DownloadString(SourceUrl + SourcePath + SourceFile);

            var document = await ParseFile(htmlString);
            var pdfNodes = GetPDFNodes(document);
            OutputPDFFiles(pdfNodes);
        }

        static async Task<IDocument> ParseFile(string source)
        {
            //Use the default configuration for AngleSharp
            var config = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);

            //Create a virtual request to specify the document to load (here from our fixed string)
            return await context.OpenAsync(req => req.Content(source));
        }

        static IEnumerable<IHtmlAnchorElement> GetPDFNodes(IDocument document)
        {
            // Do something with document like the following
            // Console.WriteLine("Serializing the (original) document:");
            // Console.WriteLine(document.DocumentElement.OuterHtml);

            var nodes = document.QuerySelectorAll(@"a[href$="".pdf""]");

            Console.WriteLine($"Count: {nodes.Count()}");
            return nodes.OfType<IHtmlAnchorElement>().Where(s => s.Href.Contains(".pdf"));
        }

        static void OutputPDFFiles(IEnumerable<IHtmlAnchorElement> pdfNodes)
        {
            var i = 0;
            string directoryName, sourceFilePath, destinationFilePath;

            foreach (var pathName in pdfNodes.Select(n => n.PathName))
            {
                i++;
                try
                {
                    directoryName = Path.GetDirectoryName(pathName);

                    if (directoryName == "\\")
                        sourceFilePath = SourceUrl + SourcePath + pathName;
                    else
                        sourceFilePath = SourceUrl + pathName;

                    destinationFilePath = DestinationUrl + "\\" + Path.GetFileName(pathName);
                    Console.WriteLine($"[{i}] Source: {sourceFilePath}");
                    Console.WriteLine($"[{i}] Destination: {destinationFilePath}");

                    Client.DownloadFile(sourceFilePath, destinationFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{i}] Exception: {ex.Message}");
                }
            }
        }
    }
}
