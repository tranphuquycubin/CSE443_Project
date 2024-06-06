﻿using System.ComponentModel.DataAnnotations;

namespace EbookApp.Models.DTOs
{
    public class StockDTO
    {
        public int BookId { get; set; }
        [Range(0, int.MaxValue,ErrorMessage ="Quantity must be a non-negative value")]
        public int Quantity {  get; set; }
    }
}
