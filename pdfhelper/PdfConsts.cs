namespace pdfHelper
{

  /// <summary>Вспомогательный класс для хранения констант</summary>
  public static class PdfConsts
  {
    #region Сообщения
    /// <summary>Ошибка при пустом или не найденном входном файле</summary>
    public const string MSG_FILE_NOT_FOUND = "Файл {0} не найден";

    /// <summary>Ошибка если файл не PDF</summary>
    public const string MSG_FILE_WRONG_FORMAT = "Файл {0} имеет неверный формат";

    public const string MSG_EMPTY_ARRAY = "Входной массив данных пуст";
    #endregion

    #region Разное
    /// <summary>Расширение PDF</summary>
    public const string PDF_EXTENSION = ".pdf";

    /// <summary>Регулярное выражение для извлечения строки из скобок</summary>
    public const string LINE_REGEX = @"\((.+)\)";

    /// <summary>Регулярное выражение для извлечения одной строки из нескольких скобок</summary>
    public const string MULTY_LINE_REGEX = @"\([^\)]+\)";
    #endregion

    #region Pdf
    /// <summary>Описание pdf документа</summary>
    public const string PDF_TRAILER = "trailer";

    /// <summary>Тип объекта</summary>
    public const string PDF_TYPE = "/Type";

    /// <summary>Количество объектов в pdf документе</summary>
    public const string PDF_SIZE = "/Size";

    /// <summary>Начало объекта</summary>
    public const string PDF_OBJECT = "obj";

    /// <summary>Конец объекта</summary>
    public const string PDF_END_OBJECT = "endobj";

    /// <summary>BleedBox страницы</summary>
    public const string PDF_BLEED_BOX = "/BleedBox";
    
    /// <summary>MediaBox страницы</summary>
    public const string PDF_MEDIA_BOX = "/MediaBox";

    /// <summary>TrimBox страницы</summary>
    public const string PDF_TRIM_BOX = "/TrimBox";

    /// <summary>ArtBox страницы</summary>
    public const string PDF_ART_BOX = "/ArtBox";

    /// <summary>Начало текстового блока</summary>
    public const string PDF_BEGIN_TEXT_BLOCK = "BT";

    /// <summary>Тег шрифта в текстовом блоке</summary>
    public const string PDF_TEXT_FONT = "Tf";

    /// <summary>Положение текста в документе</summary>
    public const string PDF_TEXT_POSITION = "Tm";

    /// <summary>Моноширинный простой текст</summary>
    public const string PDF_TEXT_PLAIN = "Tj";

    /// <summary>Текст с разным расстоянием между буквами</summary>
    public const string PDF_TEXT_ASSEMBLY = "TJ";

    /// <summary>Конец текстового блока</summary>
    public const string PDF_END_TEXT_BLOCK = "ET";

    /// <summary>Сжатый блок</summary>
    public const string PDF_FLATE_DECODE = "Filter/FlateDecode";

    /// <summary>Длина сжатого потока</summary>
    public const string PDF_STREAM_LENGTH = "/Length";

    /// <summary>Начало сжатого блока</summary>
    public const string PDF_START_STREAM = "stream";

    /// <summary>Конец сжатого блока</summary>
    public const string PDF_END_STREAM = "endstream";

    /// <summary>Модификатор для версии документа</summary>
    public const string PDF_VERSION = "%PDF-";
    #endregion

    #region Служебные символы
    /// <summary>Пробел</summary>
    public const int PDF_SPACE = 32;

    /// <summary>Процент %</summary>
    public const int PDF_PERSENT = 37;

    /// <summary>Открывающаяся круглая скобка (</summary>
    public const int PDF_OPEN_BRACKET = 40;

    /// <summary>Закрывающаяся круглая скобка )</summary>
    public const int PDF_CLOSE_BRACKET = 41;

    /// <summary>Обратный слэш /</summary>
    public const int PDF_BACKSLASH = 47;

    /// <summary>Открывающаяся треугольная скобка </summary>
    public const int PDF_OPEN_TRIANGLE_BRACKET = 60;
    /// <summary>Закрывающаяся треугольная скобка </summary>
    public const int PDF_CLOSE_TRIANGLE_BRACKET = 62;

    /// <summary>Открывающаяся квадратная скобка</summary>
    public const int PDF_OPEN_QUAD_BRACKET = 91;

    /// <summary>Закрывающаяся квадратная скобка</summary>
    public const int PDF_CLOSE_QUAD_BRACKET = 93;

    /// <summary>Открывающаяся фигурная скобка</summary>
    public const int PDF_OPEN_FIGURE_BRACKET = 123;

    /// <summary>Закрывающаяся фигурная скобка</summary>
    public const int PDF_CLOSE_FIGURE_BRACKET = 125;
    #endregion
  }
}
