using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable disable

namespace Api.Model
{
    public partial class POI
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public long CityID { get; set; }

        [Required]
        public long CategoryID { get; set; }

        public virtual City City { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }

        public POI()
        {
            Entries = new HashSet<Entry>();
        }
    }

    public class POIDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
        public long? AvgRating { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string Category { get; set; }
        public List<EntryDto> Entries { get; set; }
    }

    public class FilterPOI
    {
        public string Name { get; set; }
        public int Offset { get; set; }
        public int Amount { get; set; } = 10;
        public string Country { get; set; }
        public string City { get; set; }
        public string Category { get; set; }


        [EnumDataType(typeof(Sort))]
        public Sort Sort { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Sort
    {
        Name,
        City,
        Country,
        Category
    }
}
