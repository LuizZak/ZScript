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

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Defines an object definition that can be instantiated and used in the script
    /// </summary>
    public class ClassDefinition : Definition
    {
        /// <summary>
        /// Gets or sets the context containing this class definition
        /// </summary>
        public ZScriptParser.ClassDefinitionContext ClassContext { get; set; }

        /// <summary>
        /// Gets the list of fields colelcted in this class definition
        /// </summary>
        public List<ValueHolderDefinition> Fields { get; private set; } 

        /// <summary>
        /// Gets the list of methods collected in this class definition
        /// </summary>
        public List<MethodDefinition> Methods { get; private set; }

        /// <summary>
        /// Gets or sets the public constructor for this class definitions
        /// </summary>
        public ConstructorDefinition PublicConstructor { get; set; }

        /// <summary>
        /// Gets or sets the base class for this class definition
        /// </summary>
        public ClassDefinition BaseClass { get; set; }

        /// <summary>
        /// Initializes a new instance of the ClassDefinition class
        /// </summary>
        /// <param name="className">The name for this class</param>
        public ClassDefinition(string className)
        {
            Name = className;

            Fields = new List<ValueHolderDefinition>();
            Methods = new List<MethodDefinition>();
        }

        /// <summary>
        /// Finishes this ClassDefinition, doing commont routines like creating public parameterless constructors when no constructor is provided, etc.
        /// </summary>
        public void FinishDefinition()
        {
            if (PublicConstructor == null)
            {
                PublicConstructor = new ConstructorDefinition(this, null, new FunctionArgumentDefinition[0]);
            }
        }

        /// <summary>
        /// Returns a list of all the methods inherited and defined by this class definition
        /// </summary>
        /// <param name="inheritedOnly">Whether to only get methods that where inherited</param>
        /// <returns>A list of all the methods inherited and defined by this given class definition</returns>
        public List<MethodDefinition> GetAllMethods(bool inheritedOnly = false)
        {
            var functions = new List<MethodDefinition>();

            var curClass = this;
            if (inheritedOnly)
                curClass = curClass.BaseClass;

            while (curClass != null)
            {
                functions.AddRange(curClass.Methods);

                curClass = curClass.BaseClass;
            }

            return functions;
        }

        /// <summary>
        /// Returns a list of all the fields inherited and defined by this class definition
        /// </summary>
        /// <param name="inheritedOnly">Whether to only get fields that where inherited</param>
        /// <returns>A list of all the fields inherited and defined by this given class definition</returns>
        public List<ValueHolderDefinition> GetAllFields(bool inheritedOnly = false)
        {
            var functions = new List<ValueHolderDefinition>();

            var curClass = this;
            if (inheritedOnly)
                curClass = curClass.BaseClass;

            while (curClass != null)
            {
                functions.AddRange(curClass.Fields);

                curClass = curClass.BaseClass;
            }

            return functions;
        }
    }

    /// <summary>
    /// Represents a class constructor
    /// </summary>
    public class ConstructorDefinition : FunctionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorDefinition class
        /// </summary>
        /// <param name="classDefinition">The class definition this constructor belongs to</param>
        /// <param name="bodyContext">The body context for the constructor</param>
        /// <param name="parameters">The parameters for the constructor</param>
        public ConstructorDefinition(ClassDefinition classDefinition, ZScriptParser.FunctionBodyContext bodyContext, FunctionArgumentDefinition[] parameters)
            : base(classDefinition.Name, bodyContext, parameters)
        {

        }
    }
}