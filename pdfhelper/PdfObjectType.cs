
namespace pdfHelper
{
    /// <summary>Нумератор типов объектов</summary>
    public enum PdfObjectType
    {
        //тип неопределен
        Undefine = 0,
        //каталог pdf документа
        Catalog = 1,
        //Дерево страниц
        Pages = 2,
        //Страница
        Page = 3,
        //Аннотация
        Annot = 4,
        //Шрифт
        Font = 5,
        //Объект
        XObject = 6,
        //OCG
        OCG =7
    }
}
