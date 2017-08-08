namespace pdfHelper
{
  class Program
  {
      private const string TestFile = "programming_in_scala_2nd.pdf";
    static void Main()
    {
      var document = new PdfDocument(TestFile);
      document.GetPdfText();
    }
  }
}
