using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SomeCatIDK.PirateJim.Model;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

public class PirateJimDbContext : DbContext
{
    public DbSet<GuildTimeoutChannel> GuildTimeoutChannels  { get; set; } = null!;
    public DbSet<UserTimeout> UserTimeouts { get; set; } = null!;
    public DbSet<GuildAttachmentChannel> GuildAttachmentChannels { get; set; } = null!;
    public DbSet<GuildRatingChannel> GuildRatingChannels { get; set; } = null!;
    public DbSet<LastMessageChannel> LastMessageChannels { get; set; } = null!;

    private readonly string _dbPath;

    public PirateJimDbContext()
    {
        _dbPath = "./PirateJim.db";
    }

    // Set primary key for UserTimeouts to be both the ChannelID and the UserID since neither are unique by themselves,
    // but are always unique when compounded.
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
    // Set ChannelID as the primary key, as it should be 100% unique in the database.
    [Key]
    public ulong ChannelId { get; set; }
    public int Time { get; set; }
}

public class GuildRatingChannel
{
    [Key]
    public ulong ChannelId { get; set; }
}

public class GuildAttachmentChannel
{
    [Key]
    public ulong ChannelId { get; set; }
}

public class LastMessageChannel
{
    [Key]
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
}