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
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Defines an object definition that can be instantiated and used in the script
    /// </summary>
    public class ClassDefinition : Definition
    {
        /// <summary>
        /// Dirty flag for the '_classTypeDef' property
        /// </summary>
        private bool _classTypeDirty;

        /// <summary>
        /// Represents the type that defines this class
        /// </summary>
        private readonly ClassTypeDef _classTypeDef;

        /// <summary>
        /// Internal list of fields for this class definition
        /// </summary>
        private readonly List<ClassFieldDefinition> _fields;

        /// <summary>
        /// Internal list of methods for this class definition
        /// </summary>
        private readonly List<MethodDefinition> _methods;

        /// <summary>
        /// Gets or sets the context containing this class definition
        /// </summary>
        public ZScriptParser.ClassDefinitionContext ClassContext { get; set; }

        /// <summary>
        /// Gets the list of fields colelcted in this class definition
        /// </summary>
        public ClassFieldDefinition[] Fields
        {
            get { return _fields.ToArray(); }
        }

        /// <summary>
        /// Gets the list of methods collected in this class definition
        /// </summary>
        public MethodDefinition[] Methods
        {
            get { return _methods.ToArray(); }
        }

        /// <summary>
        /// Gets or sets the public constructor for this class definitions
        /// </summary>
        public ConstructorDefinition PublicConstructor { get; set; }

        /// <summary>
        /// Gets or sets the base class for this class definition
        /// </summary>
        public ClassDefinition BaseClass { get; set; }

        /// <summary>
        /// Gets the type that represents this class
        /// </summary>
        public ClassTypeDef ClassTypeDef
        {
            get
            {
                if (_classTypeDirty)
                    UpdateClassTypeDef();

                return _classTypeDef;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ClassDefinition class
        /// </summary>
        /// <param name="className">The name for this class</param>
        public ClassDefinition(string className)
        {
            Name = className;

            _classTypeDef = new ClassTypeDef(className);

            _fields = new List<ClassFieldDefinition>();
            _methods = new List<MethodDefinition>();

            _classTypeDirty = true;
        }

        /// <summary>
        /// Finishes this ClassDefinition, doing commont routines like creating public parameterless constructors when no constructor is provided, etc.
        /// </summary>
        public void FinishDefinition()
        {
            if (PublicConstructor == null)
            {
                PublicConstructor = new ConstructorDefinition(this, null, new FunctionArgumentDefinition[0]) { ReturnType = _classTypeDef, HasReturnType = true };
            }
        }

        /// <summary>
        /// Adds a new method definition to this class
        /// </summary>
        /// <param name="method">The method to add to this class</param>
        public void AddMethod(MethodDefinition method)
        {
            _methods.Add(method);

            _classTypeDirty = true;
        }

        /// <summary>
        /// Adds a new field definition to this class
        /// </summary>
        /// <param name="field">The field to add to this class</param>
        public void AddField(ClassFieldDefinition field)
        {
            _fields.Add(field);

            _classTypeDirty = true;
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

        /// <summary>
        /// Re-creates the _classTypeDef property
        /// </summary>
        public void UpdateClassTypeDef()
        {
            _classTypeDef.ClearFields();
            _classTypeDef.ClearMethods();

            // Add the fields
            foreach (var field in _fields)
            {
                var fieldType = new TypeFieldDef(field.Name, field.Type);

                _classTypeDef.AddField(fieldType);
            }

            // Add the methods
            foreach (var method in _methods)
            {
                var parameters = new ParameterInfo[method.Parameters.Length];

                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    parameters[i] = new ParameterInfo(method.Parameters[i].Name, method.Parameters[i].Type ?? TypeDef.AnyType, method.Parameters[i].IsVariadic, method.Parameters[i].HasValue);
                }

                var methodType = new TypeMethodDef(method.Name, parameters, method.ReturnType ?? TypeDef.AnyType);

                _classTypeDef.AddMethod(methodType);
            }

            _classTypeDirty = false;
        }
    }

    /// <summary>
    /// Represents a type that describes a class
    /// </summary>
    public class ClassTypeDef : TypeDef
    {
        /// <summary>
        /// Initializes a new instance of the ClassTypeDef class
        /// </summary>
        /// <param name="name">The name for the class type definition</param>
        public ClassTypeDef(string name) : base(name, false)
        {

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
            ReturnType = classDefinition.ClassTypeDef;
        }
    }
}