using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Goodwin.John.Fakes.FakeDbProvider
{
    /// <summary>
    /// Base class for creating FakeDb based tests
    /// To use, inherit, then in your test methods, assign a function to any combination of:
    /// * <see cref="ExecuteNonQueryAsync"/>
    /// * <see cref="ExecuteScalarAsync"/>
    /// * <see cref="ExecuteReaderAsync"/>
    /// Then, use Connection afterwards to use as a data provider.
    /// </summary>
    public abstract class AbstractFakeDbTest: IDisposable
    {
        #region private

        private bool _readonly;
        private readonly Lazy<FakeDbConnection> _connection;
        private Func<FakeDbCommand, CancellationToken, Task<int>> _executeNonQueryAsync;
        private Func<FakeDbCommand, CancellationToken, Task<object>> _executeScalarAsync;
        private Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> _executeReaderAsync;

        #endregion


        protected DbConnection Connection
        {
            get
            {
                _readonly = true;
                return _connection.Value;
            }
        }

        protected FakeDbConnection FakeConnection
        {
            get
            {
                _readonly = true;
                return _connection.Value;
            }
        }

        /// <summary>
        /// Allows mocking query execution that returns no results
        /// </summary>
        protected Func<FakeDbCommand, CancellationToken, Task<int>> ExecuteNonQueryAsync
        {
            get => _executeNonQueryAsync;
            set
            {
                if (_readonly)
                {
                    throw new ReadOnlyException(
                        $"Once {nameof(Connection)} is accessed, the {nameof(ExecuteNonQueryAsync)} is locked.");
                }

                _executeNonQueryAsync = value;
            }
        }

        /// <summary>
        /// Allows mocking query execution that returns a single scalar
        /// </summary>
        protected Func<FakeDbCommand, CancellationToken, Task<object>> ExecuteScalarAsync
        {
            get => _executeScalarAsync;
            set
            {
                if (_readonly)
                {
                    throw new ReadOnlyException(
                        $"Once {nameof(Connection)} is accessed, the {nameof(ExecuteScalarAsync)} is locked.");
                }

                _executeScalarAsync = value;
            }
        }

        /// <summary>
        /// Allows mocking data reading from a query/stored procedure 
        /// </summary>
        protected Func<FakeDbCommand, CommandBehavior, CancellationToken, Task<DbDataReader>> ExecuteReaderAsync
        {
            get => _executeReaderAsync;
            set
            {
                if (_readonly)
                {
                    throw new ReadOnlyException(
                        $"Once {nameof(Connection)} is accessed, the {nameof(ExecuteReaderAsync)} is locked.");
                }

                _executeReaderAsync = value;
            }
        }

        protected AbstractFakeDbTest()
        {
            int ExecuteNonQuery(FakeDbCommand command)
                => Task.Run(() => ExecuteNonQueryAsync(command, CancellationToken.None)).Result;

            object ExecuteScalar(FakeDbCommand command)
                => Task.Run(() => ExecuteScalarAsync(command, CancellationToken.None)).Result;

            DbDataReader ExecuteReader(FakeDbCommand command, CommandBehavior commandBehavior)
                => Task.Run(() => ExecuteReaderAsync(command, commandBehavior, CancellationToken.None)).Result;

            ExecuteNonQueryAsync = (command, token)
                => throw new NotImplementedException(nameof(ExecuteNonQueryAsync));
            ExecuteScalarAsync = (command, token)
                => throw new NotImplementedException(nameof(ExecuteScalarAsync));
            ExecuteReaderAsync = (command, commandBehavior, token)
                => throw new NotImplementedException(nameof(ExecuteReaderAsync));

            var fakeCommandExecutor = new Lazy<FakeCommandExecutor>(() => new FakeCommandExecutor(
                ExecuteNonQuery, ExecuteScalar, ExecuteReader,
                ExecuteNonQueryAsync, ExecuteScalarAsync, ExecuteReaderAsync));
            _connection = new Lazy<FakeDbConnection>(
                () => new FakeDbConnection(string.Empty, fakeCommandExecutor.Value));
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}