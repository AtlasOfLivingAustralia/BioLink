﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class RDESiteVisit : BioLinkDataObject {

        public int SiteVisitID { get; set; }
        public int SiteID { get; set; }
        public string VisitName { get; set; }
        public string Collector { get; set; }
        public int? DateStart { get; set; }
        public int? DateEnd { get; set; }
        public int? Changes { get; set; }
        public int? TemplateID { get; set; }
        public bool Locked { get; set; }

    }
}
