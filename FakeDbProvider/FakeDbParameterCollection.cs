// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Goodwin.John.Fakes.FakeDbProvider
{
    public class FakeDbParameterCollection : DbParameterCollection
    {
        private readonly List<object> _parameters = new List<object>();

        public override int Count => _parameters.Count;

        public override int Add(object value)
        {
            _parameters.Add(value);

            return _parameters.Count - 1;
        }

        protected override DbParameter GetParameter(int index)
            => (DbParameter)_parameters[index];

        public override IEnumerator GetEnumerator()
            => _parameters.GetEnumerator();

        public override object SyncRoot => ((ICollection) _parameters).SyncRoot;

        public override void AddRange(Array values)
        {
            foreach (var parameter in values.Cast<DbParameter>())
            {
                _parameters.Add(parameter);
            }
        }

        public override void Clear() => _parameters.Clear();

        public override bool Contains(string value)
            => _parameters.Cast<DbParameter>().Any(p => p.ParameterName.Equals(value));

        public override bool Contains(object value)
            => _parameters.Cast<DbParameter>().Any(p => p.Value.Equals(value));

        public override void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ((ICollection)_parameters).CopyTo(array, index);
        }

        private static string NormalizedParameterName(string parameterName)
            => parameterName.TrimStart(':', '@');

        public override int IndexOf(string parameterName)
        {
            parameterName = NormalizedParameterName(parameterName);
            for (int i = 0; i < _parameters.Count; i++)
            {
                if (NormalizedParameterName(((DbParameter)_parameters[i]).ParameterName)
                    .Equals(parameterName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
            }

            return -1;
        }

        public override int IndexOf(object value)
        {
            for (int i = 0; i < _parameters.Count; i++)
            {
                if ((_parameters[i] as DbParameter).Value.Equals(value))
                    return i;
            }

            return -1;
        }

        public override void Insert(int index, object value) => _parameters.Insert(index, value);

        public override void Remove(object value) => _parameters.Remove(value);

        public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));

        public override void RemoveAt(int index)
        {
            if (_parameters.Count - 1 < index)
                throw new IndexOutOfRangeException();
            _parameters.RemoveAt(index);
        }

        protected override DbParameter GetParameter(string parameterName)
            => (DbParameter) _parameters[IndexOf(parameterName)];

        protected override void SetParameter(string parameterName, DbParameter value)
            => SetParameter(IndexOf(parameterName), value);

        protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
    }
}
