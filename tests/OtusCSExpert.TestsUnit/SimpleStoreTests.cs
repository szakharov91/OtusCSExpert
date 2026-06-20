using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OtusCSExpert.Common.Storage;
using OtusCSExpert.Common.Types;

namespace OtusCSExpert.TestsUnit;

public class SimpleStoreTests
{
    private readonly string _randomKey = Guid.NewGuid().ToString();
    private readonly string _defaultKey = "User:1";
    private readonly byte[] _defaultValue = Encoding.UTF8.GetBytes("data");
    private readonly byte[] _updatedValue = Encoding.UTF8.GetBytes("new data");

    [Fact]
    public void SimpleStore_Set_Then_Get_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();
        
        // Act
        store.Set(_defaultKey, _defaultValue);
        var value = store.Get(_defaultKey);

        // Assert
        value.Should().BeEqualTo(_defaultValue);
    }

    [Fact]
    public void SimpleStore_Set_And_Update_Then_Get_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(_defaultKey, _defaultValue);
        store.Set(_defaultKey, _updatedValue);
        var value = store.Get(_defaultKey);

        // Assert
        value.Should().BeEqualTo(_updatedValue);
    }

    [Fact]
    public void SimpleStore_Remove_Key_And_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(_defaultKey, _defaultValue);
        store.Delete(_defaultKey);
        var value = store.Get(_defaultKey);

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public void SimpleStore_Set_And_Then_Get_Unexisting_Key()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(_defaultKey, _defaultValue);
        store.Set(_defaultKey, _updatedValue);
        var value = store.Get(_randomKey);

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public async Task SimpleStore_Concurrent_Set_UniqueKeys_AllValuesAreCorrect()
    {
        // Arrange
        using var store = new SimpleStore();
        const int taskCount = 50;

        var tasks = Enumerable.Range(0, taskCount)
            .Select(i => Task.Run(() =>
                store.Set($"key:{i}", Encoding.UTF8.GetBytes($"value:{i}"))))
            .ToArray();

        // Act
        await Task.WhenAll(tasks);

        // Assert
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        setCount.Should().Be(taskCount);
        getCount.Should().Be(0);
        deleteCount.Should().Be(0);

        // Assert
        for (int i = 0; i < taskCount; i++)
        {
            store.Get($"key:{i}").Should().BeEqualTo(Encoding.UTF8.GetBytes($"value:{i}"));
        }
    }

    [Fact]
    public async Task SimpleStore_Concurrent_SetAndGet_MixedOperations_CountersMatchExpected()
    {
        // Arrange
        using var store = new SimpleStore();
        const int keyCount = 10;
        const int setTasksPerKey = 5;
        const int getTasksPerKey = 5;

        for (int i = 0; i < keyCount; i++)
            store.Set($"key:{i}", Encoding.UTF8.GetBytes($"initial:{i}"));

        var tasks = new List<Task>();

        for (int i = 0; i < keyCount; i++)
        {
            for (int j = 0; j < setTasksPerKey; j++)
            {
                var ki = i;
                var kj = j;
                tasks.Add(Task.Run(() =>
                    store.Set($"key:{ki}", Encoding.UTF8.GetBytes($"updated:{ki}:{kj}"))));
            }

            for (int j = 0; j < getTasksPerKey; j++)
            {
                var ki = i;
                tasks.Add(Task.Run(() => store.Get($"key:{ki}")));
            }
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        setCount.Should().Be(keyCount + keyCount * setTasksPerKey);
        getCount.Should().Be(keyCount * getTasksPerKey);
        deleteCount.Should().Be(0);

        // Assert
        for (int i = 0; i < keyCount; i++)
        {
            store.Get($"key:{i}").Should().NotBeNull();
        }
    }
}
