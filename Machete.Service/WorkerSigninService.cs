﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Domain;
using Machete.Data;
using Machete.Data.Infrastructure;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Data.Objects;

namespace Machete.Service
{
    public interface IWorkerSigninService : ISigninService<WorkerSignin>
    {
        WorkerSignin GetSignin(int dwccardnum, DateTime date);
        void CreateSignin(WorkerSignin workerSignin, string user);
        int GetNextLotterySequence(DateTime date);
        bool clearLottery(int id, string user);
        bool sequenceLottery(DateTime date, string user);
    }

    public class WorkerSigninService : SigninServiceBase<WorkerSignin>, IWorkerSigninService
    {
        //
        //
        public WorkerSigninService(IWorkerSigninRepository repo, 
                                   IWorkerRepository wRepo,
                                   IPersonRepository pRepo,
                                   IImageRepository iRepo,
                                   IWorkerRequestRepository wrRepo,
                                   IUnitOfWork uow)
            : base(repo, wRepo, pRepo, iRepo, wrRepo, uow)
        {
            this.logPrefix = "WorkerSignin";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwccardnum"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public WorkerSignin GetSignin(int dwccardnum, DateTime date)
        {
            return repo.GetManyQ().FirstOrDefault(r => r.dwccardnum == dwccardnum &&
                            EntityFunctions.DiffDays(r.dateforsignin, date) == 0 ? true : false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public ServiceIndexView<WorkerSigninView> GetIndexView(dispatchViewOptions o)
        {
            IQueryable<WorkerSignin> queryable = repo.GetAllQ();
            //
            // WHERE on dateforsignin
            if (o.date != null)
                queryable = queryable.Where(p => EntityFunctions.DiffDays(p.dateforsignin, o.date) == 0 ? true : false);            
            // 
            // WHERE on wa_grouping
            switch (o.wa_grouping)
            {
                case "open": queryable = queryable.Where(p => p.WorkAssignmentID == null); break;
                case "assigned": queryable = queryable.Where(p => p.WorkAssignmentID != null); break;
                case "skilled": queryable = queryable
                                     .Where(wsi => wsi.WorkAssignmentID == null &&
                                         wsi.worker.skill1 != null ||
                                         wsi.worker.skill2 != null ||
                                         wsi.worker.skill3 != null
                    );

                    break;
                case "requested":
                    if (o.date == null) throw new MacheteIntegrityException("Date cannot be null for Requested filter");
                    queryable = queryable.Where(p => p.WorkAssignmentID == null);
                    queryable = queryable.Join(wrRepo.GetAllQ(),
                                                wsi => new { K1 = (int)wsi.WorkerID, K2 = (DateTime)EntityFunctions.TruncateTime(wsi.dateforsignin) },
                                                wr => new { K1 = wr.WorkerID, K2 = (DateTime)EntityFunctions.TruncateTime(wr.workOrder.dateTimeofWork) },
                                                (wsi, wr) => wsi);
                    break;
            }
            // 
            // typeofwork ( DWC / HHH )
            if (o.typeofwork_grouping == Worker.iDWC) //TODO: Refactor GetIndexView typework -- Should check for non-null, then group on value
                queryable = queryable
                                         .Where(wsi => wsi.worker.typeOfWorkID == Worker.iDWC)
                                         .Select(wsi => wsi);

            if (o.typeofwork_grouping == Worker.iHHH) //TODO: Refactor GetIndexView typework -- Should check for non-null, then group on value
                queryable = queryable
                                         .Where(wsi => wsi.worker.typeOfWorkID == Worker.iHHH)
                                         .Select(wsi => wsi);
            return base.GetIndexView(o, queryable);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signin"></param>
        /// <param name="user"></param>
        public void CreateSignin(WorkerSignin signin, string user)
        {
            //Search for worker with matching card number
            Worker wfound;
            wfound = wRepo.GetAllQ().FirstOrDefault(s => s.dwccardnum == signin.dwccardnum);
            if (wfound != null)
            {
                signin.WorkerID = wfound.ID;
            }
            //Search for duplicate signin for the same day
            int sfound = 0; ;
            sfound = repo.GetAllQ().Count(s => s.dateforsignin == signin.dateforsignin &&
                                                     s.dwccardnum == signin.dwccardnum);
            if (sfound == 0) Create(signin, user);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public int GetNextLotterySequence(DateTime date)
        {
            return repo.GetAllQ().Where(p => p.lottery_timestamp != null && 
                                        EntityFunctions.DiffDays(p.dateforsignin, date) == 0 ? true : false)
                                 .Count() + 1;
        }

        public bool clearLottery(int id, string user)
        {
            WorkerSignin wsi = repo.GetById(id);
            DateTime date = wsi.dateforsignin;
            wsi.lottery_sequence = null;
            wsi.lottery_timestamp = null;
            Save(wsi, user);
            if (date != null)
                sequenceLottery(date, user);
            return true;
        }

        public bool sequenceLottery(DateTime date, string user)
        {
            IEnumerable<WorkerSignin> signins;
            int i = 0;
            // select and sort by timestamp
            signins = repo.GetManyQ().Where(p => p.lottery_timestamp != null &&
                                           EntityFunctions.DiffDays(p.dateforsignin, date) == 0 ? true : false)
                                    .OrderBy(p => p.lottery_timestamp)
                                    .AsEnumerable();
            // reset sequence number
            foreach (WorkerSignin wsi in signins)
            {
                i++;
                wsi.lottery_sequence = i;                
            }
            // no logging as of now
            uow.Commit();
            return true;
        }
    }
}
