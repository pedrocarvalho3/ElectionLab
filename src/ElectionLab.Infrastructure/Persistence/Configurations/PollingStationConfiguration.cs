using ElectionLab.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectionLab.Infrastructure.Persistence.Configurations;

public class PollingStationConfiguration : IEntityTypeConfiguration<PollingStation>
{
    public void Configure(EntityTypeBuilder<PollingStation> builder)
    {
        builder.ToTable("polling_stations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ElectionId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.OpenedAt)
            .IsRequired(false);

        builder.Property(x => x.ClosedAt)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ElectionId, x.Code })
            .IsUnique();

        builder.HasOne<Election>()
            .WithMany()
            .HasForeignKey(x => x.ElectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
