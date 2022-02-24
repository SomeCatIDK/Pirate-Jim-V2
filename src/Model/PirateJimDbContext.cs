using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SomeCatIDK.PirateJim.Model;

public class PirateJimDbContext : DbContext
{
    public DbSet<GuildTimeoutChannel> GuildTimeoutChannels  { get; set; } = null!;
    public DbSet<UserTimeout> UserTimeouts { get; set; } = null!;

    private readonly string _dbPath;

    public PirateJimDbContext()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _dbPath = Path.Join(path, "PirateJim.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<UserTimeout>().HasKey(x => new { x.ChannelId, x.UserId });

    protected override void OnConfiguring(DbContextOptionsBuilder options) 
        => options.UseSqlite($"Data Source={_dbPath}");
}

public class UserTimeout
{
    public ulong ChannelId { get; set; }
    public ulong UserId { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class GuildTimeoutChannel
{
    [Key]
    public ulong ChannelId { get; set; }
    public int Time { get; set; }
}