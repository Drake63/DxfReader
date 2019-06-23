using System.Windows;

namespace DxfReader
{
    interface IDxfOffset
    {
        void OffSet(Vector vector);
        string ToStringOfVerR13();                  // строковое представления для версии файла AC1012
    }
}
