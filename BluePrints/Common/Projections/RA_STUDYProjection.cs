using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class RA_STUDYProjection : BluePrintsProjectionBase<RA_STUDY>
    {
        public IEnumerable<RA_STUDY_DATA> RA_STUDY_DATAS { get; set; }
        public IEnumerable<RA_STUDY_DRAWING> RA_STUDY_DRAWINGS { get; set; }
        public IEnumerable<RA_STUDY_NODE> RA_STUDY_NODES { get; set; }
        public IEnumerable<RA_STUDY_TEAM> RA_STUDY_TEAMS { get; set; }
        public PROJECT PROJECT { get; set; }

        public RA_STUDYProjection(RA_STUDY study, IEnumerable<RA_STUDY_DATA> study_datas, IEnumerable<RA_STUDY_DRAWING> study_drawings, IEnumerable<RA_STUDY_NODE> study_nodes, IEnumerable<RA_STUDY_TEAM> study_teams, IEnumerable<USER> users, PROJECT project)
        {
            PROJECT = project;
            Entity = study;
            RA_STUDY_DRAWINGS = study_drawings;
            RA_STUDY_NODES = study_nodes;

            foreach(RA_STUDY_TEAM study_team in study_teams)
            {
                study_team.USER = users.FirstOrDefault(x => x.GUID == study_team.GUID_USER);
            }

            RA_STUDY_TEAMS = study_teams;
            RA_STUDY_DATAS = study_datas;
        }
    }
}
