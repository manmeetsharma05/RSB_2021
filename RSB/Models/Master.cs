using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Rajya_Sanik_Board.Models
{
    public class Master
    {
        public int Rank_ID { get; set; }
         [Required(ErrorMessage = "Rank type  is required")]
         [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        public string Rank_Type { get; set; }
        public int regt_CorpsID { get; set; }
         [Required(ErrorMessage = "Regement/corps Name is required")]
         [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        public string regt_CorpsType { get; set; }
        public int Scheme_ID { get; set; }
        [Required(ErrorMessage = "Scheme Name is required")]
        
        public string Scheme_Name { get; set; }

        public int Pensionstatus_ID { get; set; }
         [Required(ErrorMessage = "Pension status is required")]
         [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        public string Pensionstatus_Type { get; set; }
         [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public int awardID { get; set; }
         [Required(ErrorMessage = "award Name is required")]
        public string awardName { get; set; }
        public int medical_ID { get; set; }
         [Required(ErrorMessage = "Medical Type is required")]
        public string MedicalCat_Type { get; set; }
        public bool Active { get; set; }
         [Required(ErrorMessage = "Date is required")]
        public DateTime Date_of_Published { get; set; }
    }
}