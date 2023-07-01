using System;
using System.Collections.Generic;
using CoreLib.Entities;
using UniRx;
using UnityEngine;

namespace Statuses
{
    /// <summary>
    /// stores all the statuses in the game, as well as the set of blocking status and required status for each
    ///
    /// <para>
    /// statuses are registered by name and then can be retrieved by name, once added to the database they can be
    /// retrieved by name but not removed.  This is because this database is a record of all statuses that have
    /// ever been created in the game, and should not be changed once created.  However, as new statuses are created
    /// they can be added to the database.
    ///
    /// A status should not need to know about every statuses that blocks it or is required by it.  However once a status
    /// is registered as a blocking status this will create a link between the two statuses.  This means that if there are entities
    /// that rely on the blocked status, they will need to be updated to consider whether the blocking status is present.
    ///
    /// Ideally the database should be created at the start of the game and then never changed.  However, if a new status is
    /// created at runtime (say by a mod) then it can be added to the database. 
    /// </para> 
    /// </summary>
    public class StatusDatabase
    {
        public void RegisterStatus(StatusRegistration newStatus)
        {
            RegisterStatus(newStatus.Status);
            foreach (var newBlockingStatus in newStatus.GetDefinedBlockingStatuses())
            {
                RegisterStatus(newBlockingStatus);
                RegisterBlockingStatus(newBlockingStatus, newStatus.Status);
            }

            foreach (var newRequiredStatus in newStatus.GetDefinedRequiredStatuses())
            {
                RegisterStatus(newRequiredStatus);
                RegisterStatusRequirement(newRequiredStatus, newStatus.Status);
            }
        }

        public StatusHandle[] RegisterStatusGroup(StatusGroup group)
        {
            throw new NotImplementedException();
        }

        public StatusHandle[] RegisterStatusGroup(string groupName, params string[] statuses)
        {
            Debug.Assert(statuses.Length >= 2, "Group must have at least 2 statuses and a name");
            var group = new StatusGroup(groupName, statuses);
            return RegisterStatusGroup(group);
        }

        public StatusHandle RegisterStatus(string newStatus)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered(string status)
        {
            return false;
        }

        public IEnumerable<string> WhatStatusesIsThisStatusBlockedBy(string status)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> WhatStatusesDoesThisStatusRequire(string status)
        {
            throw new NotImplementedException();
        }

        public void RegisterBlockingStatus(string blockingStatus, string blockedStatus)
        {
            throw new NotImplementedException();
        }

        public void RegisterStatusRequirement(string requiredStatus, string requiredByStatus, bool isTwoWay = false)
        {
            if (isTwoWay)
                RegisterStatusRequirement(requiredByStatus, requiredStatus, false);
            throw new NotImplementedException();
        }


        public void SpecifyTargetMask(string status, EntityType entityType)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<string> GetStatusesInGroup(string statusGrouping)
        {
            throw new NotImplementedException();
        }

        
        public EntityType GetTargetMask(string statusName)
        {
            throw new NotImplementedException();
        }
        
        
        /// <summary>
        /// this means that only one status in the group can be applied at a time, adding one will remove the others.
        ///
        /// this would be useful for equip tool statuses, where we only need to equip the tool we want now, we don't need
        /// to know which tool was equipped before in order to remove it. 
        /// </summary>
        /// <param name="group"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void MakeGroupedStatusesMutuallyExclusive(string group)
        {
            throw new NotImplementedException();
        }
    }
}