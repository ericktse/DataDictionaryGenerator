using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DataDictionaryGenerator.Core
{
    public class HtmlConvertor
    {
        private static readonly char REPLACE_START_FLAG = '{';
        private static readonly char REPLACE_END_FLAG = '}';

        private static Hashtable _mapParams;
        private static string _inputStr;
        private static StringBuilder _outputStr;
        private static int _currentIndex = -1;
        private static int _lastIndex;
        private static char _currentChar;

        public static string ParseHtml(string input, Hashtable param)
        {
            _outputStr = new StringBuilder();
            _inputStr = input;

            _currentIndex = -1;
            _lastIndex = input.Length;

            _mapParams = param;

            while (NextChar())
            {
                if (_currentChar == REPLACE_START_FLAG && input[_currentIndex + 1] == REPLACE_START_FLAG)
                {
                    // "{{"  end of replace is "}}"
                    ReplaceParameter();
                }
                else
                {
                    _outputStr.Append(_currentChar);
                }
            }

            return _outputStr.ToString();
        }

        /// <summary>
        /// get the next char from input string
        /// </summary>
        /// <returns></returns>
        private static bool NextChar()
        {
            _currentIndex++;

            if (_currentIndex < _lastIndex)
            {
                _currentChar = _inputStr[_currentIndex];
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// replace the Parameter
        /// </summary>
        private static void ReplaceParameter()
        {

            int start = _currentIndex + 2;

            while (NextChar())
            {
                if (_currentChar == REPLACE_END_FLAG)
                {
                    string property = _inputStr.Substring(start, _currentIndex - start);

                    var marches = Regex.Matches(property, @"\[([^]]*)\]");
                    var name = marches[0].Value.Substring(1, marches[0].Value.Length - 2);
                    var value = _mapParams[name];

                    if (value is List<Hashtable>)
                    {
                        var html = marches[1].Value.Substring(1, marches[1].Value.Length - 2);

                        var list = value as List<Hashtable>;
                        foreach (var hash in list)
                        {
                            var tempHtml = html;
                            foreach (DictionaryEntry val in hash)
                            {
                                tempHtml = tempHtml.Replace($"({val.Key})", val.Value.ToString());
                            }
                            _outputStr.Append(tempHtml);
                        }

                    }
                    else
                    {
                        _outputStr.Append(value);
                    }

                    _currentIndex++;    // next char is '}', so skip it !

                    return;
                }
            }
        }
    }
}
