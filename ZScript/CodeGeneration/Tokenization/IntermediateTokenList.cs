using System.Collections;
using System.Collections.Generic;

using ZScript.Elements;

namespace ZScript.CodeGeneration.Tokenization
{
    /// <summary>
    /// Toke list containing tokens in an intermediate format that can be compiled into a TokenList that can be executed by a Function VM
    /// </summary>
    public class IntermediateTokenList : IList<Token>
    {
        /// <summary>
        /// The internal representation of the token list
        /// </summary>
        private readonly List<Token> _tokens; 

        /// <summary>
        /// Initializes a new instance of the IntermediateTokenList class
        /// </summary>
        public IntermediateTokenList()
        {
            _tokens = new List<Token>();
        }

        /// <summary>
        /// Initializes a new instance of the IntermediateTokenList class with a starting list of tokens
        /// </summary>
        /// <param name="tokens">An enumerable of tokens to add to this intermediate token list</param>
        public IntermediateTokenList(IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
        }

        /// <summary>
        /// Adds a range of tokens to the end of this intermediate token list
        /// </summary>
        /// <param name="tokens">A range of tokens to add to this intermediate token list</param>
        public void AddRange(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                Add(token);
            }
        }

        #region IList implementation

        public IEnumerator<Token> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Token item)
        {
            _tokens.Add(item);
        }

        public void Clear()
        {
            _tokens.Clear();
        }

        public bool Contains(Token item)
        {
            return _tokens.Contains(item);
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            _tokens.CopyTo(array, arrayIndex);
        }

        public bool Remove(Token item)
        {
            return _tokens.Remove(item);
        }

        public int Count
        {
            get { return _tokens.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(Token item)
        {
            return _tokens.IndexOf(item);
        }

        public void Insert(int index, Token item)
        {
            _tokens.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _tokens.RemoveAt(index);
        }

        public Token this[int index]
        {
            get { return _tokens[index]; }
            set { _tokens[index] = value; }
        }

        #endregion
    }
}