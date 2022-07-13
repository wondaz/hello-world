using Frame.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var s1 = new Student()
            {
                stuAge = 1,
                stuGrade = "A",
                stuName = "wang"
            };
            var s2 = new Student()
            {
                stuAge = 21,
                stuGrade = "BA",
                stuName = "DAZHI"
            };

            var list = new List<Student>();
            list[0] = s1;
            list[1] = s2;

            DataTable dt = ZConvert.ToDataTable<Student>(list);
            foreach(var r in dt.Rows)
            {

            }
        }
    }
}
