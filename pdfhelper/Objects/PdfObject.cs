using System;
using System.Collections.Generic;

namespace pdfHelper
{
  /// <summary>Объект Pdf документа</summary>
  public class PdfObject
  {
    #region Поля
    /// <summary>Имя объекта</summary>
    public string Name { get; private set; }

    /// <summary>Массив байт объекта</summary>
    private static byte[] _ObjectBytes;

    /// <summary>Разжатые данные объекта</summary>
    private static byte[] _UncompressedBytes;

    /// <summary>Тип объекта</summary>
    public static PdfObjectType Type;

    /// <summary>Список атрибутов объекта</summary>
    public static List<Tuple<string, string>> Attributes = new List<Tuple<string, string>>();

    /// <summary>True если объект сжат в поток, иначе false</summary>
    public static bool Compressed { get; private set; }

    /// <summary>Список текстовых блоков объекта</summary>
    public List<PdfTextObject> TextObjects = new List<PdfTextObject>();
    #endregion

    #region Конструкторы

    /// <summary>Создаёт новый экземпляр класса <see cref="PdfObject"/>.</summary>
    /// <param name="name">Имя объекта.</param>
    /// <param name="inBytes">Массив байт объекта.</param>
    /// <param name="type">Тип объекта.</param>
    public PdfObject(string name, byte[] inBytes, PdfObjectType type)
    {
      _ObjectBytes = inBytes;
      Name = name;
      Type = type;
      Compressed = false;
      ParseData();
    }
    #endregion

    #region Методы

    /// <summary>Разбирает данные входного массива</summary>
    private void ParseData()
    {
      //Получаем сжатие, если его нет, то и смысла дальше парсить нет
      var position = PdfFunctions.GetPosition(_ObjectBytes, 0, PdfConsts.PDF_FLATE_DECODE);
      var parseText = true;
      if( position != -1 )
      {
        //объект сжат в поток, разожмем
        Compressed = true;
        parseText=DecompressData();
      }
      if (parseText)
        FillTextData();
    }

    /// <summary>Распаковывает запакованные данные</summary>
    /// <returns>true если массив распакован, иначе false</returns>
    private static bool DecompressData()
    {
      var length = Convert.ToInt32(PdfFunctions.GetAttribute(_ObjectBytes, 0, PdfConsts.PDF_STREAM_LENGTH));
      var startIndex = PdfFunctions.GetPosition(_ObjectBytes, 0, PdfConsts.PDF_START_STREAM) + 1;
      if (startIndex == -1)
        return false;
      var endIndex = startIndex + length + 1;

      //Убираем #10#13 в начале и конце
      if (_ObjectBytes[startIndex] == 0x0d && _ObjectBytes[startIndex + 1] == 0x0a) startIndex += 2;
      else if (_ObjectBytes[startIndex] == 0x0a || _ObjectBytes[startIndex] == 0x0d) startIndex++;

      if (_ObjectBytes[endIndex - 1] == 0x0d && _ObjectBytes[endIndex] == 0x0a) endIndex -= 2;
      else if (_ObjectBytes[endIndex - 1] == 0x0a) endIndex--;
      else if (_ObjectBytes[endIndex] == 0x0d || _ObjectBytes[endIndex] == 0x0a)
        endIndex--;

      var streamBytes = new byte[length];
      var counter = 0;
      for (int i = startIndex; i <= endIndex; i++)
      {
        streamBytes[counter] = _ObjectBytes[i];
        counter++;
      }
      if( streamBytes[0] == 72 )
        _UncompressedBytes = PdfFunctions.Decompress(streamBytes);
      else
        return false;
      return true;
    }

    /// <summary>Парсит и заполняет список текстовых объектов внутри Pdf объекта</summary>
    private void FillTextData()
    {
      var inBytes = Compressed ? _UncompressedBytes : _ObjectBytes;
      var position = 0;
      byte[] data;
      do
      {
        data = Compressed ? PdfFunctions.GetEncodedTextBlock(inBytes, position, out position) : PdfFunctions.GetTextBlock(inBytes, position, out position);
        if (data != null)
          TextObjects.Add(new PdfTextObject(data));
      }
      while (data != null);
    }

    #endregion
  }
}
