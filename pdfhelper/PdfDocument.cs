using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace pdfHelper
{
    /// <summary>Pdf-документ</summary>
    public class PdfDocument
    {
        #region Поля

        /// <summary>Массив байт документа</summary>
        private static byte[] _documentBytes;

        /// <summary>Страницы Pdf документа</summary>
        public static List<PdfPage> Pages = new List<PdfPage>();

        /// <summary>Список объектов документа</summary>
        private static readonly List<PdfObject> Objects = new List<PdfObject>();

        /// <summary>Версия Pdf-документа</summary>
        public static decimal Version { get; private set; }

        #endregion

        #region Конструкторы

        /// <summary>Создаёт новый экземпляр класса <see cref="PdfDocument"/>.</summary>
        /// <param name="fileName">Имя файла *.pdf.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public PdfDocument(string fileName)
        {
            //Если файла нет выдаем ошибку
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                throw new ArgumentNullException(string.Format(PdfConsts.MSG_FILE_NOT_FOUND, fileName));

            var ext = Path.GetExtension(fileName);
            //Пустое разрешение или не pdf
            if (string.IsNullOrEmpty(ext) || !ext.Equals(PdfConsts.PDF_EXTENSION))
                throw new ArgumentException(PdfConsts.MSG_FILE_WRONG_FORMAT);

            _documentBytes = File.ReadAllBytes(fileName);
            Parse();
        }

        /// <summary>Создаёт новый экземпляр класса <see cref="PdfDocument"/>.</summary>
        /// <param name="inArray">Массив байт документа.</param>
        public PdfDocument(byte[] inArray)
        {
            if (inArray == null)
                throw new ArgumentNullException(PdfConsts.MSG_EMPTY_ARRAY);
            _documentBytes = inArray;
            Parse();
        }

        #endregion

        #region Методы

        /// <summary>Получает данные из Pdf-документа</summary>
        private static void Parse()
        {
            FillObjects();
            Version = GetVersion();
        }

        /// <summary>Заполняет список объектов документа</summary>
        private static void FillObjects()
        {
            var position = PdfFunctions.GetPosition(_documentBytes, 0, PdfConsts.PDF_TRAILER);
            position = PdfFunctions.GetPosition(_documentBytes, position, PdfConsts.PDF_SIZE) + 1;
            var value = PdfFunctions.GetValue(_documentBytes, position, PdfConsts.PDF_BACKSLASH);
            var objCount = Convert.ToInt32(value);

            for (var i = 1; i < objCount; i++)
            {
                var name = i.ToString(CultureInfo.InvariantCulture) + " 0 " + PdfConsts.PDF_OBJECT;
                var data = PdfFunctions.GetObjectData(_documentBytes, 0, name);
                if (data == null) 
                    continue;

                var type = GetObjectType(data);
                if (type == PdfObjectType.Page)
                    Pages.Add(new PdfPage(name, data));
                else
                    Objects.Add(new PdfObject(name, data, type));
            }
        }

        /// <summary>Возвращает тип объекта</summary>
        /// <param name="inBytes">Входной поток объекта между [1 0 obj] и [endobj].</param>
        /// <returns>Тип объекта, если он есть в перечислении PdfObjectType и Undefine если нет</returns>
        private static PdfObjectType GetObjectType(byte[] inBytes)
        {
            var position = PdfFunctions.GetPosition(inBytes, 0, PdfConsts.PDF_TYPE);
            if (position == -1)
                return PdfObjectType.Undefine;

            position += 2;
            var current = (char) inBytes[position];
            var value = "";
            while (current != PdfConsts.PDF_CLOSE_TRIANGLE_BRACKET && current != PdfConsts.PDF_BACKSLASH)
            {
                value += current;
                position++;
                current = (char) inBytes[position];
            }
            PdfObjectType result;
            return Enum.TryParse(value, false, out result) ? result : PdfObjectType.Undefine;
        }

        /// <summary>Возвращает версию Pdf-документа</summary>
        /// <returns>Версия pdf-документа</returns>
        private static decimal GetVersion()
        {
            var pos = PdfFunctions.GetPosition(_documentBytes, 0, PdfConsts.PDF_VERSION) + 1;
            if (pos == -1)
                return 0;

            var value = PdfFunctions.GetValue(_documentBytes, pos, PdfConsts.PDF_RETURN);

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        /// <summary>Возвращает текст pdf документа</summary>
        public string GetPdfText()
        {
            return
                (from obj in Objects from text in obj.TextObjects from line in text.TextLines select line).Aggregate(
                    "", (current, line) => current + line);
        }

        #endregion
    }
}
