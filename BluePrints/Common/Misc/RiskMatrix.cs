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

            if (likelihood == Register_RiskLikelihood.Certain)
            {
                if (consequences == Register_RiskConsequence.Insignificant)
                    Ranking = Register_RiskRanking.Medium11;
                else if (consequences == Register_RiskConsequence.Minor)
                    Ranking = Register_RiskRanking.High16;
                else if (consequences == Register_RiskConsequence.Moderate)
                    Ranking = Register_RiskRanking.Extreme20;
                else if (consequences == Register_RiskConsequence.Major)
                    Ranking = Register_RiskRanking.Extreme23;
                else
                    Ranking = Register_RiskRanking.Extreme25;
            }
            else if(likelihood == Register_RiskLikelihood.Likely)
            {
                if (consequences == Register_RiskConsequence.Insignificant)
                    Ranking = Register_RiskRanking.Medium7;
                else if (consequences == Register_RiskConsequence.Minor)
                    Ranking = Register_RiskRanking.High12;
                else if (consequences == Register_RiskConsequence.Moderate)
                    Ranking = Register_RiskRanking.High17;
                else if (consequences == Register_RiskConsequence.Major)
                    Ranking = Register_RiskRanking.Extreme21;
                else
                    Ranking = Register_RiskRanking.Extreme24;
            }
            else if (likelihood == Register_RiskLikelihood.Possible)
            {
                if (consequences == Register_RiskConsequence.Insignificant)
                    Ranking = Register_RiskRanking.Low4;
                else if (consequences == Register_RiskConsequence.Minor)
                    Ranking = Register_RiskRanking.Medium8;
                else if (consequences == Register_RiskConsequence.Moderate)
                    Ranking = Register_RiskRanking.High13;
                else if (consequences == Register_RiskConsequence.Major)
                    Ranking = Register_RiskRanking.High18;
                else
                    Ranking = Register_RiskRanking.Extreme22;
            }
            else if (likelihood == Register_RiskLikelihood.Unlikely)
            {
                if (consequences == Register_RiskConsequence.Insignificant)
                    Ranking = Register_RiskRanking.Low2;
                else if (consequences == Register_RiskConsequence.Minor)
                    Ranking = Register_RiskRanking.Low5;
                else if (consequences == Register_RiskConsequence.Moderate)
                    Ranking = Register_RiskRanking.Medium9;
                else if (consequences == Register_RiskConsequence.Major)
                    Ranking = Register_RiskRanking.High14;
                else
                    Ranking = Register_RiskRanking.High19;
            }
            else if (likelihood == Register_RiskLikelihood.Rare)
            {
                if (consequences == Register_RiskConsequence.Insignificant)
                    Ranking = Register_RiskRanking.Low1;
                else if (consequences == Register_RiskConsequence.Minor)
                    Ranking = Register_RiskRanking.Low3;
                else if (consequences == Register_RiskConsequence.Moderate)
                    Ranking = Register_RiskRanking.Low6;
                else if (consequences == Register_RiskConsequence.Major)
                    Ranking = Register_RiskRanking.Medium10;
                else
                    Ranking = Register_RiskRanking.High15;
            }

            return Ranking;
        }
    }
}
