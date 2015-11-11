using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ConsoleApplication1
{
    static class Program
    {
        static StreamReader programCode;

        static void DeleteLineComment(ref string str)
        /*удаляет строчные комментарии из строки*/
        {
            int startIndex = str.IndexOf("//");
            if (startIndex != -1)
                str = str.Remove(startIndex);
        }

        static void DeleteTextFromString(ref string str)
        /*удаляет всё что представляется как строка текста*/
        {
            int startInd = str.IndexOf("'", 0), endInd;
            while (startInd != -1)
            {
                endInd = str.IndexOf("'", startInd + 1);
                str = str.Remove(startInd, endInd - startInd + 1);
                startInd = str.IndexOf("'", startInd);
            }
        }

        static void DeleteMultiLineComment(ref string str, ref StreamReader sr)
        /*вырезает многострочные комментарии*/
        {
            int startIndex;
            string commentType = "";
            if ((startIndex = str.IndexOf("{")) != -1)
                commentType = "}";
            else if ((startIndex = str.IndexOf("(*")) != -1)
                commentType = "*)";

            if (startIndex != -1)
            {
                string tempStr = str;
                str = str.Remove(startIndex) + " ";
                while ((startIndex = tempStr.IndexOf(commentType)) == -1)
                {
                    tempStr = sr.ReadLine();
                    DeleteLineComment(ref tempStr);
                }
                str = str + tempStr.Remove(0, startIndex + 1);
            }
        }

        static string GetString(ref StreamReader sr)
        /*возвращает следующую строку кода на Pascal*/
        {
            string result = sr.ReadLine();
            if (result != null)
            {
                DeleteTextFromString(ref result);
                DeleteLineComment(ref result);
                DeleteMultiLineComment(ref result, ref sr);
                result = result + ' ';
            }
            return result;
        }

        static int MetricOfMcCade(ref StreamReader programCode)
        /*выполняет метрику МакКейба и возвращает циклометрическое число МакКейба*/
        {
            int result = 1;

            string str = GetString(ref programCode);
            int beginEndCount = 0;
            bool isCase = false;
            bool wasSemicolon = false;
            while (str != null)
            {
                Regex reg = new Regex(@"(\b[A-Za-z_]\w*\b|:|;)");
                foreach (Match match in reg.Matches(str))  
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "for":
                        case "while":
                        case "until":
                        case "if":
                            result++;
                            break;
                        case "case":
                            isCase = true;
                            break;
                        case "begin":
                            if (isCase)
                                beginEndCount++;
                            break;
                        case "end":
                            if (isCase)
                                beginEndCount--;
                            if (beginEndCount == -1)
                            {
                                isCase = false;
                                beginEndCount = 0;
                            }
                            break;
                        case ";":
                            if (beginEndCount == 0)
                                wasSemicolon = true;
                            break;
                        case ":":
                            if (isCase && wasSemicolon && (beginEndCount == 0))
                            {
                                result++;
                                wasSemicolon = false;
                            }
                            break;
                        case "else":
                            if (isCase && wasSemicolon && (beginEndCount == 0))
                                wasSemicolon = false;
                            break;
                        default:
                            break;
                    }

                str = GetString(ref programCode);
            }
            return result;
        }

        static string GetFileName()
        /*возращает введённое с клавиатуры имя файла, но если он несуществует то возр. null*/
        {
            string fileName = Console.ReadLine();
            if (File.Exists(fileName))
                return fileName;
            else
                return null;
        }

        static void Main(string[] args)
        {
            string fileName;
            Console.WriteLine("Введите полное имя файла:");
            while ((fileName = GetFileName()) == null)
                Console.WriteLine("Файла не найдено!\nВведите полное имя файла:");

            programCode = new StreamReader(fileName);

            Console.WriteLine("Циклометрическое число МакКейба {0}", MetricOfMcCade(ref programCode));

            programCode.Close();
            Console.ReadLine();
        }
    }
}