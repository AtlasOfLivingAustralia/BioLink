﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class AssociateViewModel : GenericViewModelBase<Associate> {

        public AssociateViewModel(Associate model)
            : base(model) {
            DataChanged += new DataChangedHandler(AssociateViewModel_DataChanged);
        }

        void AssociateViewModel_DataChanged(ChangeableModelBase viewmodel) {
            RaisePropertyChanged("DisplayLabel");
        }

        public override string DisplayLabel {
            get {
                return String.Format("{0} ({1})", AssocName, this.RelationFromTo);
            }
            set {
                base.DisplayLabel = value;
            }
        }

        public int AssociateID {
            get { return Model.AssociateID; }
            set { SetProperty(() => Model.AssociateID, value); }
        }

        public int FromIntraCatID {
            get { return Model.FromIntraCatID; }
            set { SetProperty(() => Model.FromIntraCatID, value); } 
        }

        public int FromCatID {
            get { return Model.FromCatID; }
            set { SetProperty(() => Model.FromCatID, value); }
        }


        public int? ToIntraCatID {
            get { return Model.ToIntraCatID; }
            set { SetProperty(() => Model.ToIntraCatID, value); }
        }

        public int? ToCatID {
            get { return Model.ToCatID; }
            set { SetProperty(() => Model.ToCatID, value); }
        }

        public int? RelativeCatID {
            get {
                if (Direction == "FromTo") {
                    return ToCatID;
                } else {
                    return FromCatID;
                }
            }

            set {
                if (Direction == "FromTo") {
                    ToCatID = value;
                } else {
                    FromCatID = (value.HasValue ? value.Value : 0);
                }
            }
        }

        public int? RelativeIntraCatID {
            get {
                if (Direction == "FromTo") {
                    return ToIntraCatID;
                } else {
                    return FromIntraCatID;
                }
            }

            set {
                if (Direction == "FromTo") {
                    ToIntraCatID = value;
                } else {
                    FromIntraCatID = (value.HasValue ? value.Value : 0);
                }
            }
        }

        public string AssocDescription {
            get { return Model.AssocDescription; }
            set { SetProperty(() => Model.AssocDescription, value); }
        }

        public string RelationFromTo {
            get { return Model.RelationFromTo; }
            set { SetProperty(() => Model.RelationFromTo, value); }
        }

        public string RelationToFrom {
            get { return Model.RelationToFrom; }
            set { SetProperty(() => Model.RelationToFrom, value); }
        }

        public string RelativeRelationToFrom {
            get {
                if (Direction == "FromTo") {
                    return RelationToFrom;
                } else {
                    return RelationFromTo;
                }
            }

            set {
                if (Direction == "FromTo") {
                    RelationToFrom = value;
                } else {
                    RelationFromTo = value;
                }
            }
        }

        public string RelativeRelationFromTo {
            get {
                if (Direction == "FromTo") {
                    return RelationFromTo;
                } else {
                    return RelationToFrom;
                }
            }

            set {
                if (Direction == "FromTo") {
                    RelationFromTo = value;
                } else {
                    RelationToFrom = value;
                }
            }
        }

        public int PoliticalRegionID {
            get { return Model.PoliticalRegionID; }
            set { SetProperty(() => Model.PoliticalRegionID, value); }
        }

        public string Source {
            get { return Model.Source; }
            set { SetProperty(() => Model.Source, value); }
        }

        public int RefID { 
            get { return Model.RefID; }
            set { SetProperty(()=>Model.RefID, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public bool Uncertain {
            get { return Model.Uncertain; }
            set { SetProperty(() => Model.Uncertain, value); }
        }

        public string Notes {
            get { return Model.Notes; }
            set { SetProperty(() => Model.Notes, value); }
        }

        public string AssocName {
            get { return Model.AssocName; }
            set { SetProperty(() => Model.AssocName, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string PoliticalRegion {
            get { return Model.PoliticalRegion; }
            set { SetProperty(() => Model.PoliticalRegion, value); }
        }

        public string Direction {
            get { return Model.Direction; }
            set { SetProperty(() => Model.Direction, value); }
        }

        public string FromCategory {
            get { return Model.FromCategory; }
            set { SetProperty(() => Model.FromCategory, value); }
        }

        public string ToCategory {
            get { return ToCategory; }
            set { SetProperty(() => Model.ToCategory, value); }
        }

        public Guid AssocGUID {
            get { return AssocGUID; }
            set { SetProperty(() => Model.AssocGUID, value); }
        }

        public Guid RegionGUID {
            get { return Model.RegionGUID; }
            set { SetProperty(() => Model.RegionGUID, value); }
        }

    }
}
