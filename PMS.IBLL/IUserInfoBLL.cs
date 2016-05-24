﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMS.Model;
using System.Linq.Expressions;

namespace PMS.IBLL
{
    public partial interface IUserInfoBLL
    {
        /// <summary>
        /// 根据UserID查找该用户对应的短信任务
        /// </summary>
        /// <param name="uid">UserInfo ID</param>
        /// <param name="isMiddle">是否转成中间变量（转成中间变量为true）</param>
        /// <returns></returns>
        List<S_SMSMission> GetMissionListByUID(int uid, bool isMiddle);

        /// <summary>
        /// 根据传入的用户id获取该用户的常用群组，并从常用群组中删除传入的群组id集合
        /// </summary>
        /// <param name="list_ids_group">已经拥有的群组id（需要从常用群组中剔除的群组id集合）</param>
        /// <param name="uid">用户id</param>
        /// <param name="isMiddel">是否转成中间对象</param>
        /// <returns></returns>
        List<P_Group> GetRestGroupListByIds(List<int> list_ids_group, int uid, bool isMiddel);

        /// <summary>
        /// 根据用户Id查询该用户所拥有的群组集合
        /// </summary>
        /// <param name="uid">用户Id</param>
        /// <param name="isMiddle">是否中间转换</param>
        /// <returns></returns>
        List<P_Group> GetGroupListByUID(int uid, bool isMiddle);



        /// <summary>
        /// 根据传入的用户id查询该用户所发送的短信
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="rowCount"></param>
        /// <param name="uid"></param>
        /// <param name="isAsc"></param>
        /// <param name="isMiddle"></param>
        /// <returns></returns>
        List<S_SMSContent> GetSMSContentListByUID(int pageIndex, int pageSize, ref int rowCount, int uid, bool isAsc, bool isMiddle);

        /// <summary>
        /// 逻辑删除（物理删除）
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool DeleteLogicUserInfos(List<int> list);

        /// <summary>
        /// 软删除
        /// </summary>
        /// <param name="list_ids"></param>
        /// <returns></returns>
        bool DelSoftUserInfos(List<int> list_ids);

        /// <summary>
        /// 为指定User重新赋予新的群组——指定用户的常用群组（删除不在此任务id集合中的现有群组关系）
        /// </summary>
        /// <param name="userID">用户id</param>
        /// <param name="list_groupIDs">现拥有的群组id集合</param>
        /// <returns></returns>
        bool SetUser4Group(int userID, List<int> list_groupIDs);

        /// <summary>
        /// 为指定User重新赋予新的任务（删除不在此任务id集合中的现有任务关系）
        /// </summary>
        /// <param name="userID">用户id</param>
        /// <param name="list_missionIDs">拥有的任务id集合</param>
        /// <returns></returns>
        bool SetUser4Mission(int userID, List<int> list_missionIDs);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        /// <returns></returns>
        bool SetUser4Role(int userID, List<int> list_roleIDs);
        /// <summary>
        /// 为用户分配权限
        /// </summary>
        /// <returns></returns>
        bool SetUser4Action(int userID, List<int> list_actionIDs);
    }
}
