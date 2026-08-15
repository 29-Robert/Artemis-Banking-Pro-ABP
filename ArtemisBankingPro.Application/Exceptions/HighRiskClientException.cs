using System;

namespace ArtemisBankingPro.Application.Exceptions
{
   
    public class HighRiskClientException : Exception
    {
        public string RiskType { get; }
        public decimal CurrentDebt { get; }
        public decimal ProjectedDebt { get; }
        public decimal AverageDebt { get; }

        public HighRiskClientException(string message, string riskType, decimal currentDebt, decimal projectedDebt, decimal averageDebt)
            : base(message)
        {
            RiskType = riskType;
            CurrentDebt = currentDebt;
            ProjectedDebt = projectedDebt;
            AverageDebt = averageDebt;
        }
    }
}
