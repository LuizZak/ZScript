using System;

namespace ZScript.CodeGeneration.Definitions
{
    /// <summary>
    /// Specifies a frame for a sequence definition
    /// </summary>
    public class SequenceFrameDefinition : FunctionDefinition
    {
        /// <summary>
        /// Gets or sets the range of frames that trigger this sequence
        /// </summary>
        public SequenceFrameRange[] FrameRanges { get; set; }

        /// <summary>
        /// Gets or sets the sequence this sequence frame belongs to
        /// </summary>
        public SequenceDefinition Sequence { get; set; }

        /// <summary>
        /// Initializes a new instance of the SequenceFrameDefinition class
        /// </summary>
        /// <param name="name">A unique identifier for the sequence frame</param>
        /// <param name="bodyContext">The context containing the body of the function</param>
        /// <param name="frameRanges">The range of frames that trigger this sequence</param>
        public SequenceFrameDefinition(string name, ZScriptParser.FunctionBodyContext bodyContext, SequenceFrameRange[] frameRanges)
            : base(name, bodyContext, new FunctionArgumentDefinition[0])
        {
            FrameRanges = frameRanges;
        }
    }

    /// <summary>
    /// Specifies a frame range that triggers a frame from a sequence
    /// </summary>
    public struct SequenceFrameRange : IEquatable<SequenceFrameRange>
    {
        /// <summary>
        /// Whether the frame range of this sequence frame range is relative to the previous range that was read
        /// </summary>
        private readonly bool _isRelative;

        /// <summary>
        /// Whether this sequence frame contains an end frame that specifies a range together with the start frame
        /// </summary>
        private readonly bool _isRanged;

        /// <summary>
        /// The start frame of the sequence
        /// </summary>
        private readonly int _startFrame;

        /// <summary>
        /// The end frame of the sequence
        /// </summary>
        private readonly int _endFrame;

        /// <summary>
        /// Gets a value specifying whether the frame range of this sequence frame range is relative to the previous range that was read
        /// </summary>
        public bool IsRelative
        {
            get { return _isRelative; }
        }

        /// <summary>
        /// Gets a value specifying whether this sequence frame contains an end frame that specifies a range together with the start frame.
        /// </summary>
        public bool IsRanged
        {
            get { return _isRanged; }
        }

        /// <summary>
        /// Gets the start frame of the sequence
        /// </summary>
        public int StartFrame
        {
            get { return _startFrame; }
        }

        /// <summary>
        /// Gets the end frame of the sequence
        /// </summary>
        public int EndFrame
        {
            get { return _endFrame; }
        }

        /// <summary>
        /// Initializes a new SequenceFrameRange struct
        /// </summary>
        /// <param name="isRelative">Whether the frame range of this sequence frame range is relative to the previous range that was read</param>
        /// <param name="frame">The frame this sequence frame range triggers on</param>
        public SequenceFrameRange(bool isRelative, int frame)
        {
            _isRelative = isRelative;
            _isRanged = false;
            _startFrame = frame;
            _endFrame = _startFrame;
        }

        /// <summary>
        /// Initializes a new SequenceFrameRange struct
        /// </summary>
        /// <param name="isRelative">Whether the frame range of this sequence frame range is relative to the previous range that was read</param>
        /// <param name="startFrame">The starting frame of the trigger range for this frame range</param>
        /// <param name="endFrame">The ending frame of the trigger range for this frame range</param>
        public SequenceFrameRange(bool isRelative, int startFrame, int endFrame)
        {
            _isRelative = isRelative;
            _isRanged = true;
            _startFrame = startFrame;
            _endFrame = endFrame;
        }

        #region Equality Members

        public bool Equals(SequenceFrameRange other)
        {
            return _isRelative.Equals(other._isRelative) && _isRanged.Equals(other._isRanged) && _startFrame == other._startFrame && _endFrame == other._endFrame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SequenceFrameRange && Equals((SequenceFrameRange)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _isRelative.GetHashCode();
                hashCode = (hashCode * 397) ^ _isRanged.GetHashCode();
                hashCode = (hashCode * 397) ^ _startFrame;
                hashCode = (hashCode * 397) ^ _endFrame;
                return hashCode;
            }
        }

        public static bool operator==(SequenceFrameRange left, SequenceFrameRange right)
        {
            return left.Equals(right);
        }

        public static bool operator!=(SequenceFrameRange left, SequenceFrameRange right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}