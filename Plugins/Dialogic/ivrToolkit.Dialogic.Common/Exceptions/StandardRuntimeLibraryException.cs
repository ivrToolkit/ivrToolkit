using ivrToolkit.Dialogic.Common.DialogicDefs;
using System;
using System.Diagnostics;
using System.Linq;

namespace ivrToolkit.Dialogic.Common.Exceptions
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
