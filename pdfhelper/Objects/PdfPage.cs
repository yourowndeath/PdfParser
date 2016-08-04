using System;
using System.Globalization;

namespace pdfHelper
{
  /// <summary>Страница Pdf-документа</summary>
  public class PdfPage
  {
    #region Поля

    /// <summary>Имя объекта</summary>
    public static string Name { get; private set; }

    /// <summary>Массив байт объекта страницы</summary>
    private static byte[] _ObjectBytes;

    /// <summary>ArtBox страницы</summary>
    private static readonly decimal[] _ArtBox = new decimal[4];
   
    /// <summary>BleedBox страницы</summary>
    private static readonly decimal[] _BleedBox = new decimal[4];
    
    /// <summary>MediaBox страницы</summary>
    private static readonly decimal[] _MediaBox = new decimal[4];

    /// <summary>TrimBox страницы</summary>
    private static readonly decimal[] _TrimBox = new decimal[4];
    #endregion

    #region Конструкторы

    /// <summary>Создаёт новый экземпляр класса <see cref="PdfPage"/>.</summary>
    /// <param name="name">Имя объекта формата [1 0 obj].</param>
    /// <param name="pageBytes">Массив байт объекта.</param>
    public PdfPage(string name, byte[] pageBytes)
    {
      Name = name;
      _ObjectBytes = pageBytes;
      FillBox(_ArtBox, PdfConsts.PDF_ART_BOX);
      FillBox(_BleedBox, PdfConsts.PDF_BLEED_BOX);
      FillBox(_MediaBox, PdfConsts.PDF_MEDIA_BOX);
      FillBox(_TrimBox, PdfConsts.PDF_TRIM_BOX);
    }
    #endregion

    #region Методы
    /// <summary>Заполняет координатами переданный блок</summary>
    /// <param name="box">Массив координат.</param>
    /// <param name="boxName">Имя блока в PDF.</param>
    private static void FillBox(decimal[] box, string boxName)
    {
      var pos = PdfFunctions.GetPosition(_ObjectBytes, 0, boxName);
      if (pos == -1)
        return;
      pos += 2;
      var res = (char)_ObjectBytes[pos];
      var value = "";
      var position = 0;

      while (res != PdfConsts.PDF_CLOSE_QUAD_BRACKET)
      {
        value += res;
        pos++;
        res = (char)_ObjectBytes[pos];
        if (res == PdfConsts.PDF_SPACE || res == PdfConsts.PDF_CLOSE_QUAD_BRACKET)
        {
          box[position] = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
          position++;
          value = "";
        }
      }
    }
    #endregion
  }
}
