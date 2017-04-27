namespace BluePrints.P6Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LOCATION")]
    public partial class LOCATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int location_id { get; set; }

        [Required]
        [StringLength(255)]
        public string location_name { get; set; }

        [StringLength(24)]
        public string location_type { get; set; }

        [StringLength(200)]
        public string address_line1 { get; set; }

        [StringLength(200)]
        public string address_line2 { get; set; }

        [StringLength(200)]
        public string address_line3 { get; set; }

        [StringLength(200)]
        public string city_name { get; set; }

        [StringLength(200)]
        public string municipality_name { get; set; }

        [StringLength(200)]
        public string state_name { get; set; }

        [StringLength(2)]
        public string state_code { get; set; }

        [StringLength(200)]
        public string country_name { get; set; }

        [StringLength(3)]
        public string country_code { get; set; }

        [StringLength(20)]
        public string postal_code { get; set; }

        public decimal? longitude { get; set; }

        public decimal? latitude { get; set; }

        [Column(TypeName = "text")]
        public string geo_location { get; set; }

        public DateTime? update_date { get; set; }

        [StringLength(255)]
        public string update_user { get; set; }

        public DateTime? create_date { get; set; }

        [StringLength(255)]
        public string create_user { get; set; }

        public int? delete_session_id { get; set; }

        public DateTime? delete_date { get; set; }
    }
}