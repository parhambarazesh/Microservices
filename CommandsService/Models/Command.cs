using System.ComponentModel.DataAnnotations;

namespace CommandsService.Models
{
    public class Command
    {
        // We want to return all of these properties back via our CommandReadDto (except the Platform)
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string HowTo { get; set; }
        [Required]
        public string CommandLine { get; set; }
        [Required]
        public int PlatformId { get; set; }
        public Platform Platform { get; set; }
    }
}