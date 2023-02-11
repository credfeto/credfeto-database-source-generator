create schema if not exists ethereum;

create table if not exists ethereum.account
(
  id      serial
  constraint account_pk
  primary key,
  name    varchar(200) not null,
  address char(42)     not null
  );

alter table ethereum.account
  owner to markr;

create or replace function account_get(id integer)
    returns TABLE(id integer, name text, address character)
    language sql
as
$$
SELECT a.id, a.name, a.address from ethereum.account a where a.id = account_get.id;
$$;

alter function ethereum.account_get(integer) owner to markr;

create or replace function ethereum.account_getall(address character)
    returns TABLE(id integer, name text, address character)
    language sql
as
$$
SELECT a.id, a.name, a.address from ethereum.account a where a.address = account_getall.address;
$$;

alter function ethereum.account_getall(char) owner to markr;

create or replace procedure ethereum.account_insert(IN name character varying, IN address character)
    language sql
as
$$
    INSERT INTO ethereum.account (name, address)
    VALUES (account_insert.name, account_insert.address);
$$;

alter procedure ethereum.account_insert(varchar, char) owner to markr;


create or replace function ethereum.get_meaning_of_life_universe_and_everything()
  returns int
  language sql
AS
$$
SELECT 42;
$$;

alter procedure ethereum.get_meaning_of_life_universe_and_everything() owner to markr;
