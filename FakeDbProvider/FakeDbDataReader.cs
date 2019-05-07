// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Goodwin.John.Fakes.FakeDbProvider
{
    public class FakeDbDataReader : DbDataReader
    {
        private readonly string[] _columnNames;
        private readonly IList<object[]> _results;

        private object[] _currentRow;
        private int _rowIndex;

        public FakeDbDataReader(string[] columnNames = null, IList<object[]> results = null)
        {
            _columnNames = columnNames ?? Array.Empty<string>();
            _results = results ?? new List<object[]>();
        }

        public override bool Read()
        {
            _currentRow = _rowIndex < _results.Count
                ? _results[_rowIndex++]
                : null;

            return _currentRow != null;
        }

        public int ReadAsyncCount { get; private set; }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            ReadAsyncCount++;

            _currentRow = _rowIndex < _results.Count
                ? _results[_rowIndex++]
                : null;

            return Task.FromResult(_currentRow != null);
        }

        public int CloseCount { get; private set; }

        public override void Close()
        {
            CloseCount++;
        }

        public int DisposeCount { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;

                base.Dispose(true);
            }
        }

        public override int FieldCount => _columnNames.Length;

        public override string GetName(int ordinal) => _columnNames[ordinal];

        public override bool IsDBNull(int ordinal) => _currentRow[ordinal] == DBNull.Value;

        public override object GetValue(int ordinal) => _currentRow[ordinal];

        public int GetInt32Count { get; private set; }

        public override int GetInt32(int ordinal)
        {
            GetInt32Count++;

            return (int)_currentRow[ordinal];
        }

        public override object this[string name] => _currentRow[GetOrdinal(name)];

        public override object this[int ordinal] => GetValue(ordinal);

        public override int Depth => throw new NotImplementedException();

        public override bool HasRows => _results.Any();

        public override bool IsClosed => CloseCount > 0;

        public override int RecordsAffected => 0;

        public override bool GetBoolean(int ordinal) => (bool)_currentRow[ordinal];

        public override byte GetByte(int ordinal) => (byte)_currentRow[ordinal];

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal) => (char)_currentRow[ordinal];

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal) => (DateTime)_currentRow[ordinal];

        public override decimal GetDecimal(int ordinal) => (decimal)_currentRow[ordinal];

        public override double GetDouble(int ordinal) => (double)_currentRow[ordinal];

        public override IEnumerator GetEnumerator() => _results.GetEnumerator();

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal) => (float)_currentRow[ordinal];

        public override Guid GetGuid(int ordinal) => (Guid)_currentRow[ordinal];

        public override short GetInt16(int ordinal) => (short)_currentRow[ordinal];

        public override long GetInt64(int ordinal) => (long)_currentRow[ordinal];

        public override int GetOrdinal(string name)
        {
            var ordinal = Array.IndexOf(_columnNames, name);
            if (ordinal < 0)
            {
                throw new System.IndexOutOfRangeException(
                    $"Column {name} does not exist in the list of columns {String.Join(", ", _columnNames)}");
            }

            return ordinal;
        }

        public override string GetString(int ordinal) => (string)_currentRow[ordinal];

        public override int GetValues(object[] values)
        {
            var columnsCopied = Math.Min(_currentRow.Length, values.Length);
            Array.Copy(_currentRow, values, columnsCopied);
            return columnsCopied;
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }
    }
}
