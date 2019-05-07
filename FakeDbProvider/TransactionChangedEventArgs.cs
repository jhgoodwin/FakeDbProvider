using System.Data.Common;

namespace Goodwin.John.Fakes.FakeDbProvider
{
    public class TransactionChangedEventArgs
    {
        public DbTransaction CurrentValue { get; set; }
        public DbTransaction NextValue { get; set; }
    }
}