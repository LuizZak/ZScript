#region License information
/*
    ZScript Game Scripting Programming Language
    Copyright (C) 2015  Luiz Fernando Silva

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/
#endregion

using System.Collections.Generic;
using System.Linq;

using ZScript.CodeGeneration.Definitions;
using ZScript.Runtime.Typing;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Analysis
{
    /// <summary>
    /// Custom type source that can feed types based on generic types defined within generic contexts
    /// </summary>
    public class GenericTypeSource : ICustomTypeSource
    {
        /// <summary>
        /// The current generic signature context stack
        /// </summary>
        private readonly Stack<GenericSignatureInformation> _genericContextStack = new Stack<GenericSignatureInformation>();

        /// <summary>
        /// Pushes a generic context on this generic type source
        /// </summary>
        /// <param name="context">The context to push on this generic type source</param>
        public void PushGenericContext(GenericSignatureInformation context)
        {
            _genericContextStack.Push(context);
        }

        /// <summary>
        /// Pops a generic context from this generic type source
        /// </summary>
        public void PopContext()
        {
            _genericContextStack.Pop();
        }

        /// <summary>
        /// Clears this generic type source of all of its current contexts
        /// </summary>
        public void Clear()
        {
            _genericContextStack.Clear();
        }

#pragma warning disable CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente

        // 
        // ICustomTypeSource.HasType implementation
        // 
        public bool HasType(string typeName)
        {
            foreach (var signature in _genericContextStack)
            {
                var type = signature.GenericTypes.FirstOrDefault(g => g.Name == typeName);
                if (type != null)
                    return true;
            }

            return false;
        }

        // 
        // ICustomTypeSource.TypeNamed implementation
        // 
        public TypeDef TypeNamed(string typeName)
        {
            foreach (var signature in _genericContextStack)
            {
                var type = signature.GenericTypes.FirstOrDefault(g => g.Name == typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

#pragma warning restore CS1591 // O comentário XML ausente não foi encontrado para o tipo ou membro visível publicamente
    }
}