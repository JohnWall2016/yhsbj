using System;
using System.Collections.Generic;

namespace YHSBJ.SBGLPT
{
    /// <summary>
    ///   省内参保人员查询
    /// </summary>
    public class Sncbrycx : SessionAction
    {
        public Sncbrycx(Session session) : base(session) {}

        MetaDict _metaData;
        public MetaDict MetaData
        {
            get
            {
                if (_metaData == null)
                {
                    S.SendInput(inEnv =>
                    {
                        inEnv.Header.Add("funid", "F00.01.04");
                        inEnv.Body.Add("functionid", "F27.06");
                    });
                    var output = S.GetOutput();

                    var rset = output.Body.Resultset;
                    if (rset.Rows.Count > 0)
                    {
                        var row = rset.Rows[0];
                        if (row.ContainsKey("resultfielden") &&
                            row.ContainsKey("resultfieldcn"))
                        {
                            var en = row["resultfielden"].Split(',');
                            var cn = row["resultfieldcn"].Split(',');
                            _metaData = new MetaDict(en, cn);
                            return _metaData;
                        }
                    }
                    _metaData = new MetaDict();
                }
                return _metaData;
            }
        }

        public List<ResultDict> Search(string id)
        {
            S.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", "F00.01.03");
                inEnv.Body.Add("startrow", "1");
                inEnv.Body.Add("row_count", "-1");
                inEnv.Body.Add("pagesize", "500");
                inEnv.Body.Add("clientsql", $"( aac002 = &apos;{id}&apos;)");
                inEnv.Body.Add("functionid", "F27.06");
            });
            var output = S.GetOutput();

            return output.Body.Resultset.Rows;
        }
    }
}
