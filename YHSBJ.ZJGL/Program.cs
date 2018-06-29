using System;
using System.Collections.Generic;
using System.Linq;

using YHCJB.Util;

namespace YHSBJ.ZJGL
{
    class Program
    {
        static List<int> GetSpansFromEnd(int endYM, int months)
        {
            if (months < 1)
                throw new InvalidOperationException("月数跨度必须大于1");
            var spans = new List<int>(months) { endYM };
            for (int i = 1; i < months; i++)
            {
                endYM = PrevMonth(endYM);
                spans.Insert(0, endYM);
            }
            return spans;
        }

        static List<int> GetSpansFromTo(int fromYM, int toYM)
        {
            if (fromYM > toYM)
                throw new InvalidOperationException("开始年月必须小于结束年月");
            var spans = new List<int> { fromYM };
            while (fromYM < toYM)
            {
                fromYM = NextMonth(fromYM);
                spans.Add(fromYM);
            }
            return spans;
        }

        static void SpansSubtract(List<int> minuend, List<int> subtractor)
        {
            foreach (var sub in subtractor)
            {
                var idx = minuend.IndexOf(sub);
                if (idx != -1)
                    minuend.RemoveAt(idx);
            }
        }

        static int StepMonth(int month, int step)
        {
            int cy = month / 100;
            int cm = month % 100;

            int months = cy * 12 + cm + step;
            cy = months / 12;
            cm = months % 12;
            if (cm == 0)
            {
                cy -= 1;
                cm = 12;
            }
            
            return cy * 100 + cm;
        }

        static int NextMonth(int month)
        {
            //month += 1;
            //if (month % 100 == 13)
            //    month = (month / 100 + 1) * 100 + 1;
            //return month;
            return StepMonth(month, 1);
        }

        static int PrevMonth(int month)
        {
            //month -= 1;
            //if (month % 100 == 0)
            //    month = (month / 100 - 1) * 100 + 12;
            //return month;
            return StepMonth(month, -1);
        }

        static IEnumerable<(int begMonth, int endMonth, int count)> GetSpansList(List<int> spans)
        {
            int? begMonth = null;
            int? preMonth = null;
            int count = 0;
            foreach (var ny in spans)
            {
                if (begMonth == null)
                {
                    begMonth = ny;
                    preMonth = ny;
                    count = 1;
                    continue;
                }
                if (NextMonth((int)preMonth) == ny)
                {
                    preMonth = ny;
                    count += 1;
                    continue;
                }
                else
                {
                    yield return ((int)begMonth, (int)preMonth, count);
                    begMonth = ny;
                    preMonth = ny;
                    count = 1;
                }
            }
            yield return ((int)begMonth, (int)preMonth, count);
        }
        
        static void Main1(string[] args)
        {
            //GetSpansFromTo(200802, 201001).Select((ny, i) => $"{i+1}: {ny}").JoinToString("\n").Println();
            //Environment.Exit(-1);
            
            if (args.Length < 3)
            {
                "使用方法：缴费结束年月 缴费月数 已缴费年月 [已缴费年月...]".Println();
                "    例如：201806 130 201605-201705 201804".Println();
                Environment.Exit(-1);
            }

            var jfjsny = Convert.ToInt32(args[0]);
            var jfys = Convert.ToInt32(args[1]);
            var minuend = GetSpansFromEnd(jfjsny, jfys);

            var subtractor = new List<int>();
            for (var i = 2; i < args.Length; i++)
            {
                var yjfny = args[i];
                var nys = yjfny.Split("-");
                if (nys.Length > 1)
                {
                    var ksny = Convert.ToInt32(nys[0]);
                    var jsny = Convert.ToInt32(nys[1]);
                    subtractor.AddRange(GetSpansFromTo(ksny, jsny));
                } else
                {
                    var ksny = Convert.ToInt32(nys[0]);
                    subtractor.Add(ksny);
                }
            }

            SpansSubtract(minuend, subtractor);

            var spans = GetSpansList(minuend);

            var total = 0;
            for (var i = 0; i < spans.Count(); i++)
            {
                var span = spans.ElementAt(i);
                $"{i+1, 3}: {span.begMonth} - {span.endMonth} {span.count, 4}".Println();
                total += span.count;
            }
            $"{total, 25}".Println();
        }

        static void Main(string[] args)
        {
            //$"{StepMonth(201812, 12)}".Println();
            //Environment.Exit(-1);
            
            if (args.Length < 2)
            {
                "使用方法：缴费结束年月 测算表格路径".Println();
                Environment.Exit(-1);
            }

            var jfjsny = Convert.ToInt32(args[0]);
            var workbook = ExcelExtension.LoadExcel(args[1]);
            var sheet = workbook.GetSheetAt(0);

            for (var irow = 4; irow <= sheet.LastRowNum; irow++)
            {
                var memo = sheet.Cell(irow, 15).CellValue()?.Trim() ?? "";
                //if (memo == "重复")
                //    continue;
                
                var jfys = (int)sheet.Cell(irow, 8).NumericCellValue;
                var ptys = (int)sheet.Cell(irow, 9).NumericCellValue;
            
                var minuend = GetSpansFromEnd(jfjsny, jfys);

                var subtractor = new List<int>();
                var wdcb = sheet.Cell(irow, 14).CellValue().Trim();
                if (wdcb != null && wdcb != "")
                {
                    var wdcbs = wdcb.Split("|");
                    for (var i = 0; i < wdcbs.Length; i++)
                    {
                        var yjfny = wdcbs[i];
                        var nys = yjfny.Split("-");
                        if (nys.Length > 1)
                        {
                            var ksny = Convert.ToInt32(nys[0]);
                            var jsny = Convert.ToInt32(nys[1]);
                            subtractor.AddRange(GetSpansFromTo(ksny, jsny));
                        }
                        else
                        {
                            var ksny = Convert.ToInt32(nys[0]);
                            subtractor.Add(ksny);
                        }
                    }
                }

                SpansSubtract(minuend, subtractor);

                var spans = GetSpansList(minuend);

                var total = 0;
                var ptSpans = new List<string>();
                var grSpans = new List<string>();
                var inPtSpans = true;
                for (var i = 0; i < spans.Count(); i++)
                {
                    var span = spans.ElementAt(i);
                    var sspan = $"{span.begMonth}-{span.endMonth}[{span.count}]";
                    total += span.count;
                    if (inPtSpans && total >= ptys)
                    {
                        if (total == ptys)
                            ptSpans.Add(sspan);
                        else
                        {
                            int delta = total - ptys;
                            ptSpans.Add($"{span.begMonth}-{StepMonth(span.endMonth, -delta)}[{span.count-delta}]");
                            grSpans.Add($"{StepMonth(span.endMonth, -delta+1)}-{span.endMonth}[{delta}]");
                        }
                        inPtSpans = false;
                        continue;
                    }
                    
                    if (inPtSpans)
                        ptSpans.Add(sspan);
                    else
                        grSpans.Add(sspan);
                }
                var sptSpans = ptSpans.JoinToString("|");
                if (total < ptys)
                    sheet.Cell(irow, 17).SetValue($"补贴月数不足{sptSpans}[{total}<{ptys}]");
                else
                {
                    sheet.Cell(irow, 16).SetValue(grSpans.JoinToString("|"));
                    sheet.Cell(irow, 17).SetValue(sptSpans);   
                }
            }

            workbook.Save(Utils.FileNameAppend(args[1], ".new"));
            workbook.Close();
        }
    }
}
