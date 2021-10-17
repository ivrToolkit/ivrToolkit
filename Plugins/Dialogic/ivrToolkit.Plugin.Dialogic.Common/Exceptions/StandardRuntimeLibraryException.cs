using System;
using System.Linq;

namespace ivrToolkit.Plugin.Dialogic.Common.Exceptions
{
    public class StandardRuntimeLibraryException : Exception
    {

        public StandardRuntimeLibraryException(string message) : base(message)
        {
        }

        public override string StackTrace
        {
            get
            {
                var relevantStackFrames =
                    base.StackTrace
                        .Split('\n')
                        .Skip(1)
                        .ToArray();
                return string.Join("\n", relevantStackFrames);
            }
        }
    }
}
