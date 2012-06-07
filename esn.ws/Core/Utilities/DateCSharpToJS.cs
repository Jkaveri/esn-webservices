using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JK.Core.Utilities
{
    /// <summary>
    /// Class convert datetime in c# to jquery ui
    /// </summary>
    public static class DateCSharpToJS
    {
        /// <summary>
        /// dictionary for convert
        /// </summary>
        private static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>
                                                                   {
                                                                       {"d", "d"},
                                                                       {"dd", "dd"},
                                                                       {"ddd", "D"},
                                                                       {"dddd", "DD"},
                                                                       {"M", "m"},
                                                                       {"MM", "mm"},
                                                                       {"MMM", "M"},
                                                                       {"MMMM", "MM"},
                                                                       {"yy", "y"},
                                                                       {"yyyy", "yy"}
                                                                   };
        /// <summary>
        /// Convert input string c# datetime format to js
        /// </summary>
        /// <param name="input">input c# datetime format</param>
        /// <returns>javascript datetime format</returns>
        public static string ConvertToJS(string input)
        {
            var stack = "";
            var output="";
            var old = '\0';
            for (var i = 0; i <= input.Length; i++)
            {
                char inp = (i < input.Length) ? input[i] : '\0';
                if(inp!=old)
                {
                    old = inp;
                    if (stack != "")
                    {
                        if (Dictionary.ContainsKey(stack))
                        {
                            output += Dictionary[stack];
                        }
                        else
                        {
                            output += stack;
                        }
                        if(old!='\0')output += old;
                        stack = "";
                    }
                    else
                    {
                        stack = old.ToString();
                    }
                }else
                {
                    stack += old;
                    
                }
            }
            return output;
        }
    }
}
