namespace Coast.Core.Barrier
{
    using System;

    public class Barrier
    {
        public int Id { get; set; }

        public int TransactionType { get; set; }

        /// <summary>
        /// global id.
        /// maybe Saga or TCC.
        /// </summary>
        public long CorrelationId { get; set; }

        public long StepId { get; set; }

        public int StepType { get; set; }

        public DateTime CreateTime { get; set; }
    }
}