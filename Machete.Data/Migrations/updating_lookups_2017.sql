/****** Script for SelectTopNRows command from SSMS  ******/
SELECT *
  FROM [dbo].[Lookups]
  where category = 'skill'

update lookups set [key] = 'painting_rollerbrush' where id = 61
update lookups set [key] = 'painting_spray' where id = 62
update lookups set [key] = 'drywall' where id = 63
update lookups set [key] = 'painting_roller' where id = 64
update lookups set [key] = 'painting_roller' where id = 65
update lookups set [key] = 'painting_roller' where id = 66
update lookups set [key] = 'painting_roller' where id = 67
update lookups set [key] = 'painting_roller' where id = 68
update lookups set [key] = 'painting_roller' where id = 69


select * from configs
