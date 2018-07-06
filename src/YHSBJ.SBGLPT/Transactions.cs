using System;
using System.Collections.Generic;

namespace YHSBJ.SBGLPT
{
    /// <summary>
    ///   省内参保人员查询
    /// </summary>
    public class Sncbrycx : SessionActionWithMetaData
    {
        public Sncbrycx(Session session) : base(session, "F00.01.04", "F27.06") {}

        public List<ResultDict> Search(string pid)
        {
            S.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", "F00.01.03");
                inEnv.Body.Add("startrow", "1");
                inEnv.Body.Add("row_count", "-1");
                inEnv.Body.Add("pagesize", "500");
                inEnv.Body.Add("clientsql", $"( aac002 = &apos;{pid}&apos;)");
                inEnv.Body.Add("functionid", "F27.06");
            });
            var output = S.GetOutput();

            return output.Body.Resultset.Rows;
        }
    }

    /// <summary>
    ///   社保机构编码
    /// </summary>
    public class Sbjgbm : SessionAction
    {
        public Sbjgbm(Session s) : base(s)
        {
            FetchData();
        }

        ResultDict _bm2sbjg = new ResultDict();
        ResultDict _sbjg2bm = new ResultDict();

        public void FetchData()
        {
            S.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", "F00.01.02");
                inEnv.Body.Add("functionid", "F28.02"); // F28.01 本级
            });
            var output = S.GetOutput();
            _bm2sbjg.Clear();
            _sbjg2bm.Clear();

            foreach (var row in output.Body.Resultset.Rows)
            {
                if (row.TryGetValue("aab300", out var sbjg) &&
                    row.TryGetValue("aab034", out var bm))
                {
                    _bm2sbjg.Add(bm, sbjg);
                    _sbjg2bm.Add(sbjg, bm);
                }
            }
        }

        public string GetSbjg(string bm)
        {
            if (_bm2sbjg.ContainsKey(bm))
                return _bm2sbjg[bm];
            return "";
        }

        public string GetBm(string sbjg)
        {
            if (_sbjg2bm.ContainsKey(sbjg))
                return _sbjg2bm[sbjg];
            return "";
        }

        public string ToDictString() => _bm2sbjg.ToDictString();
    }

    /// <summary>
    ///   离退休人员查询统计
    /// </summary>
    public class Ltxrycxtj : SessionActionWithMetaData
    {
        public Ltxrycxtj(Session s) : base(s, "F00.01.01", "F27.03") {}

        public List<ResultDict> Search(string pid, string sbjgbm)
        {
            S.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", "F00.01.03");
                inEnv.Body.Add("aab034", sbjgbm);
                inEnv.Body.Add("startrow", "1");
                inEnv.Body.Add("row_count", "-1");
                inEnv.Body.Add("pagesize", "1000");
                inEnv.Body.Add("clientsql", $"( v.aac002 = &apos;{pid}&apos;)");
                inEnv.Body.Add("functionid", "F27.03");
            });
            var output = S.GetOutput();

            return output.Body.Resultset.Rows;
        }
    }

    /// <summary>
    ///   CS参保个人信息查询
    /// </summary>
    public partial class Cscbgrxxcx : SessionAction
    {
        public Cscbgrxxcx(Session s) : base(s)
        {
        }

        public List<ResultDict> Search(string pid, string sbjgmc)
        {
            if (!IsInArea(sbjgmc))
                return new List<ResultDict>();
            //Console.WriteLine("cssearch");
            S.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", "F00.01.03");
                inEnv.Body.Add("startrow", "1");
                inEnv.Body.Add("row_count", "-1");
                inEnv.Body.Add("pagesize", "200");
                inEnv.Body.Add("clientsql", $"( aac002 = &apos;{pid}&apos;)");
                inEnv.Body.Add("functionid", "F27.11.02.01");
            });
            var output = S.GetOutput();
            //Console.WriteLine(output.Body.Resultset.Rows.Count);
            foreach (var row in output.Body.Resultset.Rows)
            {
                var id = row["aac001"];
                S.SendInput(inEnv =>
                {
                    inEnv.Header.Add("funid", "F00.01.03");
                    inEnv.Body.Add("startrow", "1");
                    inEnv.Body.Add("row_count", "-1");
                    inEnv.Body.Add("pagesize", "50");
                    inEnv.Body.Add("clientsql", $"aac001=&apos;{id}&apos;");
                    inEnv.Body.Add("functionid", "F27.11.02.06");
                });
                var subOutput = S.GetOutput();
                row.SubItems = subOutput.Body.Resultset.Rows;
            }
            return output.Body.Resultset.Rows;
        }
    }
}
