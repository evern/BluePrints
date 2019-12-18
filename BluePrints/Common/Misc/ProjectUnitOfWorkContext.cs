using BluePrints.Data;
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
        public ProjectUnitOfWorkContext(PROJECT project, IPrimeroEntitiesUnitOfWork unitOfWork)
        {
            Project = project;
            PrimeroEntitiesUnitOfWork = unitOfWork;
        }

        public PROJECT Project { get; set; }
        public string ProjectNumber => Project.NUMBER;
        public string OfficeName => Project.OfficeNameForExo;
        public IPrimeroEntitiesUnitOfWork PrimeroEntitiesUnitOfWork { get; set; }
    }

    public class UserIdsAuthorisationContext
    {
        public UserIdsAuthorisationContext(string officeName, int? id)
        {
            OfficeName = officeName;
            Id = id;
        }

        public string OfficeName { get; set; }
        public int? Id { get; set; }
    }
}
