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
using ZScript.Runtime;
using ZScript.Runtime.Typing.Elements;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Defines an object definition that can be instantiated and used in the script
    /// </summary>
    public class ClassDefinition : TypeContainerDefinition
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
        public override TypeFieldDefinition[] Fields
        {
            get { return fields.ToArray(); }
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

            fields = new List<TypeFieldDefinition>();
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

            PublicConstructor.Class = this;
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
        public override void AddField(TypeFieldDefinition field)
        {
            fields.Add(field);

            _classTypeDirty = true;
        }

        /// <summary>
        /// Returns a list of all the methods inherited and defined by this class definition
        /// </summary>
        /// <param name="attributes">The attributes to use when searching the members to fetch</param>
        /// <returns>A list of all the methods inherited and defined by this given class definition</returns>
        public List<MethodDefinition> GetAllMethods(TypeMemberAttribute attributes = TypeMemberAttribute.CompleteInheritance)
        {
            var methods = new List<MethodDefinition>();

            var curClass = this;
            if (!attributes.HasFlag(TypeMemberAttribute.Defined))
                curClass = curClass.BaseClass;

            while (curClass != null)
            {
                methods.AddRange(curClass.Methods);

                if (!attributes.HasFlag(TypeMemberAttribute.Inherited))
                    break;

                curClass = curClass.BaseClass;
            }

            return methods;
        }

        /// <summary>
        /// Returns a list of all the fields inherited and defined by this class definition
        /// </summary>
        /// <param name="attributes">The attributes to use when searching the members to fetch</param>
        /// <returns>A list of all the fields inherited and defined by this given class definition</returns>
        public override List<TypeFieldDefinition> GetAllFields(TypeMemberAttribute attributes = TypeMemberAttribute.CompleteInheritance)
        {
            var fields = new List<TypeFieldDefinition>();

            var curClass = this;
            if (!attributes.HasFlag(TypeMemberAttribute.Defined))
                curClass = curClass.BaseClass;

            while (curClass != null)
            {
                fields.AddRange(curClass.Fields);

                if (!attributes.HasFlag(TypeMemberAttribute.Inherited))
                    break;

                curClass = curClass.BaseClass;
            }

            return fields;
        }

        /// <summary>
        /// Re-creates the _classTypeDef property
        /// </summary>
        public void UpdateClassTypeDef()
        {
            _classTypeDef.ClearFields();
            _classTypeDef.ClearMethods();

            // Set base type
            _classTypeDef.BaseType = (BaseClass == null ? null : BaseClass._classTypeDef);

            // Add the fields
            foreach (var field in fields)
            {
                var fieldType = new TypeFieldDef(field.Name, field.Type, field.IsConstant);

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
    public class ClassTypeDef : NativeTypeDef
    {
        /// <summary>
        /// Gets or sets the base type for this ClassTypeDef
        /// </summary>
        public TypeDef BaseType
        {
            get { return baseType; }
            set { baseType = value; }
        }

        /// <summary>
        /// The type for the constructor pointed by this ClassTypeDef
        /// </summary>
        public CallableTypeDef ConstructorType;

        /// <summary>
        /// Initializes a new instance of the ClassTypeDef class
        /// </summary>
        /// <param name="name">The name for the class type definition</param>
        public ClassTypeDef(string name)
            : base(typeof(ZClassInstance), name)
        {

        }
    }

    /// <summary>
    /// Represents a class constructor
    /// </summary>
    public class ConstructorDefinition : MethodDefinition
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