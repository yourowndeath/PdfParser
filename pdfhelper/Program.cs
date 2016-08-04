namespace pdfHelper
{
  class Program
  {
    private const string TEST_FILE = "test5.pdf";
    static void Main()
    {
      var document = new PdfDocument(TEST_FILE);
      document.GetPdfText();
    }
  }
}
