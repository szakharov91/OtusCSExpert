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
    private readonly string randomKey = Guid.NewGuid().ToString();
    private readonly string defaultKey = "User:1";
    private readonly byte[] defaultValue = Encoding.UTF8.GetBytes("data");
    private readonly byte[] updatedValue = Encoding.UTF8.GetBytes("new data");

    [Fact]
    public void SimpleStore_Set_Then_Get_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();
        
        // Act
        store.Set(defaultKey, defaultValue);
        var value = store.Get(defaultKey);

        // Assert
        value.Should().BeEqualTo(defaultValue);
    }

    [Fact]
    public void SimpleStore_Set_And_Update_Then_Get_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(defaultKey, defaultValue);
        store.Set(defaultKey, updatedValue);
        var value = store.Get(defaultKey);

        // Assert
        value.Should().BeEqualTo(updatedValue);
    }

    [Fact]
    public void SimpleStore_Remove_Key_And_Value()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(defaultKey, defaultValue);
        store.Delete(defaultKey);
        var value = store.Get(defaultKey);

        // Assert
        value.Should().BeNull();
    }

    [Fact]
    public void SimpleStore_Set_And_Then_Get_Unexisting_Key()
    {
        // Arrange
        IStoragable store = new SimpleStore();

        // Act
        store.Set(defaultKey, defaultValue);
        store.Set(defaultKey, updatedValue);
        var value = store.Get(randomKey);

        // Assert
        value.Should().BeNull();
    }
}
