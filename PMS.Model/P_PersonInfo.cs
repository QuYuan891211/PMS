//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace PMS.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class P_PersonInfo
    {
        public P_PersonInfo()
        {
            this.P_DepartmentInfo = new HashSet<P_DepartmentInfo>();
            this.P_Group = new HashSet<P_Group>();
        }
    
        public int PID { get; set; }
        public string PName { get; set; }
        public int PhoneNum { get; set; }
        public string Remark { get; set; }
        public bool isVIP { get; set; }
        public bool isDel { get; set; }
    
        public virtual ICollection<P_DepartmentInfo> P_DepartmentInfo { get; set; }
        public virtual ICollection<P_Group> P_Group { get; set; }
    }
}