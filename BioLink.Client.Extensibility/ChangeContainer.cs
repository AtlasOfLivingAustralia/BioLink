﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ChangeContainer : Window, IChangeContainer  {

        private List<DatabaseAction> _pendingChanges = new List<DatabaseAction>();
        private List<IChangeContainerObserver> _observers = new List<IChangeContainerObserver>();

        #region designer ctor
        public ChangeContainer() {
        }
        #endregion

        public ChangeContainer(User user) {
            this.User = user;
        }

        public bool HasPendingChanges {
            get { return _pendingChanges != null && _pendingChanges.Count > 0; }
        }

        public void RegisterPendingChange(DatabaseAction action, object contributer) {
            if (action != null) {
                _pendingChanges.Add(action);
                if (contributer is IChangeContainerObserver && !_observers.Contains(contributer)) {
                    _observers.Add(contributer as IChangeContainerObserver);
                }
                if (ChangeRegistered != null) {
                    ChangeRegistered(this, action);
                }
            }
        }

        public bool RegisterUniquePendingChange(DatabaseAction action, object contributer) {
            foreach (DatabaseAction existingaction in _pendingChanges) {
                if (existingaction.Equals(action)) {
                    return false;
                }
            }
            RegisterPendingChange(action, contributer);
            return true;
        }

        public void RegisterPendingChanges(List<DatabaseAction> actions, object contributer) {
            foreach (DatabaseAction action in actions) {
                RegisterPendingChange(action, contributer);
            }
        }

        public void ClearPendingChanges() {
            _pendingChanges.Clear();
        }

        public void ClearMatchingPendingChanges(Predicate<DatabaseAction> predicate) {
            var purgeList = new List<DatabaseAction>();
            // Build a list of the database actions that need to be removed...
            _pendingChanges.ForEach(action => {
                if (predicate(action)) {
                    purgeList.Add(action);
                }
            });

            // and remove them
            purgeList.ForEach(action => {
                _pendingChanges.Remove(action);
            });
        }


        public void CommitPendingChanges(Action successAction = null) {

            if (User == null) {
                throw new Exception("User object has not been set on the Control Host Window");
            }
#if DEBUG
            Logger.Debug("About to commit the following changes:");
            foreach (DatabaseAction action in _pendingChanges) {
                Logger.Debug("{0}", action);
            }
#endif

            // It may be that this control is aggregated as part of a larger control. This means that, come save time, there
            // may already be a transaction pending. If so, don'note create a new one, just piggy back on the existing
            bool commitTrans = false;  // flag to let us know if we are responsible for the transaction...

            if (!User.InTransaction) {
                User.BeginTransaction();
                commitTrans = true;
            }
            try {
                foreach (DatabaseAction action in _pendingChanges) {
                    action.Process(User);
                }

                if (commitTrans) {
                    User.CommitTransaction();
                }

                if (successAction != null) {
                    successAction();
                }

                foreach (IChangeContainerObserver observer in _observers) {
                    observer.OnChangesCommitted();
                }

                if (ChangesCommitted != null) {
                    ChangesCommitted(this);
                }

                _observers.Clear();
                _pendingChanges.Clear();
                
                // Reload the pinboard...
                JobExecutor.QueueJob(() => {
                    PluginManager.Instance.RefreshPinBoard();
                });

            } catch (Exception ex) {
                if (commitTrans) {
                    User.RollbackTransaction();
                }
                GlobalExceptionHandler.Handle(ex);
            }
        }

        public List<DatabaseAction> PendingChanges {
            get { return _pendingChanges; }
        }

        public User User { get; protected set; }

        public event PendingChangesCommittedHandler ChangesCommitted;

        public event PendingChangedRegisteredHandler ChangeRegistered;

    }
}
