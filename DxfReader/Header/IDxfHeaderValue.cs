namespace DxfReader
{
    // Общий интерфейс, который должны реализовать все классы заголовочной секции
    interface IDxfHeaderValue
    {
        string Name { get; }
    }
}
