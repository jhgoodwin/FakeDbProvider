# Fake Db Provider for System.Data.Common.DbConnection

## Master Branch Status

* [![Build Status](https://johnhgoodwin.visualstudio.com/Public/_apis/build/status/FakeDbProvider-CI?branchName=master)](https://johnhgoodwin.visualstudio.com/Public/_build/latest?definitionId=3&branchName=master)

Sometimes mocking just isn't enough, so you need a realistic double. This is where fakes come in.

To get started, I copied the code from here:
[Entity Framework Test code in github](https://github.com/aspnet/EntityFrameworkCore/tree/master/test/EFCore.Relational.Tests/TestUtilities/FakeProvider)

In order to make this work, I needed to fill in a few places. A proper fake should not `throw new NotImplementedException` for normal use of properties and methods. 

How to use:

Create a new test class, inheriting from AbstractFakeDbTest, like this:

```csharp
public class MyInsertTests: AbstractFakeDbTest
{
    private int _commitCount = 0;
    
    [Fact]
    public async void TransactionalSaveDataServer_SaveCausesOneSaveForSingleObject()
    {
        var upsertSaves = 0;
        var scalarExecutes = 0;
        ExecuteNonQueryAsync = (command, token) => Task.FromResult(upsertSaves++);
        ExecuteScalarAsync = (command, token) =>
        {
            scalarExecutes++;
            return Task.FromResult<object>("My stub scalar");
        };
        FakeConnection.TransactionChanged += OnTransactionChanged;
        try
        {
            var server = new MyClassUnderTest(Connection);
            var saveResult = await server.MySaveMethodAsync("My data to save");
            _commitCount.Should().Be(1);
            upsertSaves.Should().Be(1);
            scalarExecutes.Should().Be(1);
        }
        finally
        {
            FakeConnection.TransactionChanged -= OnTransactionChanged;
        }
    }
    
    protected virtual void OnTransactionChanged(object sender, TransactionChangedEventArgs e)
        => _commitCount = ((e.CurrentValue ?? e.NextValue) as FakeDbTransaction)?.CommitCount ?? 0;
}
```