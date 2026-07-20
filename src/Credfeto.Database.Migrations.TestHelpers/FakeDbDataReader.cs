using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Migrations.TestHelpers;

[SuppressMessage(
    category: "Microsoft.Design",
    checkId: "CA1010: Generic interface should also be implemented",
    Justification = "DbDataReader's non-generic IEnumerable is inherited from the BCL base class"
)]
public sealed class FakeDbDataReader : DbDataReader
{
    private readonly IReadOnlyList<long> _values;
    private int _index = -1;

    public FakeDbDataReader(IReadOnlyList<long> values)
    {
        this._values = values;
    }

    public override int Depth => 0;

    public override int FieldCount => 1;

    public override bool HasRows => this._values.Count != 0;

    public override bool IsClosed => false;

    public override int RecordsAffected => -1;

    public override object this[int ordinal] => this.GetInt64(ordinal);

    public override object this[string name] => this.GetInt64(this.GetOrdinal(name));

    public override bool GetBoolean(int ordinal) => throw new NotSupportedException();

    public override byte GetByte(int ordinal) => throw new NotSupportedException();

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override char GetChar(int ordinal) => throw new NotSupportedException();

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException();

    public override string GetDataTypeName(int ordinal) => nameof(Int64);

    public override DateTime GetDateTime(int ordinal) => throw new NotSupportedException();

    public override decimal GetDecimal(int ordinal) => throw new NotSupportedException();

    public override double GetDouble(int ordinal) => throw new NotSupportedException();

    public override IEnumerator GetEnumerator() => throw new NotSupportedException();

    [return: DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties
    )]
    public override Type GetFieldType(int ordinal) => typeof(long);

    public override float GetFloat(int ordinal) => throw new NotSupportedException();

    public override Guid GetGuid(int ordinal) => throw new NotSupportedException();

    public override short GetInt16(int ordinal) => throw new NotSupportedException();

    public override int GetInt32(int ordinal) => throw new NotSupportedException();

    public override long GetInt64(int ordinal) => this._values[this._index];

    public override string GetName(int ordinal) => "id";

    public override int GetOrdinal(string name) => 0;

    public override string GetString(int ordinal) => throw new NotSupportedException();

    public override object GetValue(int ordinal) => this.GetInt64(ordinal);

    public override int GetValues(object[] values)
    {
        values[0] = this.GetInt64(0);

        return 1;
    }

    public override bool IsDBNull(int ordinal) => false;

    public override bool NextResult() => false;

    public override bool Read()
    {
        this._index++;

        return this._index < this._values.Count;
    }
}
