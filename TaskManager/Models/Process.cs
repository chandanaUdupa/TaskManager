using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class Process
    {
        [Key]
        [Editable(false, AllowInitialValue = true)]
        public long PID { get; set; }

        [Required]
        [Editable(false, AllowInitialValue = true)]
        public Priority Priority { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public enum Priority
    {
        low = 1,
        medium = 2,
        high = 3
    }
}
