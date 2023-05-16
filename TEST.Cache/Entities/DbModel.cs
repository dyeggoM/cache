using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TEST.Cache.Entities
{
    /// <summary>
    /// Test entity
    /// </summary>
    [Table("CACHE_Entity1")]
    public class DbModel
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }
}
