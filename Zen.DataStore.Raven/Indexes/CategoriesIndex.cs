/*using Palantir.DataModel.ReferencedData;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Linq.Indexing;
using Raven.Abstractions.Indexing;

namespace Palantir.DataModel.Raven.Indexes
{
    public class CategoriesIndex : AbstractIndexCreationTask<Category>
    {
        public CategoriesIndex()
        {
            Map = categories => from category in categories
                                          select new
                                          {
                                              ParentGuid = category.ParentGuid,
                                              //AdditionalFields_link = category.AdditionalFields.link,
                                              FullPath = category.FullPath,
                                              Guid = category.Guid,
                                              data = category.data,
                                              Id = category.Id
                                          };
			Analyze(x => x.data, "WhitespaceAnalyzer");
        }
    }
}*/

