﻿namespace EbookApp.Models.DTOs
{
    public class OrderDetailModelDTO
    {
        public string DivId { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
