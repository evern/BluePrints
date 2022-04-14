using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public static class RiskMatrix
    {
        public static Register_RiskRanking? GetRanking(Register_RiskLikelihood? likelihood, Register_RiskConsequence? consequences)
        {
            Register_RiskConsequence? Consequences = consequences;
            Register_RiskLikelihood? Likelihood = likelihood;
            Register_RiskRanking? Ranking = null;

            if (likelihood == Register_RiskLikelihood.High)
            {
                if (consequences == Register_RiskConsequence.Low)
                    Ranking = Register_RiskRanking.Medium;
                else if (consequences == Register_RiskConsequence.Medium)
                    Ranking = Register_RiskRanking.High;
                else
                    Ranking = Register_RiskRanking.High;
            }
            else if(likelihood == Register_RiskLikelihood.Medium)
            {
                if (consequences == Register_RiskConsequence.Low)
                    Ranking = Register_RiskRanking.Low;
                else if (consequences == Register_RiskConsequence.Medium)
                    Ranking = Register_RiskRanking.Medium;
                else
                    Ranking = Register_RiskRanking.High;
            }
            else if (likelihood == Register_RiskLikelihood.Low)
            {
                if (consequences == Register_RiskConsequence.Low)
                    Ranking = Register_RiskRanking.Low;
                else if (consequences == Register_RiskConsequence.Medium)
                    Ranking = Register_RiskRanking.Low;
                else
                    Ranking = Register_RiskRanking.Medium;
            }

            return Ranking;
        }
    }
}
