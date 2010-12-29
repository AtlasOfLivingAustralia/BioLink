﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class RDEMaterial : BioLinkDataObject {
        public int SiteVisitID { get; set; }	
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public int BiotaID { get; set; }	
        public string TaxaDesc { get; set; }
        public string AccessionNo { get; set; }
        public string RegNo { get; set; }
        public string CollectorNo { get; set; }
        public DateTime? IDDate { get; set; }
        public string Institution { get; set; }
        public string ClassifiedBy { get; set; }
        public string MicroHabitat { get; set; }
        public string MacroHabitat { get; set; }
        public int? TrapID { get; set; }
        public string TrapName { get; set; }
        public string MaterialSource { get; set; }
        public string CollectionMethod { get; set; }
        public int? Changes	{ get; set; }
        public bool Locked { get; set; }
        public int TemplateID { get; set; }
    }
}
