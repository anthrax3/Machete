declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByLimitedEnglish-' + min(limitedEnglish) as id,
limitedEnglish as [Limited English?], 
count(*) as [Count]
FROM (
  select W.ID, 
  CASE 
	WHEN W.englishlevelID < 3 then 'yes'
	when W.englishlevelID >= 3 then 'no'
	when W.englishlevelID is null then 'NULL'
  END as limitedEnglish
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.Englishlevelid
) as WW
group by limitedEnglish

union 
select 
convert(varchar(8), @startDate, 112) + '-' + convert(varchar(8), @endDate, 112) + '-WorkersByLimitedEnglish-TOTAL'  as id,
'Total' as [Limited English], 
count(*) as [Count]
from (
   select W.ID, min(dateforsignin) firstsignin
   from workers W
   JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
   WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
   group by W.ID
) as WWW