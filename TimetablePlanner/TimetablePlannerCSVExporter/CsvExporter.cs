using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TimetablePlanner;

namespace TimetablePlannerCSVExporter
{
    public class CsvExporter
    {
        private static void Export(short[, ,] dataToExport, Block[] blocks, Course[] courses, string title, string filePath)
        {
            //output[col,row]
            string[,] output = new string[dataToExport.GetLength(1) + 1, dataToExport.GetLength(2) + 2];

            //Day/Block-frame for the data
            output[0, 0] = title;
            for (int day = 0; day < dataToExport.GetLength(1); day++)
            {
                output[day + 1, 1] = Enum.GetName(typeof(DayOfWeek), day + 1);
            }
            for (int block = 0; block < dataToExport.GetLength(2); block++)
            {
                output[0, block] = blocks[block].Start.ToShortTimeString() + " - " + blocks[block].End.ToShortTimeString();
            }

            //data
            for (int column = 1; column < dataToExport.GetLength(1) + 1; column++)
            {
                for (int row = 2; row < dataToExport.GetLength(2) + 2; row++)
                {

                }
            }

            //string[][] output = new string[][]{  
            //        new string[]{"Col 1 Row 1", "Col 2 Row 1", "Col 3 Row 1"},  
            //        new string[]{"Col1 Row 2", "Col2 Row 2", "Col3 Row 2"}  
            //    };


            StringBuilder sb = new StringBuilder();
            for (int column = 0; column < output.GetLength(0); column++)
            {
                for (int row = 0; row < output.GetLength(1); row++)
                {
                    sb.Append("," + output[column, row]);
                }
                sb.AppendLine();
                //sb.AppendLine(string.Join(",", output[column]));
            }
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
