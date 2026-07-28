using NetCord.Hosting.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gitbot2.Source.Utils
{
    internal class Tokenizer
    {
        private readonly string path;

        private List<(bool complete,string task)> Lines;
        private List<string> lines;
        public Tokenizer(string file)
        {
            if (!Path.Exists(file))
            {
                return;
            }

            path = file;

            
        }

        public async Task<List<(bool,string)>> GetTokensAsync()
        {
            await Tokenize();  // initiate Lines & lines first

            return Lines;

        }

        public async Task<List<string>> GetLines()
        {

            await Tokenize(); // initiate Lines & lines first

            return lines;
        }



        private async Task Tokenize()
        {
            string[] _Lines = await File.ReadAllLinesAsync(path);
            List<(bool, string)> Temp = new();

            _Lines.ToList().ForEach((line) =>
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                string[] Token = Utility.TokenizeLine(line); // '[' ']' and "Task", so its 3 by default

                if(Token.Length > 2)
                {
                    Temp.Add((false, line));
                }
                else if(Token.Length == 2)
                {
                    Temp.Add((true, line));
                }
                else
                {
                    throw new Exception($"Invalid line {line}");
                }
            });


            Lines = new(Temp);
            lines = new(_Lines);

        }
    }
}
