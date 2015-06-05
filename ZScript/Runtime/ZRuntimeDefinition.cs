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
using ZScript.Elements;
using ZScript.Elements.ValueHolding;

namespace ZScript.Runtime
{
    /// <summary>
    /// Represents a runtime definition that can be instantiated into a ZRuntime
    /// </summary>
    public class ZRuntimeDefinition
    {
        /// <summary>
        /// The list of function definitions
        /// </summary>
        private readonly List<ZFunction> _functionDefinitions;

        /// <summary>
        /// The list of export function definitions
        /// </summary>
        private readonly List<ZExportFunction> _exportFunctionDefinitions;

        /// <summary>
        /// The list of closure definitions
        /// </summary>
        private readonly List<ZClosureFunction> _closureDefinitions;

        /// <summary>
        /// The list of global variable definitions
        /// </summary>
        private readonly List<GlobalVariable> _globalVariableDefinitions;

        /// <summary>
        /// The list of class definitions
        /// </summary>
        private readonly List<ZClass> _classDefinitions;

        /// <summary>
        /// Gets an array of all the function definitions stored in this ZRuntimeDefinition
        /// </summary>
        public ZFunction[] ZFunctionDefinitions => _functionDefinitions.ToArray();

        /// <summary>
        /// Gets an array of all the export function definitions stored in this ZRuntimeDefinition
        /// </summary>
        public ZExportFunction[] ZExportFunctionDefinitions => _exportFunctionDefinitions.ToArray();

        /// <summary>
        /// Gets an array of all the closure definitions stored in this ZRuntimeDefinition
        /// </summary>
        public ZClosureFunction[] ZClosureFunctionDefinitions => _closureDefinitions.ToArray();

        /// <summary>
        /// Gets an array of all the global variable definitions stored in this ZRuntimeDefinition
        /// </summary>
        public GlobalVariable[] GlobalVariableDefinitions => _globalVariableDefinitions.ToArray();

        /// <summary>
        /// Gets an array of all the class definitions stored in this ZRuntimeDefinition
        /// </summary>
        public ZClass[] ClassDefinitions => _classDefinitions.ToArray();

        /// <summary>
        /// Initializes a new instance of the ZRuntimeDefinition class
        /// </summary>
        public ZRuntimeDefinition()
        {
            _functionDefinitions = new List<ZFunction>();
            _closureDefinitions = new List<ZClosureFunction>();
            _exportFunctionDefinitions = new List<ZExportFunction>();
            _globalVariableDefinitions = new List<GlobalVariable>();
            _classDefinitions = new List<ZClass>();
        }

        /// <summary>
        /// Adds a new function definition to this ZRuntimeDefinition calss
        /// </summary>
        /// <param name="def">A valid FunctionDef</param>
        public void AddFunctionDef(ZFunction def)
        {
            _functionDefinitions.Add(def);
        }

        /// <summary>
        /// Adds a range of new function definitions to this ZRuntimeDefinition calss
        /// </summary>
        /// <param name="defs">A valid enumerable if ZFunction values</param>
        public void AddFunctionDefs(IEnumerable<ZFunction> defs)
        {
            foreach (var function in defs)
            {
                AddFunctionDef(function);
            }
        }

        /// <summary>
        /// Adds a range of new export function definitions to this ZRuntimeDefinition calss
        /// </summary>
        /// <param name="defs">A valid enumerable of ZExportFunction values</param>
        public void AddExportFunctionDefs(IEnumerable<ZExportFunction> defs)
        {
            _exportFunctionDefinitions.AddRange(defs);
        }

        /// <summary>
        /// Adds a range of new closure definitions to this ZRuntimeDefinition calss
        /// </summary>
        /// <param name="defs">A valid enumerable of ZClosureFunction values</param>
        public void AddClosurenDefs(IEnumerable<ZClosureFunction> defs)
        {
            _closureDefinitions.AddRange(defs);
        }

        /// <summary>
        /// Adds a new global variable definition to this ZRuntimeDefinition class
        /// </summary>
        /// <param name="definition">A valid global variable definition</param>
        public void AddGlobalVariable(GlobalVariable definition)
        {
            _globalVariableDefinitions.Add(definition);
        }

        /// <summary>
        /// Adds a range of global variable definitions to this ZRuntimeDefinition class
        /// </summary>
        /// <param name="definitions">A valid enumerable of global variable definitions</param>
        public void AddGlobalVariables(IEnumerable<GlobalVariable> definitions)
        {
            foreach (var definition in definitions)
            {
                AddGlobalVariable(definition);
            }
        }

        /// <summary>
        /// Adds a range of class definitions to this ZRuntimeDefinition class
        /// </summary>
        /// <param name="definitions">A valid enumerable of class definitions</param>
        public void AddClassDefinitions(IEnumerable<ZClass> definitions)
        {
            _classDefinitions.AddRange(definitions);
        }

        /// <summary>
        /// Returns an array containing all of the ZFunctions defined in this ZRuntimeDefinition.
        /// The order of the representation of the functions is importante, since it is liked to the address described
        /// in the global function references in the VM tokens
        /// </summary>
        /// <returns>An array containing all of the ZFunctions defined in this ZRuntimeDefinition</returns>
        public ZFunction[] GetFunctions()
        {
            return _functionDefinitions.Concat(_exportFunctionDefinitions).Concat(_closureDefinitions).ToArray();
        }
    }
}