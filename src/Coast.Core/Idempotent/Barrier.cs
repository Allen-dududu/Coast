using System;

namespace Coast.Core.Barrier
{
    public class Barrier
    {
        public int Id { get; set; }

        public int TransactionType { get; set; }

        public long GlobalSagaId { get; set; }

        public long SagaStepId { get; set; }

        public int SagaStepType { get; set; }

        public DateTime CreateTime { get; set; }
    }
}