using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class ProjectUnitOfWorkContext
    {
        public ProjectUnitOfWorkContext(string projectNumber, IPrimeroEntitiesUnitOfWork unitOfWork)
        {
            ProjectNumber = projectNumber;
            PrimeroEntitiesUnitOfWork = unitOfWork;
        }

        public string ProjectNumber { get; set; }
        public IPrimeroEntitiesUnitOfWork PrimeroEntitiesUnitOfWork { get; set; }
    }
}
