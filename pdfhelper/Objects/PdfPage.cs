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
    private static byte[] _objectBytes;

    /// <summary>ArtBox страницы</summary>
    private static readonly decimal[] ArtBox = new decimal[4];
   
    /// <summary>BleedBox страницы</summary>
    private static readonly decimal[] BleedBox = new decimal[4];
    
    /// <summary>MediaBox страницы</summary>
    private static readonly decimal[] MediaBox = new decimal[4];

    /// <summary>TrimBox страницы</summary>
    private static readonly decimal[] TrimBox = new decimal[4];
    #endregion

    #region Конструкторы

    /// <summary>Создаёт новый экземпляр класса <see cref="PdfPage"/>.</summary>
    /// <param name="name">Имя объекта формата [1 0 obj].</param>
    /// <param name="pageBytes">Массив байт объекта.</param>
    public PdfPage(string name, byte[] pageBytes)
    {
      Name = name;
      _objectBytes = pageBytes;
      FillBox(ArtBox, PdfConsts.PDF_ART_BOX);
      FillBox(BleedBox, PdfConsts.PDF_BLEED_BOX);
      FillBox(MediaBox, PdfConsts.PDF_MEDIA_BOX);
      FillBox(TrimBox, PdfConsts.PDF_TRIM_BOX);
    }
    #endregion

    #region Методы
    /// <summary>Заполняет координатами переданный блок</summary>
    /// <param name="box">Массив координат.</param>
    /// <param name="boxName">Имя блока в PDF.</param>
    private static void FillBox(decimal[] box, string boxName)
    {
      var pos = PdfFunctions.GetPosition(_objectBytes, 0, boxName);
      if (pos == -1)
        return;

      pos += 2;
      var res = (char)_objectBytes[pos];
      var value = "";
      var position = 0;

      while (res != PdfConsts.PDF_CLOSE_QUAD_BRACKET)
      {
        value += res;
        pos++;
        res = (char)_objectBytes[pos];
          if (res != PdfConsts.PDF_SPACE && res != PdfConsts.PDF_CLOSE_QUAD_BRACKET) 
              continue;

          box[position] = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
          position++;
          value = "";
      }
    }
    #endregion
  }
}
