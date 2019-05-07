// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Goodwin.John.Fakes.FakeDbProvider
{
    public class FakeDbCommand : DbCommand
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly FakeCommandExecutor _commandExecutor;

        public FakeDbCommand()
        {
        }

        public FakeDbCommand(
            FakeDbConnection connection,
            FakeCommandExecutor commandExecutor)
        {
            DbConnection = connection;
            _commandExecutor = commandExecutor;
        }

        protected override DbConnection DbConnection { get; set; }

        protected override DbTransaction DbTransaction { get; set; }

        private string DebugLogSuffix()
            => $"{nameof(FakeDbCommand)} with {nameof(CommandText)} starting with, '{CommandText.Substring(0, 50).Trim()}'";

        public override void Cancel()
        {
            Logger.Debug($"Attempted to cancel{DebugLogSuffix()}");
        }

        public override string CommandText { get; set; }

        public static int DefaultCommandTimeout = 30;

        public override int CommandTimeout { get; set; } = DefaultCommandTimeout;

        public override CommandType CommandType { get; set; }

        protected override DbParameter CreateDbParameter()
            => new FakeDbParameter();

        protected override DbParameterCollection DbParameterCollection { get; }
            = new FakeDbParameterCollection();

        public override void Prepare()
        {
            Logger.Debug($"Prepared{DebugLogSuffix()}");
        }

        public override int ExecuteNonQuery()
        {
            AssertTransaction();

            return _commandExecutor.ExecuteNonQuery(this);
        }

        public override object ExecuteScalar()
        {
            AssertTransaction();

            return _commandExecutor.ExecuteScalar(this);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            AssertTransaction();

            return _commandExecutor.ExecuteReader(this, behavior);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            AssertTransaction();

            return _commandExecutor.ExecuteNonQueryAsync(this, cancellationToken);
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            AssertTransaction();

            return _commandExecutor.ExecuteScalarAsync(this, cancellationToken);
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            AssertTransaction();

            return _commandExecutor.ExecuteReaderAsync(this, behavior, cancellationToken);
        }

        public override bool DesignTimeVisible
        {
            get => false;
            set => Logger.Debug($"Set {nameof(DesignTimeVisible)} value {value}{DebugLogSuffix()}");
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => UpdateRowSource.None;
            set => Logger.Debug($"Set {nameof(UpdatedRowSource)} value {value}{DebugLogSuffix()}");
        }

        public int DisposeCount { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }

            base.Dispose(disposing);
        }

        private void AssertTransaction()
        {
            if (Transaction == null)
            {
                Debug.Assert(((FakeDbConnection)DbConnection).ActiveTransaction == null);
            }
            else
            {
                var transaction = (FakeDbTransaction)Transaction;

                Debug.Assert(transaction.Connection == Connection);
                Debug.Assert(transaction.DisposeCount == 0);
            }
        }
    }
}
