using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Entities;

namespace Statuses
{
    public struct StatusHandle
    {
        private StatusDatabase _db;

        public string StatusName { get; }

        public bool IsStatusGroup { get; }
        internal StatusHandle(string status, StatusDatabase _db, bool isGroup = false)
        {
            this._db = _db;
            StatusName = status;
            IsStatusGroup = isGroup;
        }
        
        public IEnumerable<string> BlockingStatuses => _db.WhatStatusesIsThisStatusBlockedBy(StatusName);
        
        public IEnumerable<string> RequiredStatuses => _db.WhatStatusesDoesThisStatusRequire(StatusName);
        
        public bool IsBlockedBy(string status) => BlockingStatuses.Contains(status);
        
        public bool Requires(string status) => RequiredStatuses.Contains(status);
        
        public void AddRequirement(string requirement)
        {
            _db.RegisterStatusRequirement(requirement, StatusName);
            if (IsStatusGroup)
            {
                foreach (var status in _db.GetStatusesInGroup(StatusName))
                {
                    _db.RegisterStatusRequirement(requirement, status);
                }
            }
        }
        
        public void AddBlockingStatus(string blockingStatus)
        {
            this._db.RegisterBlockingStatus(blockingStatus, StatusName);
            if (IsStatusGroup)
            {
                foreach (var status in _db.GetStatusesInGroup(StatusName))
                {
                    _db.RegisterBlockingStatus(blockingStatus, status);
                }
            }
        }

        
        /// <summary>
        /// if this is a group handle, returns the statuses in the group otherwise returns the status name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetStatuses()
        {
            if (IsStatusGroup)
            {
                foreach (var status in _db.GetStatusesInGroup(StatusName))
                {
                    yield return status;
                }
            }
            else
            {
                yield return StatusName;
            }
        }

        public void SpecifyTarget(EntityType entityTarget)
        {
            _db.SpecifyTargetMask(StatusName, entityTarget);
            if (IsStatusGroup)
            {
                foreach (var status in _db.GetStatusesInGroup(StatusName))
                {
                    _db.SpecifyTargetMask(status, entityTarget);
                }
            }
        }

        public EntityType TargetMask => _db.GetTargetMask(StatusName);
    }
}