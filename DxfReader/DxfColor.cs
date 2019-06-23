namespace DxfReader
{
    // Статический класс для установки цвета линии объекта, в зависимости от полученных данных
    static class DxfColor
    {
        public static DxfColors Create (DxfCodePair pair, bool ignoreLineType)
        {            
            if (pair == null) return DxfColors.MainOutline;

            switch(pair.AsInt)
            {
                case 1: // маркировка
                    return DxfColors.LineOfMarking;
                case 100:
                    return DxfColors.LineOfMarking;
                case 4: // незамкнутый контур
                    return DxfColors.NotClosedLine;
                case 40:
                    return DxfColors.NotClosedLine;
                case 93: // основной контур
                    return DxfColors.MainOutline;
                case 5:
                    return DxfColors.MainOutline;
                case 7:
                    return DxfColors.MainOutline;
                case 143:
                    return DxfColors.MainOutline;
                case 150:
                    return DxfColors.MainOutline;
                default: // все осталные коды итерпритировать как основная линия
                    if (ignoreLineType)
                        return DxfColors.MainOutline;
                    else
                        return DxfColors.NoColor;
            }
        } 
    }
}
