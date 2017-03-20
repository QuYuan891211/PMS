﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMS.Model.ViewModel;
using PMS.IBLL;
using Common.EasyUIFormat;
using PMS.Model.SMSModel;
using ISMS;

namespace SMSOA.Areas.SMS.Controllers
{
    public class MMSSendController : SendController
    {
        IP_PersonInfoBLL personBLL { get; set; }
        IP_GroupBLL groupBLL { get; set; }
        IS_SMSMissionBLL smsMissionBLL { get; set; }
        IUserInfoBLL userBLL { get; set; }
        IMMSSend mmsSendBLL { get; set; }
        public override ViewModel_MyHttpContext GetHttpContext()
        {
            throw new NotImplementedException();
        }

        // GET: SMS/MMSSend
        public ActionResult Index()
        {
            //定义Razor标签
            ViewBag.GetAllMission_combogrid = "/SMS/Send/GetAllMissionByPID";
            ViewBag.GetMissionByUID = "/SMS/Send/GetMissionByUID";
            ViewBag.GetGroupByMID_combogrid = "/SMS/MMSSend/GetGroupByMID";
            ViewBag.GetDepartment_combotree = "/SMS/Send/GetDepartmentInfo4ComboTree";
            ViewBag.GetPersonByMission = "/SMS/Send/GetPersonByMission";
            ViewBag.GetPersonByGroupDepartment = "/SMS/Send/GetPersonByGroupDepartment";
            ViewBag.DoSend = "/SMS/Send/DoSend";
            ViewBag.GetTemplateByUidAndMission = "/SMS/MsgTemplate/GetTemplateByUserIdAndMission";
            //注意不在此处传获取任务的方法
            ViewBag.ShowSetOftenMissionAndGroup = "/SMS/Send/ShowSetWindow";
            ViewBag.LoginUser = -999;
            //若父控制器中的登录用户不为空
            if (base.LoginUser != null)
            {
                //获取登录用户的id
                ViewBag.LoginUserID = base.LoginUser.ID;
            }
            return View();
        }

        public ActionResult GetGroupByMID(int mid)
        {
            int userId = int.Parse(Request["uid"]);
           // int mid = int.Parse(Request["mid"]);
            var mission = smsMissionBLL.GetListBy(m => m.SMID == mid).FirstOrDefault();
            //1.根据任务ID获取彩信群组的ID集合
            var list_MMS_group = smsMissionBLL.GetMMSGroups(true, mission);
            list_MMS_group = list_MMS_group.Select(m => m.ToMiddleModel()).ToList();
            var list_MMS_group_ids = list_MMS_group.Select(m => m.GID).ToList();

            //1.1 转换为easyUI的格式
            var list = ToEasyUICombogrid_Group.ToEasyUIDataGrid(list_MMS_group, true);
            //var list = ToEasyUICombogrid_Group.ToEasyUIDataGrid(list_owned_group, false);
            //2 从所有的群组中删除该任务所拥有的群组集合
            //2.1 获取当前用户所拥有的常用群组(通过User查询对应的Group）
            var list_excludeOwned_group = userBLL.GetRestGroupListByIds(list_MMS_group_ids, userId, true);
            //var list_excludeOwned_group = groupBLL.GetListBy(g => g.isDel == false).ToList().Where(g => !list_owned_group.Contains(g)).Select(g=>g.ToMiddleModel()).ToList();
            list.AddRange(ToEasyUICombogrid_Group.ToEasyUIDataGrid(list_excludeOwned_group, false));
            //将该任务拥有的群组设置为选中状态
            PMS.Model.EasyUIModel.EasyUIDataGrid model = new PMS.Model.EasyUIModel.EasyUIDataGrid()
            {
                total = 0,
                rows = list,
                footer = null
            };
            var temp = Common.SerializerHelper.SerializerToString(model);
            temp = temp.Replace("Checked", "checked");
            return Content(temp);
        }
        public ActionResult DoSend(PMS.Model.ViewModel.ViewModel_MMSMessage model)
        {
            //1 有效性判断
            //1.1 联系人名单为空，不执行发送操作，返回
            //if (model.PersonIds == null||model.PersonIds== "undefined") { return Content("empty contact list"); }
            //1.2 短信内容为空，不执行发送操作，返回
            if(model.Content == null) { return Content("empty content"); }
            if (model.Content.Length >= 300) { return Content("out of range"); }
            if(model.MMSTitle.Length >= 15) { return Content("title out of range"); }
            HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
            if (files.Count <= 0) { return Content("empty file"); }
            
            //2 图片处理
            HttpPostedFile file = files[0];
            var picture_stream = file.InputStream;
            string fileDirectory = System.Web.HttpContext.Current.Server.MapPath("~/FileUpLoad/");
            //2.1最终在项目目录下创建Zip包,并获取路径
            var path = mmsSendBLL.CreateZip(picture_stream, fileDirectory);
            //2.2 将路径封装进实体模型
            model.ZipUrl = path;

            PMS.Model.CombineModel.SendAndMessage_Model combine_model = new PMS.Model.CombineModel.SendAndMessage_Model();
            SMSModel_Receive receive = new SMSModel_Receive();
            combine_model.Model_Message = model;
            //3 执行发送操作
            var isOk_Send = DoSendNow(combine_model, out receive);
            if ("0".Equals(receive.result) && isOk_Send)
                {
                    //6 查询发送状态(是否加入等待时间？)
                    return Content("ok");

                }
                if (!isOk_Send)
                {
                    return Content("send_error");
                }
                else
                {
                    return Content("error");
                }
            }
        /// <summary>
        /// 发送彩信方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        private bool DoSendNow(PMS.Model.CombineModel.SendAndMessage_Model model, out SMSModel_Receive receive)
        {
            //重新梳理并做抽象
            #region 暂时注释掉
            ///*步骤一：
            //    获取传入的群组及部门获取对应联系人
            //    获取要删除的联系人id
            //    从联系人集合中去除要删除的联系人获得最终要发送的联系人
            //*/
            ////1.1 获取要去除的 联系人id 数组
            //var ids = model.PersonId_Int;    
            //int count = 0;
            //string dids_str = null;
            //string gids_str = null;
            //if (model.GroupIds == null)
            //{
            //    gids_str = "";
            //}

            //if (model.DepartmentIds == null)
            //{
            //    dids_str = "";
            //}

            //if (model.GroupIds != null)
            //{
            //    foreach (var item in model.GroupIds)
            //    {
            //        gids_str += item.ToString() + ",";
            //    }
            //}

            //if (model.DepartmentIds != null)
            //{
            //    foreach (var item in model.DepartmentIds)
            //    {
            //        dids_str += item.ToString() + ",";
            //    }
            //}

            ////1.3 根据传入的群组及部门id获取对应的联系人
            //var list_person = GetPersonListByGroupDepartment(dids_str, gids_str, out count);

            ////2.1 去除不需要的联系人，获得最终联系人集合
            //list_person = (from p in list_person
            //               where !ids.Contains(p.PID)
            //               select p).ToList();

            ////2.2 获取联系人集合中的电话生成电话集合
            //List<string> list_phones = new List<string>();
            //list_person.ForEach(p => list_phones.Add(p.PhoneNum.ToString()));

            ///*步骤二：
            //        获取添加的临时联系人
            //        向数据库中写入这些临时联系人
            //*/
            ////1.2 获取临时联系人的电话数组
            //var phoneNums = model.PhoneNum_Str;
            ////1.3 调用personBLL中的添加联系人方法，将临时联系人写入数据库（qu）
            //string PName_Temp = "临时联系人";

            ////1.4 目前默认只添加到全部联系人群组中
            //int groupID_AllContacts = groupBLL.GetListBy(a => a.GroupName.Equals("全部联系人")).FirstOrDefault().GID;

            //List<int> groupIds = new List<int>();
            //groupIds.Add(groupID_AllContacts);
            ////1.5 循环写入数据库
            //bool isSaveTempPersonOk = false;
            //if (phoneNums != null && phoneNums.Length != 0)
            //{
            //    foreach (var item in phoneNums)
            //    {
            //        //1.6 判断输入的联系人在是否存在在数据库中

            //        if (!personBLL.AddValidation(item))
            //        {
            //            //1.7 不存在在数据库中，则将临时联系人添加进数据库
            //            isSaveTempPersonOk = personBLL.DoAddTempPerson(PName_Temp, item, true, groupIds);
            //            if (!isSaveTempPersonOk)
            //            {
            //                return false;
            //            }
            //        }
            //        //1.7 存在在数据库中，且已经在发送列表中，这种情况需讨论

            //    }

            //    list_phones.AddRange(phoneNums.ToList());
            //}

            ///*步骤三                    
            //        获取短信内容
            //        封装要提交至联通接口的发送对象
            //        （含联系人电话号码）
            //*/
            ////2 获取短信内容
            //var content = model.Content;


            ////2.1 设置发送对象相关参数
            //string subCode = "";//短信子码"74431"，接收回馈信息用
            //string sign = "【国家海洋预报台】"; //短信签名，！仅在！发送短信时用= "【国家海洋预报台】";
            //                           //短信发送与查询所需参数
            //string smsContent = content;//短信内容
            //string sendTime;//计划发送时间，为空则立即发送
            //                //3 对短信内容进行校验——先暂时不做

            ////6月27日新增将List电话集合转成用,拼接的字符串
            ////查询时不需要联系人电话
            //SMSModel_Send sendMsg = new SMSModel_Send()
            //{
            //    account = "dh74381",
            //    password = "uAvb3Qey",
            //    content = content,
            //    phones = list_phones.ToArray(),
            //    sendtime = DateTime.Now,

            //};
            #endregion
            //1 根据选定的群组及部门获取相应的联系人
            var list_PersonPhonesByGroupAndDepartment = mmsSendBLL.GetFinalPersonPhoneList(model.Model_Message, GetPersonListByGroupDepartment);

            //2 获取临时联系人电话集合
            var list_tempPersonPhones = mmsSendBLL.AddAndGetTempPersons(model.Model_Message, personBLL, groupBLL);

            //2.2 获取最终的联系人电话集合
            list_PersonPhonesByGroupAndDepartment.AddRange(list_tempPersonPhones);

            var list_phones = list_PersonPhonesByGroupAndDepartment;

            //3 转成发送对象
            var sendMsg = mmsSendBLL.ToSendModel(model.Model_Message, list_phones);

            /*步骤四
                    生成提交对象及短信及作业对象
                    由SMSFactory进行短信提交操作（并选择延时/立刻发送）
            */
            //4 短信发送
            //注意：desc:定时时间格式错误;
            //      result:定时时间格式错误
            //PMS.Model.CombineModel.SendAndMessage_Model sendandMsgModel = new PMS.Model.CombineModel.SendAndMessage_Model() { Model_Message = model, Model_Send = sendMsg };
            model.Model_Send = sendMsg;
            //PMS.Model.Message.BaseResponse response = new PMS.Model.Message.BaseResponse();
            mmsSendBLL.SendMsg(model, out /*response*/receive);
            //receive = new SMSModel_Receive();
            
            return true;
        }






    }
}