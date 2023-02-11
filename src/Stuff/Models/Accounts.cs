using System.Diagnostics;
using Stuff.Infrastructure;
using Stuff.Mappers;
using Stuff.Primatives;

namespace Stuff.Models;

[DebuggerDisplay("Id = {Id}, Name = {Name}, Address = {Address}")]
public sealed record Accounts(int Id,
                              string Name,
                              [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                              AccountAddress Address);