namespace Core;

public class NotFoundException : Exception
{
    public string? EntityName { get; }
    public object? Key { get; }

    // Простое сообщение
    public NotFoundException(string message) : base(message)
    {
    }

    // Когда знаем, что искали
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    // С внутренним исключением (если нужно)
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}