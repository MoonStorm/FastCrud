using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud
{
    public interface ISqlBuilder
    {
        string GetTableName(string alias = null);

        string ConstructKeysWhereClause(string alias = null);

        string ConstructColumnEnumerationForSelect(string alias = null);

        string ConstructColumnEnumerationForInsert();

        string ConstructParamEnumerationForInsert();

        string ConstructUpdateClause(string alias = null);
    }
}
