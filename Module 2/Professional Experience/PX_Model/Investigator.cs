//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PX_Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Investigator
    {
        public Investigator()
        {
            this.Investigator_Intervention_Area = new HashSet<Investigator_Intervention_Area>();
            this.Trial_Investigator = new HashSet<Trial_Investigator>();
        }
    
        public int Id { get; set; }
        public Nullable<int> Person_Id { get; set; }
        public string Institution { get; set; }
    
        public virtual ICollection<Investigator_Intervention_Area> Investigator_Intervention_Area { get; set; }
        public virtual Person Person { get; set; }
        public virtual ICollection<Trial_Investigator> Trial_Investigator { get; set; }
    }
}
