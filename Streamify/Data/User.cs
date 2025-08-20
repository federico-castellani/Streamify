using System.ComponentModel.DataAnnotations;

namespace Streamify.Data;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    [Required]
    public string Role { get; set; }
    public bool DarkMode { get; set; } = false;
    public string? Name { get; set; }
    public ICollection<WatchHistory> WatchHistoryEntries { get; set; } = new List<WatchHistory>();
}