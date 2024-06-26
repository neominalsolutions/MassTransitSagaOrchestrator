﻿using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachine.Service.Instruments
{
    /// <summary>
    /// OrderStateInstance : Her bir sipariş eklendiğinde(tetikleyici event geldiğinde)
    /// bu siparişe karşılık Saga State Machine'de tutulacak olan satırı Order State Instance
    /// olarak tarif etmekteyiz.
    /// </summary>
    public class OrderStateInstance : SagaStateMachineInstance
    {
        /// <summary>
        /// Her bir State Instance ait bilgilerin tutulacağı Entity
        /// </summary>
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } // her bir asenkron işlemdeki state bilgisi
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
