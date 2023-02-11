using System.Diagnostics;
using Credfeto.Database.Interfaces;
using Credfeto.Database.Source.Generation.Example.Mappers;
using Credfeto.Database.Source.Generation.Example.Primatives;

namespace Credfeto.Database.Source.Generation.Example.Models;

[DebuggerDisplay("Id = {Id}, Name = {Name}, Address = {Address}")]
public sealed record Accounts(int Id,
                              string Name,
                              [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                              AccountAddress Address);