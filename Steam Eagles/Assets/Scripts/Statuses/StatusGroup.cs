using System;

namespace Statuses
{
    /// <summary>
    /// the purpose of this class is to enable a group of statuses to function as a single status for the purpose
    /// of blocking or being required by other statuses.
    ///
    /// <para>
    /// for example if we have the statuses equip_destruct_tool, equip_repair_tool, equip_recipe_tool, equip_build_tool.
    /// now we want to make a requirement status: in_engineering_room that is required by all these statuses.  We could
    /// make a status group called equip_tool, which cannot be added directly, but will be added implicitly if any one of the other
    /// statuses is added. The requirement/blocking logic will now operate on the status group and effect all the statuses defined
    /// in the group. 
    /// </para>
    /// </summary>
    [Serializable]
    public class StatusGroup
    {
        public string groupName = "Group";
        public string[] statuses;

        public StatusGroup(string groupName, params string[] statuses)
        {
            this.groupName = groupName;
            this.statuses = statuses;
        }
    }
}