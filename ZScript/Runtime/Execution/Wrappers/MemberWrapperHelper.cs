using System;
using System.Linq;
using ZScript.Elements;
using ZScript.Runtime.Execution.Wrappers.Callables;
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

        /// <summary>
        /// Returns an ICallableWrapper for a given target object's method.
        /// If the target object does not contains a matching method, an ArgumentException is raised
        /// </summary>
        /// <param name="target">The target to wrap</param>
        /// <param name="callableName">The name of the method to wrap</param>
        /// <returns>An ICallableWrapper created from the given target</returns>
        /// <exception cref="ArgumentException">No method with the given name found on target object</exception>
        public static ICallableWrapper CreateCallableWrapper(object target, string callableName)
        {
            // Native method
            var info = target.GetType().GetMethods().Where(m => m.Name == callableName).ToArray();

            if(info.Length == 0)
                throw new ArgumentException("No public method of name '" + callableName + "' found on object of type '" + target.GetType() + "'", "callableName");

            return new ClassMethod(target, info);
        }
    }
}