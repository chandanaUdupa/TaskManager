using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ProcessDTO
    {
        [Required]
        [Editable(false, AllowInitialValue = true)]
        [Display(Name = "Process Id")]
        public long PID { get; set; }

        [Required]
        [Editable(false, AllowInitialValue = true)]
        [RegularExpression("low|medium|high", ErrorMessage = "The Gender must be either 'low' or 'medium' or 'high' only.")]
        public string Priority { get; set; }

        public string CreatedAt { get; set; }

    }
}
