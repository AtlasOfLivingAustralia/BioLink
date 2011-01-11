﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace BioLink.Data.Model {

    [AttributeUsage(AttributeTargets.Property)]
    public class MappingInfo : System.Attribute {

        public MappingInfo(string column) {
            Column = column;
        }

        public string Column { get; set; }
    }

    // Place holder class representing all Biolink Transfer Objects (TO)    
    [Serializable()]
    public abstract class BioLinkDataObject  {

        protected BioLinkDataObject() {            
        }

        protected abstract Expression<Func<int>> IdentityExpression { get; }

        public int? ObjectID {
            get {
                if (IdentityExpression == null) {
                    return null;
                } else {
                    var destProp = (PropertyInfo)((MemberExpression)IdentityExpression.Body).Member;
                    return (int)destProp.GetValue(this, null);
                }
            }
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            throw new NotImplementedException();
        }
    }



    // Biolink data objects that have a GUID column
    [Serializable()]
    public abstract class GUIDObject : BioLinkDataObject {      

        public Nullable<Guid> GUID { get; set; }
    }

    // Biolink data objects that have ownership columns
    [Serializable()]
    public abstract class OwnedDataObject : GUIDObject {

        public DateTime DateCreated { get; set; }
        public string WhoCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        public string WhoLastUpdated { get; set; }

    }

}
