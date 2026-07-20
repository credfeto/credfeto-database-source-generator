using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Credfeto.Database.Migrations.Tests.Helpers;

internal sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = [];

    public override int Count => this._parameters.Count;

    public override object SyncRoot { get; } = new();

    public override int Add(object value)
    {
        this._parameters.Add((DbParameter)value);

        return this._parameters.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (object value in values.Cast<object>())
        {
            this.Add(value);
        }
    }

    public override void Clear()
    {
        this._parameters.Clear();
    }

    public override bool Contains(object value)
    {
        return this._parameters.Contains((DbParameter)value);
    }

    public override bool Contains(string value)
    {
        return this.IndexOf(value) >= 0;
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)this._parameters).CopyTo(array: array, index: index);
    }

    public override IEnumerator GetEnumerator()
    {
        return this._parameters.GetEnumerator();
    }

    protected override DbParameter GetParameter(int index)
    {
        return this._parameters[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return this._parameters[this.IndexOf(parameterName)];
    }

    public override int IndexOf(object value)
    {
        return this._parameters.IndexOf((DbParameter)value);
    }

    public override int IndexOf(string parameterName)
    {
        return this._parameters.FindIndex(match: p => StringComparer.Ordinal.Equals(p.ParameterName, parameterName));
    }

    public override void Insert(int index, object value)
    {
        this._parameters.Insert(index: index, item: (DbParameter)value);
    }

    public override void Remove(object value)
    {
        this._parameters.Remove((DbParameter)value);
    }

    public override void RemoveAt(int index)
    {
        this._parameters.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        this.RemoveAt(this.IndexOf(parameterName));
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        this._parameters[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        this._parameters[this.IndexOf(parameterName)] = value;
    }
}
