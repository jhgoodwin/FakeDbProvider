# Fake Db Provider for System.Data.Common.DbConnection

Sometimes mocking just isn't enough, so you need a realistic double. This is where fakes come in.

To get started, I copied the code from here:
[Entity Framework Test code in github](https://github.com/aspnet/EntityFrameworkCore/tree/master/test/EFCore.Relational.Tests/TestUtilities/FakeProvider)

In order to make this work, I needed to fill in a few places. A proper fake should not `throw new NotImplementedException` for normal use of properties and methods. 