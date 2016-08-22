﻿using PMS.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Model
{
    public partial class S_SMSMission
    {
        /// <summary>
        /// 返回该对象的回收站对象
        /// </summary>
        /// <returns></returns>
        public ViewModel_Recycle_Common ToRecycleModel()
        {
            return new ViewModel_Recycle_Common()
            {
                Id = this.SMID,
                Name = this.SMSMissionName,
                Sort = this.Sort,
                SubDateTime = this.SubTime
            };
        }
    }
}
