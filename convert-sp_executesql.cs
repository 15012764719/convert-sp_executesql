using System;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    /*
    Converts parameterized dynamic SQL from SQL Profiler to normal T-SQL. Usage example: first copy the dynamic sql to clipboard, then run following command:
      gnuwin32\bin\getclip.exe | convert-sp_executesql.exe | sqlformatter.exe | clip
      
      sqlformatter.exe can be found at http://architectshack.com/PoorMansTSqlFormatter.ashx
    */
    [STAThreadAttribute]
    static void Main()
    {
        var re = new Regex(@"exec*\s*sp_executesql\s+N'([\s\S]*)',\s*N'(@[\s\S]*?)',\s*([\s\S]*)", RegexOptions.IgnoreCase); // 1: the sql, 2: the declare, 3: the setting

        var rew = new Regex(@"exec*\s*sys.sp_executesql\s+N'([\s\S]*)',\s*N'(@[\s\S]*?)',\s*([\s\S]*)", RegexOptions.IgnoreCase); // 1: the sql, 2: the declare, 3: the setting

        //var input = Console.In.ReadToEnd();



        //重 剪切板中 获得 要处理的 参数化 SQL
       var input = Clipboard.GetText();

       


        StringBuilder builder = new StringBuilder();


        //string[] batch = input.Split(new[] { "Go", "go", "GO" }, StringSplitOptions.None);


        //使用 正则表达式 拆分 批量操作
        //Regex regexs = new Regex(@"Go$",RegexOptions.Multiline|RegexOptions.Singleline|RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);




        var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.CultureInvariant;



        Regex regexs = new Regex(@"\bgo\b", options);


        string[] batch = regexs.Split(input);


        

        foreach (var item in batch)
        {
            var match = re.Match(item);

            if (!match.Success)
            {
                match = rew.Match(item);
            }

            if (match.Success)
            {


                var sql = match.Groups[1].Value.Replace("''", "'");
                var declare = match.Groups[2].Value;
                var setting = match.Groups[3].Value + ',';

                // to deal with comma or single quote in variable values, we can use the variable name to split
                var re2 = new Regex(@"@[\s\S]*?\s*=");
                var variables = re2.Matches(setting).Cast<Match>().Select(m => m.Value).ToArray();
                var values = re2.Split(setting).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].Trim();
                }


                builder.AppendFormat("BEGIN\nDECLARE {0};\n", declare);

                Console.WriteLine("BEGIN\nDECLARE {0};", declare);
                for (int i = 0; i < variables.Length; i++)
                {
                    builder.AppendFormat("SET {0}{1};\n", variables[i], values[i].Substring(0, values[i].Length - 1));
                    Console.WriteLine("SET {0}{1};", variables[i], values[i].Substring(0, values[i].Length - 1));
                }
                Console.WriteLine("{0}\nEND", sql);
                builder.AppendFormat("{0}\nEND\n", sql);

                builder.AppendFormat("{0}\n", "Go");
            }
        }

        Clipboard.SetText(builder.ToString());


    }
}
