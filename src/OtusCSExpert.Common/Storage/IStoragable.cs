namespace OtusCSExpert.Common.Storage;

public interface IStoragable
{
    /// <summary> добавляет или обновляет значение по ключу. </summary>
    void Set(string key, byte[] value);

    /// <summary> возвращает значение по ключу или null, если ключ не найден. </summary>
    byte[] Get(string key);

    /// <summary> удаляет ключ и значение. </summary>
    void Delete(string key);

    /// <summary> Получение статистики </summary>
    (long setCount, long getCount, long deleteCount) GetStatistics();
}
