using System.Diagnostics;

namespace Stuff;

[DebuggerDisplay("Id = {Id}, Name = {Name}, Address = {Address}")]
public sealed record Accounts(int Id,
                              string Name,
                              [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                              AccountAddress Address);