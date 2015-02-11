using System;

using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers.Members;

namespace ZScript.Runtime.Execution.Wrappers
{
    /// <summary>
    /// Helper class used during member wrapping
    /// </summary>
    public static class MemberWrapperHelper
    {
        /// <summary>
        /// Returns an IMemberWrapper for a given target object's member.
        /// If the target object does not contains a matching field and it is not a ZObject instance, an ArgumentException is raised
        /// </summary>
        /// <param name="target">The target to wrap</param>
        /// <param name="memberName">The name of the member to wrap</param>
        /// <returns>An IMemberWrapper created from the given target</returns>
        /// <exception cref="ArgumentException">No member with the given name found on target object</exception>
        public static IMemberWrapper CreateMemberWrapper(object target, string memberName)
        {
            var z = target as ZObject;
            if (z != null)
            {
                return new ZObjectMember(z, memberName);
            }

            return ClassMember.CreateMemberWrapper(target, memberName);
        }
    }
}