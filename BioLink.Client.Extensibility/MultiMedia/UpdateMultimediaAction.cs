﻿using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {

    public class UpdateMultimediaAction : GenericDatabaseAction<MultimediaViewModel> {

        public UpdateMultimediaAction(MultimediaViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimedia(Model.MultimediaID, Model.Name, Model.Number, Model.Artist, Model.DateRecorded, Model.Owner, Model.Copyright);
        }
    }

    public class DeleteMultimediaLinkAction : GenericDatabaseAction<MultimediaLink> {

        public DeleteMultimediaLinkAction(MultimediaLink model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteMultimediaLink(Model.MultimediaLinkID);
        }

    }

    public class InsertMultimediaAction : GenericDatabaseAction<MultimediaLink> {

        public InsertMultimediaAction(MultimediaLink model, string filename)
            : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            int newId = service.InsertMultimedia(Model.Name, Model.Extension, bytes);
            Model.MultimediaID = newId;
        }

        public string Filename { get; private set; }

    }

    public class InsertMultimediaLinkAction : GenericDatabaseAction<MultimediaLink> {

        public InsertMultimediaLinkAction(MultimediaLink model, TraitCategoryType category, int intraCatId)
            : base(model) {
            this.Category = category;
            this.IntraCategoryID = intraCatId;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            BioLink.Client.Utilities.Debug.Assert(Model.MultimediaID >= 0, "Not a valid multimedia ID!");

            var newId = service.InsertMultimediaLink(Category.ToString(), IntraCategoryID, Model.MultimediaType, Model.MultimediaID, Model.Caption);
            Model.MultimediaLinkID = newId;
        }

        public TraitCategoryType Category { get; private set; }

        public int IntraCategoryID { get; private set; }
    }

    public class UpdateMultimediaLinkAction : GenericDatabaseAction<MultimediaLink> {

        public UpdateMultimediaLinkAction(MultimediaLink model, TraitCategoryType category)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateMultimediaLink(Model.MultimediaLinkID, TraitCategory.ToString() , Model.MultimediaType, Model.Caption);
        }

        public TraitCategoryType TraitCategory { get; private set; }
    }

    public class UpdateMultimediaBytesAction : GenericDatabaseAction<MultimediaLink> {

        public UpdateMultimediaBytesAction(MultimediaLink model, string filename)
            : base(model) {
            this.Filename = filename;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var bytes = SystemUtils.GetBytesFromFile(Filename);
            service.UpdateMultimediaBytes(Model.MultimediaID, bytes);
        }

        public string Filename { get; private set; }
    }

}
