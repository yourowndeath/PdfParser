using System;
using System.IO;
using System.Linq;
using ZLibNet;

namespace pdfHelper
{
    /// <summary>Вспомогательный функции для работы с массивами данных pdf</summary>
    internal class PdfFunctions
    {
        #region Поля

        /// <summary>Начало текстового блока</summary>
        private const string BeginTextBlock = "BT\n";

        /// <summary>Конец текстового блока</summary>
        private const string EndTextBlock = "ET\n";

        /// <summary>Блок простого текста</summary>
        private const string PlainTextBlock = "Tj\n";

        /// <summary>Блок текста PDF</summary>
        private const string TextBlock = "TJ\n";

        #endregion

        #region Методы

        /// <summary>Возвращает массив байт исходного файла без текстовых полей</summary>
        /// <param name="fileName">Имя файла Pdf.</param>
        /// <returns>массив байт без текстовых полей</returns>
        /// <exception cref="System.ArgumentNullException">Если файл не найден</exception>
        public static byte[] RemoveTextBlocks(string fileName)
        {
            //Если файла нет выдаем ошибку
            if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                throw new ArgumentNullException(String.Format(PdfConsts.MSG_FILE_NOT_FOUND, fileName));

            //получаем бинарный файл pdf
            var pdfFile = File.ReadAllBytes(fileName);
            var i = 0;

            //Чистим закодированный текст
            while (i < pdfFile.Length)
            {
                //Получаем позицию блока со сжатием
                var flateDecode = GetPosition(pdfFile, i, PdfConsts.PDF_FLATE_DECODE);
                if (flateDecode == -1)
                    break;
                //Получаем позицию начала потока
                var startIndex = GetPosition(pdfFile, flateDecode, PdfConsts.PDF_START_STREAM) + 1;
                if (startIndex == -1)
                    break;
                //Получаем позицию конца потока
                var endIndex = GetPosition(pdfFile, startIndex, PdfConsts.PDF_END_STREAM) - 9;
                if (endIndex == -1)
                    break;

                //Убираем #10#13 в начале и конце
                if (pdfFile[startIndex] == 0x0d && pdfFile[startIndex + 1] == 0x0a) startIndex += 2;
                else if (pdfFile[startIndex] == 0x0a || pdfFile[startIndex] == 0x0d) startIndex++;

                if (pdfFile[endIndex - 1] == 0x0d && pdfFile[endIndex] == 0x0a) endIndex -= 2;
                else if (pdfFile[endIndex - 1] == 0x0a) endIndex--;
                else if (pdfFile[endIndex] == 0x0d || pdfFile[endIndex] == 0x0a)
                    endIndex--;

                var inBytes = new byte[endIndex - startIndex + 1];
                var counter = 0;
                for (var j = startIndex; j <= endIndex; j++)
                {
                    inBytes[counter] = pdfFile[j];
                    counter++;
                }
                //Проверка на компрессор ZIPLib выполняет декомпрессию при первых двух байтах 72,137
                if (inBytes[0] == 72)
                {
                    inBytes = Decompress(inBytes);
                    EraseArray(inBytes);
                    inBytes = Compress(inBytes);
                    counter = 0;
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        if (counter < inBytes.Length)
                            pdfFile[j] = inBytes[counter];
                        counter++;
                    }
                }
                i = endIndex;
            }
            //Если остались незакодированные строки их тоже чистим
            EraseArray(pdfFile);
            return pdfFile;
        }


        /// <summary>Преобразует массив байт в строку</summary>
        /// <param name="inArray">Входной массив байт.</param>
        /// <returns>строка, соответствующая массиву байт</returns>
        public static string ConvertByteToString(byte[] inArray)
        {
            return inArray.Aggregate("", (current, element) => current + (char) element);
        }

        /// <summary>Возвращает позицию строки в массиве байт</summary>
        /// <param name="buffer">Входной массив байт.</param>
        /// <param name="startIndex">Начальный индекс поиска.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Возвращаемый индекс входного массива</returns>
        public static int GetPosition(byte[] buffer, int startIndex, string value)
        {
            var inBytes = new byte[value.Length];

            for (var i = 0; i < value.Length; i++)
                inBytes[i] = (byte) value[i];
            return GetPosition(buffer, startIndex, inBytes);
        }

        /// <summary>Возвращает позицию строки в массиве байт</summary>
        /// <param name="buffer">Входной массив байт.</param>
        /// <param name="startIndex">Начальный индекс поиска.</param>
        /// <param name="inBytes">Искомое значение.</param>
        /// <returns></returns>
        public static int GetPosition(byte[] buffer, int startIndex, byte[] inBytes)
        {
            for (int i = startIndex; i < buffer.Length; i++)
            {
                if (buffer[i] == inBytes[0])
                {
                    var counter = 1;
                    for (int j = 1; j < inBytes.Length; j++)
                    {
                        if (j + i < buffer.Length)
                            if (buffer[i + j] == inBytes[j])
                                counter++;
                    }
                    if (counter == inBytes.Length)
                        return i + inBytes.Length - 1;
                }
            }
            return -1;
        }

        /// <summary>Возвращает значение атрибута объекта</summary>
        /// <param name="inBytes">Входной массив объекта.</param>
        /// <param name="startIndex">Начало поиска в массиве.</param>
        /// <param name="name">Имя атрибута.</param>
        /// <returns>Значение атрибута</returns>
        public static string GetAttribute(byte[] inBytes, int startIndex, string name)
        {
            var position = GetPosition(inBytes, startIndex, name);
            if (position == -1)
                return "";
            position++;
            var current = (char) inBytes[position];
            var value = "";
            while (current != PdfConsts.PDF_BACKSLASH && current != PdfConsts.PDF_CLOSE_TRIANGLE_BRACKET)
            {
                value += current;
                position++;
                current = (char) inBytes[position];
            }
            return value;
        }

        /// <summary>Возвращает значение текстового атрибута</summary>
        /// <param name="inBytes">Входной массив объекта.</param>
        /// <param name="startIndex">Начало поиска в массиве.</param>
        /// <param name="name">Имя атрибута.</param>
        /// <param name="position">Конечный индекс поиска.</param>
        /// <returns>Значение атрибута</returns>
        public static string GetTextAttribute(byte[] inBytes, int startIndex, string name, out int position)
        {
            position = startIndex;
            var pos = GetPosition(inBytes, startIndex, name);
            if (pos == -1)
                return "";
            var beginIndex = pos - name.Length;
            var endIndex = beginIndex;
            var current = (char) inBytes[beginIndex];
            var value = "";
            while (current != 10 && endIndex != 0)
            {
                endIndex--;
                current = (char) inBytes[endIndex];
            }
            for (int i = endIndex; i <= beginIndex; i++)
                value += (char) inBytes[i];
            position = beginIndex + name.Length;
            return value;
        }

        /// <summary>Возвращает массив байт объекта (между [obj] и [endobj])</summary>
        /// <param name="buffer">Входной массив байт.</param>
        /// <param name="startIndex">Начальный индекс поиска.</param>
        /// <param name="objName">Имя объекта [1 0 obj].</param>
        /// <returns>Массив байт объекта</returns>
        public static byte[] GetObjectData(byte[] buffer, int startIndex, string objName)
        {
            var name = new byte[objName.Length + 1];
            name[0] = 10;
            for (var i = 0; i < objName.Length; i++)
                name[i + 1] = (byte) objName[i];

            var position = GetPosition(buffer, startIndex, name);
            if (position == -1)
            {
                name[0] = 13;
                position = GetPosition(buffer, startIndex, name);
                if (position == -1)
                    return null;
            }

            var endPosition = GetPosition(buffer, position, PdfConsts.PDF_END_OBJECT) - PdfConsts.PDF_END_OBJECT.Length;
            var result = new byte[endPosition - position + 1];
            var counter = 0;
            for (var i = position; i <= endPosition; i++)
            {
                result[counter] = buffer[i];
                counter++;
            }
            return result;
        }

        /// <summary>Разжимает массив</summary>
        /// <param name="input">Входной массив.</param>
        /// <returns>Разжатый массив</returns>
        public static byte[] Decompress(byte[] input)
        {
            try
            {
                return ZLibCompressor.DeCompress(input);
            }
            catch (Exception)
            {
                throw new ArgumentException("Ошибка декомпрессии");
            }

        }

        /// <summary>Сжимает массив</summary>
        /// <param name="data">Входной массив.</param>
        /// <returns>Сжатый массив</returns>
        public static byte[] Compress(byte[] data)
        {
            return ZLibCompressor.Compress(data);
        }

        /// <summary>Удаляет все данные между BT и ET в массиве</summary>
        /// <param name="inArray">Входной массив.</param>
        private static void EraseArray(byte[] inArray)
        {
            for (int i = 0; i < inArray.Length; i++)
            {
                var beginText = GetPosition(inArray, i, BeginTextBlock);
                if (beginText == -1)
                    break;
                beginText += 1;
                var endText = GetPosition(inArray, beginText, EndTextBlock);
                if (endText == -1)
                    break;
                endText -= 2;
                for (int j = beginText; j < endText; j++)
                {
                    var pos = GetPosition(inArray, j, PlainTextBlock);
                    if (pos != -1)
                    {

                        for (int k = pos - 1; k > 0; k--)
                        {
                            if (inArray[k] == 10)
                                break;
                            inArray[k] = 0;
                        }
                        j = pos;
                    }
                }
                for (int j = beginText; j < endText; j++)
                {
                    var pos = GetPosition(inArray, j, TextBlock);
                    if (pos != -1)
                    {

                        for (int k = pos - 1; k > 0; k--)
                        {
                            if (inArray[k] == 10)
                                break;
                            inArray[k] = 0;
                        }
                        j = pos;
                    }
                }
                i = endText + 1;
            }
        }

        /// <summary>Возвращает массив байт сжатого текстового блока</summary>
        /// <param name="inArray">Входной массив.</param>
        /// <param name="startIndex">Начальный индекс поиска.</param>
        /// <param name="position">Конечный индекс поиска.</param>
        /// <returns>массив байт текстового блока</returns>
        public static byte[] GetEncodedTextBlock(byte[] inArray, int startIndex, out int position)
        {
            position = startIndex;
            var startPos = GetPosition(inArray, startIndex, BeginTextBlock);
            if (startPos == -1)
                return null;
            startPos++;
            var endPos = GetPosition(inArray, startPos, EndTextBlock);
            if (endPos == -1)
                return null;
            position = endPos;
            endPos -= 2;
            var outArray = new byte[endPos - startPos];
            var counter = 0;
            for (int i = startPos; i < endPos; i++)
            {
                outArray[counter] = inArray[i];
                counter++;
            }
            return outArray;
        }

        /// <summary>Возвращает массив байт не сжатого текстового блока</summary>
        /// <param name="inArray">Входной массив.</param>
        /// <param name="startIndex">Начальный индекс поиска.</param>
        /// <param name="position">Конечный индекс поиска.</param>
        /// <returns>массив байт текстового блока</returns>
        public static byte[] GetTextBlock(byte[] inArray, int startIndex, out int position)
        {
            position = startIndex;
            var startPos = GetPosition(inArray, startIndex, BeginTextBlock);
            if (startPos == -1)
                return null;
            startPos++;
            var endPos = GetPosition(inArray, startPos, EndTextBlock);
            if (endPos == -1)
                return null;
            position = endPos;
            endPos -= 2;
            var outArray = new byte[endPos - startPos];
            var counter = 0;
            for (int i = startPos; i < endPos; i++)
            {
                outArray[counter] = inArray[i];
                counter++;
            }
            return outArray;
        }


        /// <summary>Конвертирует массив байт в Unicode строку</summary>
        /// <param name="inArray">Входной массив.</param>
        /// <returns>Unicode строка</returns>
        public static string AnsiToUnicode(byte[] inArray)
        {
            var byteArray = System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(1251),
                System.Text.Encoding.Unicode, inArray);
            return System.Text.Encoding.Unicode.GetString(byteArray);
        }

        /// <summary>Конвертирует ANSI строку в Unicode строку</summary>
        /// <param name="text">ANSI строка.</param>
        /// <returns>Unicode строка</returns>
        public static string AnsiToUnicode(string text)
        {
            var buffer = new byte[text.Length];
            int i = 0;
            foreach (var element in text)
            {
                buffer[i] = (byte) element;
                i++;
            }
            return AnsiToUnicode(buffer);
        }

        /// <summary>Возвращает значение</summary>
        /// <param name="bytes">Массив байт.</param>
        /// <param name="from">Начало значения.</param>
        /// <param name="to">Конец значения.</param>
        /// <returns>Значение</returns>
        /// <exception cref="ArgumentNullException">bytes</exception>
        /// <exception cref="ArgumentException">Выход за границы массива</exception>
        public static string GetValue(byte[] bytes, int from, int to)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (from < 0 || to < 0 || from > bytes.Length || to > bytes.Length)
                throw new ArgumentException("Выход за границы массива");

            var pos = from;
            var res = (char) bytes[pos];
            var value = string.Empty;
            while (res != to)
            {
                value += res;
                pos++;
                res = (char) bytes[pos];
            }
            return value;
        }

        #endregion
    }
}
