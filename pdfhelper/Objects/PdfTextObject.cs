using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace pdfHelper
{

  /// <summary>Текстовый объект pdf документа</summary>
   public class PdfTextObject
   {
     #region Поля
     /// <summary>Массив данных объекта</summary>
     private static byte[] _ObjectBytes;

     /// <summary>Имя шрифта в формате pdf</summary>
     public static string FontName { get; private set; }

     /// <summary>Размер шрифта</summary>
     public static int FontSize { get; private set; }

     /// <summary>Текстовые строки из pdf</summary>
     public List<string> TextLines = new List<string>();

     #endregion

     #region Конструкторы
     /// <summary>Создаёт новый экземпляр класса <see cref="PdfTextObject"/>.</summary>
     /// <param name="inBytes">Массив байт текстового объекта.</param>
     public PdfTextObject(byte[] inBytes)
     {
       _ObjectBytes = inBytes;
       var str = PdfFunctions.ConvertByteToString(_ObjectBytes);
       Parse();
     }
     #endregion

     #region Методы
     /// <summary>Парсит содержимое массива текстового объекта</summary>
     private void Parse()
     {
       int position;
       var font = PdfFunctions.GetTextAttribute(_ObjectBytes, 0, PdfConsts.PDF_TEXT_FONT, out position);
       if (!String.IsNullOrEmpty(font))
         FillTextParameters(font);
       string text;
       position = 0;
       do
       {
         text = PdfFunctions.GetTextAttribute(_ObjectBytes, position, PdfConsts.PDF_TEXT_PLAIN, out position);
         if (!String.IsNullOrEmpty(text))
           AddPlainText(text);
       }
       while (!String.IsNullOrEmpty(text));
       position = 0;
       do
       {
         text = PdfFunctions.GetTextAttribute(_ObjectBytes, position, PdfConsts.PDF_TEXT_ASSEMBLY, out position);
         if (!String.IsNullOrEmpty(text))
           AddAssemblyText(text);
       }
       while (!String.IsNullOrEmpty(text));

     }

     /// <summary>Добавляет сложный текст</summary>
     private void AddAssemblyText(string value)
     {
       var textValues = Regex.Matches(value, PdfConsts.MULTY_LINE_REGEX);
       var outValue = textValues.Cast<Match>().Aggregate("", ( current, textValue ) => current + Regex.Replace(textValue.Value, PdfConsts.LINE_REGEX, "$1"));
       if (!String.IsNullOrEmpty(outValue))
         TextLines.Add(outValue);
     }

     /// <summary>Добавляет моноширинный текст</summary>
     private void AddPlainText(string value)
     {
       var startPos = value.IndexOf("(", StringComparison.Ordinal);
       if (startPos == -1)
         AddHexaDecimalPlainText(value);
       else
         AddLiteralPlainText(value,startPos);
     }

    private void AddHexaDecimalPlainText(string value)
    {
      var startPos = value.IndexOf("<", StringComparison.Ordinal);
      if( startPos == -1 )
        return;
      var endPos = value.IndexOf(">", StringComparison.Ordinal);
      var res = endPos == -1 ? value.Substring(startPos) : value.Substring(startPos + 1, endPos - startPos - 1);
      var i = 0;
      var current = "";
      var outValue = "";
      while( i < res.Length )
      {
        current = res.Substring(i, 2);
        outValue += (char)Int32.Parse(current, System.Globalization.NumberStyles.HexNumber);
        i += 2;
      }
      outValue=PdfFunctions.AnsiToUnicode(outValue);
    }

    private void AddLiteralPlainText( string value,int startPos)
    {
      var endPos = value.IndexOf(")", StringComparison.Ordinal);
      var res = endPos == -1 ? value.Substring(startPos) : value.Substring(startPos + 1, endPos - startPos - 1);
      TextLines.Add(res);
    }

    /// <summary>Заполняет имя и размер шрифта</summary>
     private static void FillTextParameters(string value)
     {
       if (String.IsNullOrEmpty(value))
         return;
       var startPos = value.IndexOf("/", StringComparison.Ordinal);
       var endPos = value.IndexOf(" ", StringComparison.Ordinal);
       FontName = value.Substring(startPos + 1, endPos - startPos);
       FontSize = Convert.ToInt32(value.Substring(endPos + 1));
     }
     #endregion
   }
}
