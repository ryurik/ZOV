//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Pos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Pos()
        {
            this.posStatus_ID = 1;
            this.PosImages = new HashSet<PosImages>();
            this.Samples = new HashSet<Samples>();
            this.PosPhones = new HashSet<PosPhones>();
            this.PosEmails = new HashSet<PosEmails>();
            this.PosRanks = new HashSet<PosRanks>();
            this.Sites = new HashSet<Sites>();
            this.Contacts = new HashSet<Contacts>();
            this.Docs = new HashSet<Docs>();
        }
    
        public int ID { get; set; }
        public Nullable<System.DateTime> dateadd { get; set; }
        public string legalName { get; set; }
        public string fabricSynonim { get; set; }
        public string yandexAdress { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string houseNumber { get; set; }
        public string locationDescription { get; set; }
        public Nullable<int> posArea { get; set; }
        public string brand { get; set; }
        public Nullable<int> posSortOrder { get; set; }
        public Nullable<int> dealer_ID { get; set; }
        public Nullable<int> posType_ID { get; set; }
        public Nullable<int> Ruby_Id { get; set; }
        public Nullable<double> longitude { get; set; }
        public Nullable<double> attitude { get; set; }
        public string coordstextdata { get; set; }
        public string Country { get; set; }
        public Nullable<int> DealerLegalNames_ID { get; set; }
        public Nullable<bool> DeleteMark { get; set; }
        public int posStatus_ID { get; set; }
        public Nullable<System.DateTime> posStatusDate { get; set; }
        public string comment { get; set; }
        public string SiteImagePath { get; set; }
        public Nullable<bool> isOptima { get; set; }
    
        public virtual Dealers Dealers { get; set; }
        public virtual PosTypes PosTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PosImages> PosImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Samples> Samples { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PosPhones> PosPhones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PosEmails> PosEmails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PosRanks> PosRanks { get; set; }
        public virtual DealerLegalNames DealerLegalNames { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sites> Sites { get; set; }
        public virtual StatusOfPos StatusOfPos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contacts> Contacts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Docs> Docs { get; set; }
    }
}
